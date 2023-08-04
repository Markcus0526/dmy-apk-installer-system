// ADBWrapper.cpp : Defines the exported functions for the DLL application.
//

#include <stdio.h>
#include <stdlib.h>
#include <stddef.h>
#include <string.h>

#include "stdafx.h"
#include "ADBWrapper.h"
#include <ObjBase.h>

extern "C" int org_main(int argc, char **argv);
extern "C" char * ac_get_shell_cmd_result(char * hardwareid, char * cmd);
extern "C" char* ac_get_devices( );
extern "C" char* ac_get_screen_image(char *hardwareid);
extern "C" int ac_reboot(char * hardwareid);
extern "C" int ac_install_app(char * hardwareid, char * apkpath);
extern "C" int ac_update_app(char * hardwareid, char * apkpath);

extern "C" void WriteLogFile(char* szString);
// This is an example of an exported function.

#define RETURN_WITH_MALLOC(x) do{ \
	char * ret = x;\
	if ( !ret ) return NULL;\
	char * pszReturn = NULL;\
	size_t stSize = strlen(ret) + sizeof(char);\
	pszReturn = (char*)::CoTaskMemAlloc(stSize);\
	strcpy_s(pszReturn, stSize,ret);\
	free(ret);\
	return pszReturn;}while(0)


ADBWRAPPER_API char* adb_list_devices()
{
	RETURN_WITH_MALLOC(ac_get_devices());
}

ADBWRAPPER_API char* adb_screen_image(char * hardwareid)
{
	RETURN_WITH_MALLOC(ac_get_screen_image(hardwareid));
}

ADBWRAPPER_API char * adb_get_device_vendor( char * hardwareid)
{
	RETURN_WITH_MALLOC(ac_get_shell_cmd_result(hardwareid, "shell:getprop ro.product.brand"));
}

ADBWRAPPER_API char * adb_get_device_model( char * hardwareid)
{
	RETURN_WITH_MALLOC(ac_get_shell_cmd_result(hardwareid, "shell:getprop ro.product.model"));
}

ADBWRAPPER_API void adb_reboot_device( char * hardwareid)
{
	ac_reboot(hardwareid);
}

ADBWRAPPER_API char* adb_get_freespace(char * hardwareid)
{
	RETURN_WITH_MALLOC(ac_get_shell_cmd_result(hardwareid, "shell:/system/bin/busybox df -P -k"));
}

ADBWRAPPER_API void adb_test_server()
{

}

int adb_server_thread_main(void* param)
{
	int argc = 3;
	char * argv[3]={"adb","fork-server", "server"};
	return org_main(argc, argv);
}


ADBWRAPPER_API int adb_install_apk(char * hardwareid, char *apkname, char* vcode, char* vname, char* package, int location, char * localapk_path)
{
	if (location != 2) {
		int argc = 10;
		char * argv[10] ={"adb", "-s", NULL, "install", "-r", NULL, vcode, vname, package, apkname};

		argv[2] = hardwareid;
		argv[5] = localapk_path;
		return org_main(argc, argv);

	} else if (location == 2) {	//SD card
		int argc = 11;
		char * argv[11] = {"adb", "-s", NULL, "install", "-r", "-s", NULL, vcode, vname, package, apkname};
		argv[2] = hardwareid;
		argv[6] = localapk_path;
		return org_main(argc, argv);
	}
	
	return 0;
}

ADBWRAPPER_API int adb_uninstall_apk(char * hardwareid, char * apkname, char * package_name, bool bReportStatus)
{
	int argc = 7;
//	char * argv[5] ={"adb", "-s", NULL, "uninstall", NULL};
	char * argv[7] ={"adb", "-s", NULL, "uninstall", NULL, bReportStatus ? "1" : "0", apkname};
	argv[2] = hardwareid;
	argv[4] = package_name;
	return org_main(argc, argv);
}


ADBWRAPPER_API int adb_forward_tcp(char * hardwareid, int localport, int remote_port)
{
	int argc = 6;
	char * argv[6] ={"adb", "-s", NULL, "forward", NULL, NULL};
	char strlocalport[50];
	char strremoteport[50];
	argv[2] = hardwareid;
	_snprintf(strlocalport, 50, "tcp:%d", localport);
	_snprintf(strremoteport, 50, "tcp:%d", remote_port);
	argv[4] = strlocalport;
	argv[5] = strremoteport;
	return org_main(argc, argv);
}

// ADBWRAPPER_API int adb_start_agent(char * hardwareid, int apkagent_versioncode, char * agent_package)
// {
// 	int nCheckApkAgentReadyCount = 0;
// 	bool bFirstBooting = false;
// 
// 	// checking if device complete booting
// 	char *checkBootCompletedStr;
// 	while(nCheckApkAgentReadyCount < 20)
// 	{
// 		checkBootCompletedStr = ac_get_shell_cmd_result(hardwareid, "shell:getprop init.svc.bootanim");
// 		if(!checkBootCompletedStr)
// 		{
// 			Sleep(1000);
// 			nCheckApkAgentReadyCount++;
// 			bFirstBooting = true;
// 			continue;
// 		}
// 
// 		if(checkBootCompletedStr)
// 		{		
// 			if(strstr(checkBootCompletedStr, "stopped"))
// 				break;
// 			else
// 			{
// 				bFirstBooting = true;
// 				Sleep(1000);
// 			}
// 		}
// 		nCheckApkAgentReadyCount++;
// 	}	
// 
// 	if(bFirstBooting)		// if this is device first booting, wait for other Apks finished.
// 		Sleep(5000);
// 
// 	// checking if device's ApkAgent version is up-to-date
// 	char *versionCodeStr = ac_get_shell_cmd_result(hardwareid, "shell:dumpsys package com.damy.apkagent");	
// 	int bReport = 0;
// 	int versionCode = 1;
// 
// 	if(versionCodeStr) {
// // 		if(strstr(versionCodeStr, "Can't find service: package"))		// device not yet complete starting
// // 			return -1;
// 
// 		char *versionCodeIndex = strstr(versionCodeStr, "versionCode=");
// 		if(versionCodeIndex)
// 			versionCode = atoi(versionCodeIndex + 12);
// 	}
// 
// 	if(versionCode < apkagent_versioncode)
// 	{
// 		adb_uninstall_apk(hardwareid, "", "com.damy.apkagent", bReport);
// 		if(bReport > 0)
// 			return bReport;
// 
// 		bReport = ac_install_app(hardwareid, "APKAgent.apk");
// 		if(bReport > 0)
// 			return bReport;
// 	}
// 			
// 	nCheckApkAgentReadyCount = 0;
// 
// 	while(nCheckApkAgentReadyCount < 10)
// 	{
// 		// starting ApkAgent
// 		char * startactivity = ac_get_shell_cmd_result(hardwareid, "shell:am start -n com.damy.apkagent/.MainActivity");
// 		if ( startactivity )
// 			free(startactivity);		
// 		
// 		// checking if ApkAgent complete starting
// 		char * process = ac_get_shell_cmd_result(hardwareid, "shell:ps" );	
// 		if ( process )
// 		{
// 			if ( strstr(process, agent_package) != NULL )
// 			{
// 				return 0;
// 			}
// 			free(process);
// 		}
// 		Sleep(1000);
// 		nCheckApkAgentReadyCount++;
// 	}	
// 
// 	return 1;
// }

ADBWRAPPER_API int adb_start_agent(char * hardwareid, char * apkagent_file, int apkagent_versioncode, char * agent_package)
{
	int bDevBoot = 0;
	char *checkBootCompletedStr = ac_get_shell_cmd_result(hardwareid, "shell:getprop init.svc.bootanim");

	if(checkBootCompletedStr)
	{		
		if(strstr(checkBootCompletedStr, "stopped") || checkBootCompletedStr[0] == '\r')
		{
			bDevBoot = 1;
		}
		free(checkBootCompletedStr);
	}

	if (bDevBoot == 1)
	{
		char *versionCodeStr = ac_get_shell_cmd_result(hardwareid, "shell:dumpsys package com.damy.apkagent");	
		int versionCode = 0;

		if(versionCodeStr) {
			char *versionCodeIndex = strstr(versionCodeStr, "versionCode=");
			if(versionCodeIndex)
				versionCode = atoi(versionCodeIndex + 12);
			free(versionCodeStr);
		}

		if(versionCode < apkagent_versioncode)
		{
			if (versionCode > 0) {
				adb_uninstall_apk(hardwareid, "", agent_package, 0);
			}

			int ret = ac_install_app(hardwareid, apkagent_file);
			if ( ret > 0 )
				return ret;
		}

		char * process = ac_get_shell_cmd_result(hardwareid, "shell:ps" );
		int bFound = 0;
		if ( process )
		{
			if ( strstr(process, agent_package) != NULL )
			{
				bFound = 1;
			}
			free(process);
		}

		//if (!bFound)
		{
			char * startactivity = ac_get_shell_cmd_result(hardwareid, "shell:am start -n com.damy.apkagent/.MainActivity");
			if ( startactivity )
				free(startactivity);
		}
	}

	return 0;
}

// #define AGENT_NAME	"com.damy.apkagent"
// ADBWRAPPER_API int adb_start_agent(char * hardwareid, int apkagent_versioncode, char * agent_package)
// {
// 	char * process = ac_get_shell_cmd_result(hardwareid, "shell:ps" );
// 	int bFound = 0;
// 	if ( process )
// 	{
// 		if ( strstr(process, AGENT_NAME) != NULL )
// 		{
// 			bFound = 1;
// 		}
// 		free(process);
// 	}
// 	if ( !bFound )
// 	{
// 
// 		char * packagepath = ac_get_shell_cmd_result(hardwareid, "shell:pm path com.damy.apkagent");
// 		if ( !packagepath )
// 		{
// 			int ret = ac_install_app(hardwareid, "APKAgent.apk");
// 
// 			//Modified by CSC:
// 			//if ( ret < 0 )
// 			if ( ret > 0 )
// 				return ret;
// 		}
// 		else
// 			free(packagepath);
// 
// 		// start activity
// 		char * startactivity = ac_get_shell_cmd_result(hardwareid, "shell:am start -n com.damy.apkagent/.MainActivity");
// 		if ( startactivity )
// 			free(startactivity);
// 	} else {
// 		char * startactivity = ac_get_shell_cmd_result(hardwareid, "shell:am start -n com.damy.apkagent/.MainActivity");
// 		if ( startactivity )
// 			free(startactivity);
// 	}
// 
// 
// 	return 0;
// }

ADBWRAPPER_API int adb_get_install_location(char * hardwareid)
{
	char * install_location = ac_get_shell_cmd_result(hardwareid, "shell:pm get-install-location" );
	int ret = -1;
	if ( install_location )
	{
		if ( install_location[0]>='0' && install_location[0] <= '2')
			ret = install_location[0] -'0';
		free(install_location);
	}

	return ret;
}


ADBWRAPPER_API int adb_set_install_location(char * hardwareid, int install_location)
{

	char cmd[1024];
	char * result;
	int ret = -1;

	_snprintf(cmd, 1024,"shell:pm set-install-location %d", install_location);
	result = ac_get_shell_cmd_result(hardwareid, cmd);
	if ( result )
	{
		free(result);
	}

	return ret;
}