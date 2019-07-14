#ifndef _Sender_H_
#define _Sender_H_
#include <windows.h>
#pragma comment(lib, "Ws2_32.lib")

class Sender
{
public:
	Sender();
	~Sender();
	bool Connect();
	void Disconnect();
	int SendData(const char* msg,int len);
private:
	SOCKET mSocket;
	SOCKADDR_IN serverInfo;
	void fillServerInfo();
	bool connected;
};

Sender::Sender()
{
	connected = false;
	mSocket = socket(AF_INET,SOCK_STREAM,IPPROTO_TCP);
	if (mSocket == INVALID_SOCKET)
	{
		OutputDebugStringA("@Sender::Sender -> Socket create failed");
		return ;
	}

	fillServerInfo();
}

void Sender::fillServerInfo()
{
	serverInfo.sin_family = AF_INET;
	serverInfo.sin_port = htons(13579);
	serverInfo.sin_addr.s_addr = inet_addr("127.0.0.1");
}

bool Sender::Connect()
{
	int status = connect(mSocket,(sockaddr*)&serverInfo,sizeof(serverInfo));
	if (status != 0)
	{
		OutputDebugStringA("@Sender::Connect -> Connect failed with error");
	}
	connected = (status == 0);
	return connected;
}

int Sender::SendData(const char* msg,int len)
{
	if(!connected)
	{
		OutputDebugStringA("@Sender::SendData -> Send msg without connection");
		return 0;
	}
	int bytesSent = 0;

	bytesSent = send(mSocket,msg,len,0);
	if (bytesSent < 0)
	{
		OutputDebugStringA("@Sender::SendData -> Send msg failed with error");
	}
	else
	{
		char c[100];
		std::sprintf(c, "Real Send size : %d bytes" ,bytesSent);
		OutputDebugStringA(c);
	}

	return bytesSent;
}

void Sender::Disconnect()
{
	closesocket(mSocket);
	connected = false;
}

Sender::~Sender()
{
	if(connected)
		Disconnect();
}

#endif