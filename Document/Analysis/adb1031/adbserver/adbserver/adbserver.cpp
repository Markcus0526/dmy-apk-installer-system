// adbserver.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "adb_api.h"
extern "C" int org_main(int argc, char*argv[]);
int _tmain(int argc, _TCHAR* argv[])
{
	int largc = 3;
	char * largv[3]={"adb", "fork-server", "server"};
	adb_api_init();
	return org_main(largc, largv);
}

