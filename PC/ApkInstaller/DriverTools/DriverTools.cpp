// DriverTools.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "Newdev.h"

#define SLOT_NAME _T("\\\\.\\mailslot\\mailbox")

int _tmain(int argc, _TCHAR* argv[])
{
	char a[100];
	
	//sprintf(a, "---*---- start %d", argc);
	//OutputDebugStringA(a);

	if ( argc != 4 && _tcsicmp(argv[0], TEXT("update")) )
		return 0;

	HANDLE hMailSlot = NULL;  
	TCHAR message[50];
	DWORD bytesWritten;  

	
	hMailSlot=CreateFile(SLOT_NAME, GENERIC_WRITE, FILE_SHARE_READ, NULL,
		OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

	if ( hMailSlot == INVALID_HANDLE_VALUE )
		return 0;

	DWORD dwError = 0;
	BOOL bRebootReq = 0;
	if ( UpdateDriverForPlugAndPlayDevices(NULL, argv[2], argv[3], 1, &bRebootReq) )
	{
		dwError = 0;
	}
	else
	{
		dwError = GetLastError();
	}

	WriteFile(hMailSlot, &dwError, sizeof(dwError), &bytesWritten, NULL);
	CloseHandle(hMailSlot);

	return 0;
}

