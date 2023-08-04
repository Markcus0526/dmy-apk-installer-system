// DriverInstall.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include <stdio.h>

#include "Psapi.h"
#include "setupapi.h"
#include "newdev.h"
#include "regstr.h"
#include "cfgmgr32.h"
#include <vector>

#define MAX_LOADSTRING 100
#define MAX_STR			0x1000
#define MAX_CMDSTR			0x2000

#define STATUS_SUCCESS               ((NTSTATUS)0x00000000L)
#define STATUS_INFO_LENGTH_MISMATCH  ((NTSTATUS)0xC0000004L)

typedef enum _SYSTEM_INFORMATION_CLASS {
	SystemProcessInformation = 5
} SYSTEM_INFORMATION_CLASS;

typedef enum
{
	// IMPORTANT: If you add a new item here, update IsOSVersionAtLeast().

	WIN_UNKNOWN = 0,
	WIN_31,
	WIN_95,
	WIN_98,
	WIN_ME,
	WIN_NT3,
	WIN_NT4,
	WIN_2000,
	WIN_XP,
	WIN_XP64,
	WIN_SERVER_2003,
	WIN_VISTA,
	WIN_SERVER_2008,
	WIN_7,
	WIN_SERVER_2008_R2,
} OSVersionEnum;

typedef struct _UNICODE_STRING {
	USHORT Length;
	USHORT MaximumLength;
	PWSTR  Buffer;
} UNICODE_STRING;

typedef LONG KPRIORITY; // Thread priority
typedef LONG NTSTATUS;

typedef struct _SYSTEM_PROCESS_INFORMATION_DETAILD {
	ULONG NextEntryOffset;
	ULONG NumberOfThreads;
	LARGE_INTEGER SpareLi1;
	LARGE_INTEGER SpareLi2;
	LARGE_INTEGER SpareLi3;
	LARGE_INTEGER CreateTime;
	LARGE_INTEGER UserTime;
	LARGE_INTEGER KernelTime;
	UNICODE_STRING ImageName;
	KPRIORITY BasePriority;
	HANDLE UniqueProcessId;
	ULONG InheritedFromUniqueProcessId;
	ULONG HandleCount;
	BYTE Reserved4[4];
	PVOID Reserved5[11];
	SIZE_T PeakPagefileUsage;
	SIZE_T PrivatePageCount;
	LARGE_INTEGER Reserved6[6];
} SYSTEM_PROCESS_INFORMATION_DETAILD, *PSYSTEM_PROCESS_INFORMATION_DETAILD;

#define SLOT_NAME    TEXT("\\\\.\\mailslot\\mailbox")
HANDLE hMailSlot = NULL;  
std::vector<char*> g_ArrayFailHWID;

typedef int (WINAPI *PFN_NT_QUERY_SYSTEM_INFORMATION)(
	IN       SYSTEM_INFORMATION_CLASS SystemInformationClass,
	IN OUT   PVOID SystemInformation,
	IN       ULONG SystemInformationLength,
	OUT OPTIONAL  PULONG ReturnLength
	);

BOOL Is64bitOS()
{
	char buf[MAX_STR];
	if ( ::GetSystemWow64DirectoryA(buf, MAX_STR) )
		return TRUE;
	else 
		return FALSE;
}

void MakeCharLower(char *str)
{
	if ( !str )	return;
	for(int i = 0; i < strlen(str); i++)
		if ( 'A' <= str[i] && str[i] <= 'Z' )
			str[i] += ('a' - 'A');
}

void MakeWCharLower(WCHAR *str)
{
	if ( !str )	return;
	for(int i = 0; i < wcslen(str); i++)
		if ( L'A' <= str[i] && str[i] <= L'Z' )
			str[i] += (L'a' - L'A');
}

#define SAFE_DELETE(x)	\
	if (x)				\
{					\
	delete x;		\
	x = NULL;		\
}			

WCHAR *bufWStr = NULL;
LPWSTR  GetWCHAR(char *str)
{
	static UINT nLength = 0;

	UINT nStrLength = (UINT)strlen(str);
	if ( nStrLength == 0 )
		return NULL;

	if ( nStrLength > nLength )
	{
		nLength = nStrLength;

		SAFE_DELETE(bufWStr);		
		bufWStr = new WCHAR[nLength + 2];
	}

	if ( MultiByteToWideChar(CP_ACP, 0, str, -1, bufWStr, nLength + 2) )
		return (LPWSTR)bufWStr;
	else
		return NULL;
}

BOOL ExistingFailHWID(char *hardID)
{
	for(int i = 0; i < g_ArrayFailHWID.size(); i++)
	{
		if ( _stricmp(g_ArrayFailHWID[i], hardID) == 0 )
			return TRUE;
	}
	return FALSE;
}

WCHAR* GetWizardProcessName()
{
	OSVERSIONINFOEX os;
	os.dwOSVersionInfoSize = sizeof (OSVERSIONINFOEX);

	if (GetVersionEx ((OSVERSIONINFO *)&os) == FALSE)
		return NULL;

	if ( os.dwMajorVersion == 6 )
	{
		static WCHAR wizardProcessXp[] = L"mmc.exe";
		return wizardProcessXp;
	}
	else if ( os.dwMajorVersion == 5 )
	{
		static WCHAR wizardProcess7[] = L"rundll32.exe";
		return wizardProcess7;
	}
	return NULL;
}

BOOL KillRundll32Process()
{
	PVOID pFree = NULL;
	size_t bufferSize = 102400;
	PSYSTEM_PROCESS_INFORMATION_DETAILD pspid=
		(PSYSTEM_PROCESS_INFORMATION_DETAILD) malloc (bufferSize);
	pFree = (PVOID)pspid;

	ULONG ReturnLength;
	NTSTATUS status;

	PFN_NT_QUERY_SYSTEM_INFORMATION pfnNtQuerySystemInformation = (PFN_NT_QUERY_SYSTEM_INFORMATION)
		GetProcAddress (GetModuleHandle(TEXT("ntdll.dll")), "NtQuerySystemInformation");

	while (TRUE) {
		status = pfnNtQuerySystemInformation (SystemProcessInformation, (PVOID)pspid,
			bufferSize, &ReturnLength);
		if (status == STATUS_SUCCESS)
			break;
		else if (status != STATUS_INFO_LENGTH_MISMATCH) { // 0xC0000004L
			//			_tprintf (TEXT("ERROR 0x%X\n"), status);
			free(pFree);
			return FALSE;   // error
		}

		bufferSize *= 2;
		pspid = (PSYSTEM_PROCESS_INFORMATION_DETAILD) realloc ((PVOID)pspid, bufferSize);
		pFree = (PVOID)pspid;
	}

	WCHAR *wizardProcess = GetWizardProcessName();
	if ( !wizardProcess )
	{
		free(pFree);
		return FALSE;
	}

	for (;;
		pspid=(PSYSTEM_PROCESS_INFORMATION_DETAILD)(pspid->NextEntryOffset + (PBYTE)pspid)) {

			if ( pspid->ImageName.Length && pspid->ImageName.Buffer )
			{ 
				if ( _wcsicmp(pspid->ImageName.Buffer, wizardProcess) == 0 )
				//if ( _wcsicmp(pspid->ImageName.Buffer, L"rundll32.exe") == 0 )
				{
					HANDLE hTerminateProcess = OpenProcess(PROCESS_TERMINATE, 0, (DWORD)pspid->UniqueProcessId);
					if ( hTerminateProcess )
					{
						DWORD dwExitCode = 0;
						GetExitCodeProcess(hTerminateProcess, &dwExitCode);
						TerminateProcess(hTerminateProcess, dwExitCode);
					}
				}
				// 			_tprintf (TEXT("ProcessId: %d, ImageFileName: %ls\n"), pspid->UniqueProcessId,
				// 				(pspid->ImageName.Length && pspid->ImageName.Buffer)? pspid->ImageName.Buffer: L"");
				// 
			}
			if (pspid->NextEntryOffset == 0) break;
	}

	free(pFree);
	return TRUE;
}


BOOL IsRunningDriverTool()
{
	PVOID pFree = NULL;
	size_t bufferSize = 102400;
	PSYSTEM_PROCESS_INFORMATION_DETAILD pspid=
		(PSYSTEM_PROCESS_INFORMATION_DETAILD) malloc (bufferSize);

	pFree = (PVOID)pspid;

	ULONG ReturnLength;
	PFN_NT_QUERY_SYSTEM_INFORMATION pfnNtQuerySystemInformation = (PFN_NT_QUERY_SYSTEM_INFORMATION)
		GetProcAddress (GetModuleHandle(TEXT("ntdll.dll")), "NtQuerySystemInformation");
	NTSTATUS status;

	while (TRUE) {
		status = pfnNtQuerySystemInformation (SystemProcessInformation, (PVOID)pspid,
			bufferSize, &ReturnLength);
		if (status == STATUS_SUCCESS)
			break;
		else if (status != STATUS_INFO_LENGTH_MISMATCH) { // 0xC0000004L
//			_tprintf (TEXT("ERROR 0x%X\n"), status);
			free(pFree);
			return FALSE;   // error
		}

		bufferSize *= 2;
		pspid = (PSYSTEM_PROCESS_INFORMATION_DETAILD) realloc ((PVOID)pspid, bufferSize);
		pFree = (PVOID)pspid;
	}

	for (;;
		pspid=(PSYSTEM_PROCESS_INFORMATION_DETAILD)(pspid->NextEntryOffset + (PBYTE)pspid)) {

			if ( pspid->ImageName.Length && pspid->ImageName.Buffer )
			{ 
				MakeWCharLower(pspid->ImageName.Buffer);
				if ( wcsstr(pspid->ImageName.Buffer, L"drvtool") )
				{
					free(pFree);
					return TRUE;
				}
			// 			_tprintf (TEXT("ProcessId: %d, ImageFileName: %ls\n"), pspid->UniqueProcessId,
			// 				(pspid->ImageName.Length && pspid->ImageName.Buffer)? pspid->ImageName.Buffer: L"");
			// 
			}
			if (pspid->NextEntryOffset == 0) break;
	}

	free(pFree);
	return FALSE;
}


void InstallDriver(char *hardwareID)
{
	//return;
	WCHAR filePath[MAX_STR];
	char parameter[MAX_STR]	;
	DWORD dwErr = 0;
	BOOL b64OS = Is64bitOS();

	if ( !::GetModuleFileNameW(NULL, filePath, MAX_STR) )
		return;

	int nSlash = 0;
	for ( nSlash = wcslen(filePath) - 1; filePath[nSlash] != L'\\' && nSlash > 0 ; nSlash--);	
	filePath[nSlash + 1] = 0;

	int nMaxLen = wcslen(filePath) * 2 + MAX_STR;
	WCHAR *cmdLine = new WCHAR [nMaxLen];

	//WCHAR strIniFile[MAX_STR];
	//swprintf_s(strIniFile, MAX_STR, L"%s\\Drivers\\%s\\android_winusb.inf", b64OS ? L"Driver64" : L"Driver32");

// 	BOOL bRestart = FALSE;
// 
// 	if ( UpdateDriverForPlugAndPlayDevicesW(NULL, GetWCHAR(hardwareID), strIniFile, 1, &bRestart) )
// 	{
// 
// 	}
// 	else
// 	{
// 		dwErr = GetLastError();
// 	}
// 	return ;
	//sprintf_s(moduleName, MAX_STR, "%sDrivers\\%s"
	//	, filePath
	//	, b64OS ? "DrvToolx64.exe" : "DrvTool.exe");

	//sprintf_s(cmdLine, nMaxLen
	//	, "\"update\" \"%s\" \"%sDrivers\\%s\\android_winusb.inf\""
	//	//, filePath
	//	//, b64OS ? "DrvToolx64.exe" : "DrvTool.exe" 
	//	, hardwareID
	//	, filePath
	//	, b64OS ? "Driver64" : "Driver32"
	//	);

	swprintf_s(cmdLine, nMaxLen
		, L"\"%sDrivers\\%s\" \"update\" \"%s\" \"%sDrivers\\%s\\android_winusb.inf\""
		, filePath
		, b64OS ? L"DrvToolx64.exe" : L"DrvTool.exe" 
		, GetWCHAR(hardwareID)
		, filePath
		, b64OS ? L"Driver64" : L"Driver32"
		);

	STARTUPINFOW si;
	PROCESS_INFORMATION pi;

	ZeroMemory( &si, sizeof(si) );
	si.cb = sizeof(si);
	ZeroMemory( &pi, sizeof(pi) );

	// Start the child process. 
	if( !CreateProcessW( NULL,			// No module name (use command line). 
		cmdLine,						// Command line. 
		NULL,							// Process handle not inheritable. 
		NULL,							// Thread handle not inheritable. 
		FALSE,							// Set handle inheritance to FALSE. 
		CREATE_NO_WINDOW | NORMAL_PRIORITY_CLASS,// No creation flags. 
		NULL,							// Use parent's environment block. 
		NULL,							// Use parent's starting directory. 
		&si,							// Pointer to STARTUPINFO structure.
		&pi )							// Pointer to PROCESS_INFORMATION structure.
		)
	{
		DWORD dwErr = GetLastError();
		delete cmdLine;
		return ;
	}

	DWORD dwError = 0;
	DWORD sizeRead = 0;
	if ( !ReadFile(hMailSlot, &dwError, sizeof(DWORD), &sizeRead, NULL) )
	{

	}
	else
	{
		if ( dwError )
		{
			g_ArrayFailHWID.push_back(strdup(hardwareID));
		}
	}
	//WaitForSingleObject( pi.hProcess, INFINITE );

	//// Close process and thread handles. 
	//CloseHandle( pi.hProcess );
	//CloseHandle( pi.hThread );

	//UINT uRet = WinExec(cmdLine, SW_HIDE);
	delete cmdLine;
}

BOOL IsAndroidAdbDevice(DEVNODE DevNode)
{
	ULONG BufferLen = 0;
	int  BufferSize = 5000;
	static char  Buffer[5000];
	memset(Buffer, 0, sizeof(Buffer));
	ULONG pulType = 0;

	int lenString = 0;

	BufferLen = BufferSize ;

	if (CR_SUCCESS == CM_Get_DevNode_Registry_Property(DevNode,
		CM_DRP_DEVICEDESC, &pulType,
		Buffer, &BufferLen, 0))
	{
// 		char *pCurStr = Buffer;
// 
// 		lenString = strlen(pCurStr);
//		while(lenString)
		if ( pulType == REG_SZ && BufferSize >= BufferLen )
		{
			MakeCharLower(Buffer);

			if ( strstr(Buffer, "interface") && strstr(Buffer, "adb") )
				return TRUE;
		}
	}	

	return FALSE;
}

char* GetAndroidHardwareID(DEVNODE DevNode, BOOL bNormal)
{
	ULONG BufferLen = 0;
	int  BufferSize = 5000;
	static char  Buffer[5000];
	memset(Buffer, 0, sizeof(Buffer));
	ULONG pulType = 0;

	int lenString = 0;

	BufferLen = BufferSize ;

	if (CR_SUCCESS == CM_Get_DevNode_Registry_Property(DevNode,
		CM_DRP_HARDWAREID, &pulType,
		Buffer, &BufferLen, 0))
	{
		if ( !(pulType == REG_SZ || pulType == REG_MULTI_SZ) )
			return NULL;
		if ( BufferLen > BufferSize )
			return NULL;

		char *pCurStr = Buffer;

		lenString = strlen(pCurStr);
		while(lenString)
		{
			MakeCharLower(pCurStr);

			if ( strstr(pCurStr, "vid") && strstr(pCurStr, "pid") && !strstr(pCurStr, "rev") 
				&& ( !bNormal || strstr(pCurStr, "mi") ) )
				return pCurStr;

			pCurStr += lenString + 1;
			lenString = strlen(pCurStr);
		}
	}	

	return NULL;
}

void GetUnknownUSBDrivers(std::vector<char*> &unknownHWID)
{
	HDEVINFO        hDevInfo         = NULL;
	SP_DEVINFO_DATA spDevInfoData    = {0};
	short           wIndex           = 0;

	hDevInfo = SetupDiGetClassDevs(0L, 0L, NULL, DIGCF_PRESENT |
		DIGCF_ALLCLASSES | DIGCF_PROFILE);
	if (hDevInfo == INVALID_HANDLE_VALUE)
	{		
		return ;
	};

	wIndex = 0;
	spDevInfoData.cbSize = sizeof(SP_DEVINFO_DATA);

	while (SetupDiEnumDeviceInfo(hDevInfo,
		wIndex,
		&spDevInfoData))
	{
		char  szBuf[2048] = {0};
		short wImageIdx       = 0;
		short wItem           = 0;
		

		if ( !IsAndroidAdbDevice(spDevInfoData.DevInst) ) // If this is adb device, 

		{
			char *hardID = GetAndroidHardwareID(spDevInfoData.DevInst, TRUE); // pid, vid, !ref, mi

			if ( hardID && !ExistingFailHWID(hardID) ) //normal anroid hardwareid type and is not there in fail list.
				unknownHWID.push_back(strdup(hardID));
		}
		
		if (!SetupDiGetDeviceRegistryProperty(hDevInfo,
			&spDevInfoData,
			SPDRP_CLASS, //SPDRP_DEVICEDESC,
			0L,
			(PBYTE)szBuf,
			2048,
			0)) // Unknown Device
		{
			char *hardID = GetAndroidHardwareID(spDevInfoData.DevInst, FALSE);		// pid, vid, !ref, !mi

			if ( hardID && !ExistingFailHWID(hardID) )
				unknownHWID.push_back(strdup(hardID));
		}

		wIndex++;
	}

	SetupDiDestroyDeviceInfoList(hDevInfo);
}

BOOL	ExistingHardware(char *hwID)
{
	std::vector<char*> ukHWIDs;
	GetUnknownUSBDrivers(ukHWIDs);

	BOOL bExist = FALSE;
	for(int i = 0; i < ukHWIDs.size(); i++)
	{
		if ( !bExist && strcmp(hwID, ukHWIDs[i]) == 0 )
			bExist = TRUE;
		free(ukHWIDs[i]);
	}

	return bExist;
}

DWORD InstallProcess(LPVOID lpParam)
{
	int i;

	KillRundll32Process();
	hMailSlot = CreateMailslot(SLOT_NAME, 0, MAILSLOT_WAIT_FOREVER, NULL);
	if ( hMailSlot == INVALID_HANDLE_VALUE )
		return 0;

	for(;;)
	{
		Sleep(1000);

		std::vector<char*> ukHWIDs;
		GetUnknownUSBDrivers(ukHWIDs);

		for (i = 0; i < ukHWIDs.size(); i++)
		{
			while ( IsRunningDriverTool() )
				Sleep(500);

			if ( !ExistingHardware(ukHWIDs[i]) )
			{
				free(ukHWIDs[i]);
				continue;
			}

			InstallDriver(ukHWIDs[i]);
			Sleep(500);
			free(ukHWIDs[i]);
		}

		ukHWIDs.clear();
	}

	CloseHandle(hMailSlot);
	return 0;
}

void InstallMain()
{
	//UINT uErr = WinExec("\"DriverToolsx64.exe\"\
	//		\"update\"\
	//		\"USB\\VID_0955&PID_7100&MI_01\"\
	//		\"C:\\a b\\android_winusb.inf\""
	//		, SW_HIDE);	

	TCHAR messageBox[50];
	DWORD bytesRead;  // number of bytes read


	CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)InstallProcess, NULL, 0, 0);
}


