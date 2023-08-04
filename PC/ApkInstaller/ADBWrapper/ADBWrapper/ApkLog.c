#include "stdio.h"

// extern "C"
// {

	void WriteLogFile(char* szString)
	{
		FILE* pFile = fopen("c:\\logFile.txt", "a");
		fprintf(pFile, "%s\n",szString);
		fclose(pFile);
	}

//};