// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"

extern int adb_api_init();
extern int adb_server_thread_main(void* param);
extern "C" int launch_server(int server_port);

HANDLE tid_server = INVALID_HANDLE_VALUE; 
BOOL APIENTRY DllMain( HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
	)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		adb_api_init();
		//launch_server(5037);
		//tid_server = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE) adb_server_thread_main, NULL, 0, NULL);
		break;
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		if ( tid_server != INVALID_HANDLE_VALUE )
		{
			TerminateThread(tid_server, 0);
			tid_server = INVALID_HANDLE_VALUE;
		}
		break;
	}
	return TRUE;
}