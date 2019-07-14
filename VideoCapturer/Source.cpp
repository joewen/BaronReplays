#pragma comment(lib, "Psapi.lib")
#pragma comment(lib, "d3d9.lib")
#pragma comment(lib, "Ws2_32.lib")
#define _CRT_SECURE_NO_DEPRECATE 
#include <iostream>
#include <Windows.h>
#include <Psapi.h>
#include <d3d9.h>
#include <tchar.h>

#include "Sender.h"
#include "detours.h"

#define ENDSCENE_FUNCTION_ORDER 42

typedef int (__stdcall* EndSceneFunctionPtr)(LPDIRECT3DDEVICE9 Device);


struct VideoFormat
{
	UINT32 width;
	UINT32 height;
	char *bits;
};

const LPCWSTR tempWindowClassName = _T("BaronReplaysWindow");
HINSTANCE processInstance;


EndSceneFunctionPtr oldEndScene;

LPDIRECT3DDEVICE9 device;
IDirect3DSurface9 *surface;
LPDIRECT3DSURFACE9 targetSurface;
D3DSURFACE_DESC surfaceInfo; 
LPDWORD sendThreadID;
char *bits;
VideoFormat videoFormat;
bool formatCreated;



void InitPointers()
{
	oldEndScene = nullptr;
	device = nullptr;
	surface = nullptr;
	formatCreated = false;
}

void DebugMessage(const char* name,void* val)
{
	char c[100];
	std::sprintf(c, name,val);
	OutputDebugStringA(c);
}

void DebugMessage(const char* msg)
{
	OutputDebugStringA(msg);
}

bool SendFormat()
{
	Sender *formatSender = new Sender();
	OutputDebugStringA("Connecting to BaronReplays...");
	bool isConnected = formatSender->Connect();
	bool formatSent = false;
	if(isConnected)
	{
		OutputDebugStringA("Send format to BaronReplays");
		formatSent = (formatSender->SendData((char*)(&videoFormat),sizeof(VideoFormat)) == sizeof(VideoFormat));
		formatSender->Disconnect();
	}
	delete formatSender;
	return formatSent;
}

void CreateVideoFormatData()
{
	OutputDebugStringA("CreateVideoFormatData");
	videoFormat.width = surfaceInfo.Width;
	videoFormat.height = surfaceInfo.Height;
	videoFormat.bits = bits;
	formatCreated = true;
}

void RegisterWindowClass()
{
	WNDCLASSEX wc;
	ZeroMemory(&wc, sizeof(WNDCLASSEX));

	// fill in the struct with the needed information
	wc.cbSize = sizeof(WNDCLASSEX);
	wc.style = CS_HREDRAW | CS_VREDRAW;
	wc.lpfnWndProc = DefWindowProc;
	wc.hInstance = processInstance;
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = NULL;
	wc.lpszClassName = tempWindowClassName;

	// register the window class
	RegisterClassEx(&wc);
}

HWND CreateTempWindow()
{
	RegisterWindowClass();
	HWND hWnd = CreateWindowEx(NULL,
		tempWindowClassName,    // name of the window class
		tempWindowClassName,   // title of the window
		WS_OVERLAPPEDWINDOW,    // window style
		0,    // x-position of the window
		0,    // y-position of the window
		100,    // width of the window
		100,    // height of the window
		NULL,    // we have no parent window, NULL
		NULL,    // we aren't using menus, NULL
		processInstance,    // application handle
		NULL);    // used with multiple windows, NULL

	return hWnd;
	// create the window and use
}

bool InitTargetSurface(LPDIRECT3DDEVICE9 Device)
{
	OutputDebugStringA("InitTargetSurface");
	static int counter = 0;
	device = Device;

	if(Device->GetBackBuffer(0,0, D3DBACKBUFFER_TYPE_MONO, &surface)!= D3D_OK)
	{
		DebugMessage("GetBackBuffer Error");
	}

	if (FAILED(surface->GetDesc(&surfaceInfo)))
	{
		DebugMessage("GetDesc Error");
	}

	if(FAILED(Device->CreateOffscreenPlainSurface(surfaceInfo.Width,surfaceInfo.Height,surfaceInfo.Format,D3DPOOL_SYSTEMMEM, &targetSurface,NULL)))
	{
		DebugMessage("CreateOffscreenPlainSurface Error");
	}
	bits = new char[surfaceInfo.Width * surfaceInfo.Height * 4];
	CreateVideoFormatData();
	return true;
}

int WINAPI newEndScene(LPDIRECT3DDEVICE9 Device)
{
	if(device == nullptr)
	{
		InitTargetSurface(Device);
	}

	if(FAILED(Device->GetRenderTargetData(surface,targetSurface)))
	{
		DebugMessage("GetRenderTargetData Error");
	}

	D3DLOCKED_RECT lockedRect;
	if (targetSurface->LockRect(&lockedRect, NULL, D3DLOCK_READONLY) == D3D_OK)
		memcpy(bits, lockedRect.pBits, lockedRect.Pitch* surfaceInfo.Height);
	else
		DebugMessage("Lock Error");
	targetSurface->UnlockRect();

	return oldEndScene(Device);
}


void HookEndScene()
{
	OutputDebugStringA("FindEndSceneAddress Begin\n");
	IDirect3D9 *direct3D9 = Direct3DCreate9(D3D_SDK_VERSION);
	OutputDebugStringA("Direct3DCreate9 Successed\n");

	// create a device class using this information and information from the d3dpp stuct
	HWND hWnd = CreateTempWindow();
	DebugMessage("Temp Window HWND: 0x%08X\n", hWnd);

	D3DPRESENT_PARAMETERS d3dpp;
	ZeroMemory(&d3dpp, sizeof(d3dpp)); 
	d3dpp.Windowed = TRUE; 
	d3dpp.SwapEffect = D3DSWAPEFFECT_DISCARD;
	d3dpp.hDeviceWindow = hWnd;

	LPDIRECT3DDEVICE9 d3ddev;
	if(FAILED(direct3D9->CreateDevice(D3DADAPTER_DEFAULT, D3DDEVTYPE::D3DDEVTYPE_NULLREF , hWnd, D3DCREATE_SOFTWARE_VERTEXPROCESSING, &d3dpp, &d3ddev)))
	{
		OutputDebugStringA("CreateDevice Failed\n");
	}
	else
	{
		DebugMessage("LPDIRECT3DDEVICE9 value: 0x%08X\n", d3ddev);
		uintptr_t *vTableAddress =  (uintptr_t*)*(uintptr_t*)d3ddev;
		DebugMessage("vTable Address: 0x%08X\n", vTableAddress);
		uintptr_t *EndSceneAddrPtr = vTableAddress + ENDSCENE_FUNCTION_ORDER; //+1是加一個指標大小的距離
		EndSceneFunctionPtr oldEndSceneAddr = EndSceneFunctionPtr(*EndSceneAddrPtr);
		DebugMessage("EndScene Address: 0x%08X\n", (void*)(oldEndSceneAddr));
		oldEndScene = (EndSceneFunctionPtr)DetourFunction((PBYTE)oldEndSceneAddr, (PBYTE)newEndScene);
	}

	DestroyWindow(hWnd);
	d3ddev->Release();
	direct3D9->Release();
	OutputDebugStringA("FindEndSceneAddress Done\n");
}



DWORD WINAPI SendImage( LPVOID lpParam )
{
	OutputDebugString(L"SendImage");
	HookEndScene();

	while (!formatCreated)
		Sleep(1000);

	while (!SendFormat())
		Sleep(1000);
	return TRUE;
}


EndSceneFunctionPtr FindEndSceneAddressOldMethod()
{
	HMODULE hd3d9 = NULL;
	MODULEINFO Info;
	hd3d9 = GetModuleHandle(L"d3d9.dll");
	GetModuleInformation(GetCurrentProcess(), hd3d9, &Info, sizeof(MODULEINFO));
	EndSceneFunctionPtr EndSceneAddr =  EndSceneFunctionPtr((uintptr_t)(Info.EntryPoint) + 0x20D5A);
	DebugMessage("Old method EndScene Address: 0x%08X\n", (void*)EndSceneAddr);
	return EndSceneAddr;
}

int WINAPI DllThread()
{
	CreateThread( NULL, 0, (LPTHREAD_START_ROUTINE) SendImage,NULL, 0, sendThreadID); 
	return 0;
}




bool WINAPI DllMain(HINSTANCE hDll, DWORD reason, LPVOID reserved)
{
	processInstance = hDll;
	switch(reason)
	{
	case DLL_PROCESS_ATTACH:
		OutputDebugString(L"DLL_PROCESS_ATTACH");
		DllThread();
		break;
	case DLL_PROCESS_DETACH:
		OutputDebugString(L"DLL_PROCESS_DETACH");
		break;
	}

	return true;
}