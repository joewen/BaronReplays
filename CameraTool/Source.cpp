#define _CRT_SECURE_NO_DEPRECATE 
#include <Windows.h>
#include "detours.h"
#include <psapi.h>
#include <iostream>
#define _USE_MATH_DEFINES
#include <math.h>
#define TRACE_ANGLE_SPEED 0.25

typedef void(*TargetFuncPtr)();

enum CameraTraceMood
{
	None,
	Back,
	Front
};

HHOOK keyboardHook;
HHOOK mouseHook;

HANDLE hTimerQueue = nullptr;
HANDLE updateAnglesTimer = nullptr;
HANDLE switchUpdateTimer = nullptr;

TargetFuncPtr oldFunc;
HINSTANCE hInstance = NULL;
float* dataStartPos = nullptr;

bool rightButtonDown = false;
POINT mouseRDownPos;
float viewAngle;
float roundAngle;

float cameraZValue;

float screenX;
float screenY;

float *cameraX;
float *cameraY;
float *cameraZ;
float *LookZ;
float *LookAngle;

float angleRemain = 0;

HWND windowHandle;
int screenHeight = 0;

float direction = 1.0;

CameraTraceMood traceMode = None;

bool isCursorVisible = true;


void GetScreenHeight()
{
	windowHandle = GetActiveWindow();
	if (windowHandle != 0)
	{
		RECT r;
		if (GetClientRect(windowHandle, &r))
		{
			screenHeight = r.bottom - r.top;

		}
	}

}

void FlyCameraMove(float x, float y)
{
	float radianX = *LookAngle * (M_PI / 180);
	float radianY = (*LookAngle + 90) * (M_PI / 180);
	screenX = screenX + x * cos(radianX) + y * cos(radianY);
	screenY = screenY + x * sin(radianX) + y * sin(radianY);
	if (screenX < -120)
		screenX = -120;
	if (screenX > 15000)
		screenX = 15000;
	if (screenY < -120)
		screenY = -120;
	if (screenY > 15000)
		screenY = 15000;
	*cameraX = screenX;
	*cameraY = screenY;
}

void SetLookAngleWithAnimation(float deg)
{
	if (abs(deg - *LookAngle) < 180)
		angleRemain = deg - *LookAngle;
	else
		angleRemain = *LookAngle - deg;
}

float NormoalizeLookDegree(float deg)
{
	if (deg > 360 || deg < 0)
	{
		float ratio = (deg + 360) / 360.0;
		int ceil = ratio;
		deg = (ratio - ceil) * 360;
	}
	return deg;
}

void NormoalizeLookAngle()
{
	*LookAngle = NormoalizeLookDegree(*LookAngle);
}

void TurnLookAngle(float deg)
{
	*LookAngle = *LookAngle + deg;
	NormoalizeLookAngle();
}

void TurnLookZ(float deg)
{
	float res = *LookZ + deg;
	if (res > 89)
		res = 89;
	else if (res <= -89)
		res = -89;
	*LookZ = res;
}

void TurnRight(float deg)
{
	TurnLookAngle(deg * -1);
}

void TurnLeft(float deg)
{
	TurnLookAngle(deg);
}

void ReturnToOriginal()
{
	*LookZ = 56.0;
	*LookAngle = 0;
	cameraZValue = 40.0;
	screenX = 0;
	screenY = 0;
}



void CalculateRoundDirection()
{
	POINT p;
	GetCursorPos(&p);
	ScreenToClient(windowHandle, &p);
	if (p.y < screenHeight / 2)
		direction = 1.0;
	else
		direction = -1.0;
}

static VOID CALLBACK UpdateAngles(PVOID lpParam, BOOLEAN TimerOrWaitFired)
{
	POINT curPos;
	GetCursorPos(&curPos);


	SHORT MouseRbtnState = GetAsyncKeyState(VK_RBUTTON);

	if ((MouseRbtnState == -32768) && !rightButtonDown)
	{
		CalculateRoundDirection();
		rightButtonDown = true;
		GetCursorPos(&mouseRDownPos);
		viewAngle = *LookZ;
		roundAngle = *LookAngle;
	}
	else if ((MouseRbtnState == 0) && rightButtonDown)
	{
		rightButtonDown = false;
	}
	else if ((MouseRbtnState == -32768) && rightButtonDown)
	{
		*LookZ = viewAngle + (curPos.y - mouseRDownPos.y) / 10.0;
		*LookAngle = roundAngle + ((curPos.x - mouseRDownPos.x) / 10.0 * direction);
	}

	if (GetAsyncKeyState(VK_NUMPAD6) == -32768)
	{
		FlyCameraMove(50, 0);
	}
	if (GetAsyncKeyState(VK_NUMPAD4) == -32768)
	{
		FlyCameraMove(-50, 0);
	}
	if (GetAsyncKeyState(VK_NUMPAD8) == -32768)
	{
		FlyCameraMove(0, 50);
	}
	if (GetAsyncKeyState(VK_NUMPAD2) == -32768)
	{
		FlyCameraMove(0, -50);
	}
	if (GetAsyncKeyState(VK_NUMPAD7) == -32768)
	{
		TurnLeft(2.0);
	}
	if (GetAsyncKeyState(VK_NUMPAD9) == -32768)
	{
		TurnRight(2.0);
	}

	if (GetAsyncKeyState(VK_NUMPAD1) == -32768)
	{
		TurnLookZ(-2.0);
	}
	if (GetAsyncKeyState(VK_NUMPAD3) == -32768)
	{
		TurnLookZ(2.0);
	}
}


void AnimateRemainAngle()
{
	if (angleRemain > 0)
	{
		if (angleRemain >= TRACE_ANGLE_SPEED * 10)
		{
			*LookAngle = *LookAngle + TRACE_ANGLE_SPEED;
			angleRemain -= TRACE_ANGLE_SPEED;
		}
		else
		{
			angleRemain = 0;
		}
	}
	else if (angleRemain < 0)
	{

		if (angleRemain < TRACE_ANGLE_SPEED * -10)
		{
			*LookAngle = *LookAngle - TRACE_ANGLE_SPEED;
			angleRemain += TRACE_ANGLE_SPEED;
		}
		else
		{
			angleRemain = 0;
		}
	}

}


char* findDataInContent(char* target, int targetLen, char* content, int contentLen, char* mask)
{
	char* end = (content + contentLen) - targetLen;
	while (content != end)
	{
		int j = 0;
		for (; j < targetLen; j++)
		{
			if (mask[j] != '?')
			{
				if (target[j] != content[j])
					break;
			}
			if (j == targetLen - 1)
			{
				return content;
			}
		}
		content++;
	}
	return nullptr;
}

void DebugMessage(const char* name, int val)
{
	char c[100];
	std::sprintf(c, name, val);
	OutputDebugStringA(c);
}



LRESULT __declspec(dllexport)__stdcall  CALLBACK KeyboardProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	if ((lParam & 0x40000000) && (HC_ACTION == nCode))
	{
		switch (wParam)
		{
		case VK_NUMPAD5:
			ReturnToOriginal();
			break;
		case VK_MULTIPLY:
			traceMode = CameraTraceMood::Back;
			angleRemain = 0;
			break;
		case VK_DIVIDE:
			traceMode = CameraTraceMood::None;
			angleRemain = 0;
			break;
		case VK_NUMPAD0:

			if (isCursorVisible)
			{
				while (ShowCursor(FALSE) >= 0);
			}
			else
			{
				while (ShowCursor(TRUE) < 0);
			}
			isCursorVisible = !isCursorVisible;
			break;
		}
	}

	LRESULT RetVal = CallNextHookEx(keyboardHook, nCode, wParam, lParam);
	return  RetVal;
}

LRESULT __declspec(dllexport)__stdcall  CALLBACK MouseProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	MOUSEHOOKSTRUCTEX *mouseInfo = (MOUSEHOOKSTRUCTEX *)lParam;
	short wheelDelta = HIWORD(mouseInfo->mouseData);

	if (wheelDelta >= 120)
		cameraZValue -= 1.0;
	else if (wheelDelta <= -120)
		cameraZValue += 1.0;

	LRESULT RetVal = CallNextHookEx(mouseHook, nCode, wParam, lParam);
	return  RetVal;
}

void NewFunc()
{
	void* startAddr = nullptr;
	__asm
	{
		mov startAddr, edi;
	}

	//DebugMessage("startAddr: 0x%08X\n", startAddr);

	if (dataStartPos == nullptr)
	{
		char target[] = { 0x60, 0x42, 0, 0, 0x34, 0x43, 0, 0, 0x34, 0x43, 0, 0, 0x60, 0x42 };		//往下找預設視角參數
		char mask[] = { "00000000000000" };
		char* startPtr = findDataInContent(target, 14, (char*)startAddr, 1024, mask);	//找512 byte之內的

		if (startPtr == nullptr)
			return;


		//startAddr = (void*)((char*)startAddr + 0x120);
		dataStartPos = (float*)(startPtr - 30);

		cameraX = dataStartPos;
		cameraY = dataStartPos + 2;
		cameraZ = dataStartPos + 11;
		LookZ = dataStartPos + 7;
		LookAngle = dataStartPos + 9;

		screenX = *cameraX;
		screenY = *cameraY;

		*(dataStartPos + 8) = 0;
		*(dataStartPos + 9) = 0;

		cameraZValue = *cameraZ;

		hTimerQueue = CreateTimerQueue();
		CreateTimerQueueTimer(&updateAnglesTimer, hTimerQueue, (WAITORTIMERCALLBACK)UpdateAngles, nullptr, 0, 15, 0);


		keyboardHook = SetWindowsHookEx(WH_KEYBOARD, (HOOKPROC)KeyboardProc, hInstance, GetCurrentThreadId());
		mouseHook = SetWindowsHookEx(WH_MOUSE, (HOOKPROC)MouseProc, hInstance, GetCurrentThreadId());
	}
	else
	{
		if (screenHeight == 0)
			GetScreenHeight();

		AnimateRemainAngle();

		float diffX = *cameraX - screenX;
		float diffY = *cameraY - screenY;

		if (diffX == 0 && diffY == 0)
		{

		}
		else if (abs(diffX - diffY) <= abs(0.00001) || (diffX == 0 && diffY != 0) || (diffX != 0 && diffY == 0))
		{
			FlyCameraMove(diffX, diffY);
		}
		else
		{
			screenX = *cameraX;
			screenY = *cameraY;

			double angleInDegrees = (std::atan2(diffY, diffX) / M_PI) * 180.0;

			if (traceMode == CameraTraceMood::Back)
			{
				SetLookAngleWithAnimation(NormoalizeLookDegree(angleInDegrees - 90));
			}

		}

		NormoalizeLookAngle();
		*cameraZ = cameraZValue;

	}
}




DWORD WINAPI Hook(LPVOID lpParam)
{
	HANDLE hProc = GetCurrentProcess();
	HMODULE hMod = GetModuleHandle(NULL);
	MODULEINFO moduleInfo;
	GetModuleInformation(hProc, hMod, &moduleInfo, sizeof(moduleInfo));
	char target[] = { 0xA1, 0, 0, 0, 0, 0x83, 0xEC, 0x20, 0x53, 0x56, 0x8B, 0xF1, 0x57, 0x85, 0xC0 };
	char mask[] = { "0????0000000000" };
	char* oldfuncPtr = findDataInContent(target, 15, (char*)(moduleInfo.lpBaseOfDll), moduleInfo.SizeOfImage, mask);
	if (oldfuncPtr != nullptr)
	{
		oldFunc = (TargetFuncPtr)DetourFunction((PBYTE)oldfuncPtr, (PBYTE)NewFunc);
	}
	return TRUE;
}



int WINAPI DllThread()
{
	CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)Hook, NULL, 0, NULL);
	return 0;
}

bool WINAPI DllMain(HINSTANCE hDll, DWORD reason, LPVOID reserved)
{
	hInstance = hDll;
	switch (reason)
	{
	case DLL_PROCESS_ATTACH:
		OutputDebugString("DLL_PROCESS_ATTACH");
		DllThread();
		break;
	case DLL_PROCESS_DETACH:
		if (hTimerQueue != nullptr)
		{
			DeleteTimerQueueTimer(hTimerQueue, updateAnglesTimer, nullptr);	//做個好公民，清掃垃圾
			DeleteTimerQueue(hTimerQueue);
		}
		UnhookWindowsHookEx(keyboardHook);
		UnhookWindowsHookEx(mouseHook);
		OutputDebugString("DLL_PROCESS_DETACH");
		break;
	}

	return true;
}