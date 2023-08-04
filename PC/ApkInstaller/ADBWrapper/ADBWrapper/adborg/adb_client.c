#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <limits.h>
#include <stdarg.h>
#include <zipfile.h>
#include <sys/types.h>
#include <sys/stat.h>

#include "sysdeps.h"

#define  TRACE_TAG  TRACE_ADB
#include "adb_client.h"




void adb_set_transport(transport_type type, const char* serial)
{
 
}

void adb_set_tcp_specifics(int server_port)
{

}

void adb_set_tcp_name(const char* hostname)
{

}

int  adb_get_emulator_console_port(adb_client_info * client)
{
    const char*   serial = client->serial;
    int           port;

    if (serial == NULL) {
        /* if no specific device was specified, we need to look at */
        /* the list of connected devices, and extract an emulator  */
        /* name from it. two emulators is an error                 */
        char*  tmp = adb_query(client, "host:devices");
        char*  p   = tmp;
        if(!tmp) {
            printf("no emulator connected\n");
            return -1;
        }
        while (*p) {
            char*  q = strchr(p, '\n');
            if (q != NULL)
                *q++ = 0;
            else
                q = p + strlen(p);

            if (!memcmp(p, LOCAL_CLIENT_PREFIX, sizeof(LOCAL_CLIENT_PREFIX)-1)) {
                if (serial != NULL) {  /* more than one emulator listed */
                    free(tmp);
                    return -2;
                }
                serial = p;
            }

            p = q;
        }
        free(tmp);

        if (serial == NULL)
            return -1;  /* no emulator found */
    }
    else {
        if (memcmp(serial, LOCAL_CLIENT_PREFIX, sizeof(LOCAL_CLIENT_PREFIX)-1) != 0)
            return -1;  /* not an emulator */
    }

    serial += sizeof(LOCAL_CLIENT_PREFIX)-1;
    port    = strtol(serial, NULL, 10);
    return port;
}



const char *adb_error(adb_client_info * client)
{
    return (const char*)client->adb_error;
}

static int switch_socket_transport(adb_client_info * client, int fd)
{
    char service[64];
    char tmp[5];
    int len;

    if (client->serial)
        _snprintf(service, sizeof service, "host:transport:%s", client->serial);
    else {
        char* transport_type = "???";

         switch (client->transtype) {
            case kTransportUsb:
                transport_type = "transport-usb";
                break;
            case kTransportLocal:
                transport_type = "transport-local";
                break;
            case kTransportAny:
                transport_type = "transport-any";
                break;
            case kTransportHost:
                // no switch necessary
                return 0;
                break;
        }

        _snprintf(service, sizeof service, "host:%s", transport_type);
    }
    len = strlen(service);
    _snprintf(tmp, sizeof tmp, "%04x", len);

    if(writex(fd, tmp, 4) || writex(fd, service, len)) {
        strcpy(client->adb_error, "write failure during connection");
        adb_close(fd);
        return -1;
    }
    D("Switch transport in progress\n");

    if(adb_status(client, fd)) {
        adb_close(fd);
        D("Switch transport failed\n");
        return -1;
    }
    D("Switch transport success\n");
    return 0;
}

int adb_status(adb_client_info * client, int fd)
{
    unsigned char buf[5];
    unsigned len;

    if(readx(fd, buf, 4)) {
        strcpy(client->adb_error, "protocol fault (no status)");
        return -1;
    }

    if(!memcmp(buf, "OKAY", 4)) {
        return 0;
    }

    if(memcmp(buf, "FAIL", 4)) {
        sprintf(client->adb_error,
                "protocol fault (status %02x %02x %02x %02x?!)",
                buf[0], buf[1], buf[2], buf[3]);
        return -1;
    }

    if(readx(fd, buf, 4)) {
        strcpy(client->adb_error, "protocol fault (status len)");
        return -1;
    }
    buf[4] = 0;
    len = strtoul((char*)buf, 0, 16);
    if(len > 255) len = 255;
    if(readx(fd, client->adb_error, len)) {
        strcpy(client->adb_error, "protocol fault (status read)");
        return -1;
    }
    client->adb_error[len] = 0;
    return -1;
}

int _adb_connect(adb_client_info * client, const char *service)
{
    char tmp[5];
    int len;
    int fd;

    D("_adb_connect: %s\n", service);
    len = strlen(service);
    if((len < 1) || (len > 1024)) {
        strcpy(client->adb_error, "service name too long");
        return -1;
    }
    _snprintf(tmp, sizeof tmp, "%04x", len);

    if (client->server_name)
        fd = socket_network_client(client->server_name, client->server_port, SOCK_STREAM);
    else
        fd = socket_loopback_client(client->server_port, SOCK_STREAM);

    if(fd < 0) {
        strcpy(client->adb_error, "cannot connect to daemon");
        return -2;
    }

    if (memcmp(service,"host",4) != 0 && switch_socket_transport(client, fd)) {
        return -1;
    }

    if(writex(fd, tmp, 4) || writex(fd, service, len)) {
        strcpy(client->adb_error, "write failure during connection");
        adb_close(fd);
        return -1;
    }

    if(adb_status(client, fd)) {
        adb_close(fd);
        return -1;
    }

    D("_adb_connect: return fd %d\n", fd);
    return fd;
}

int adb_connect(adb_client_info * client, const char *service)
{
    // first query the adb server's version
    int fd = _adb_connect(client, "host:version");

    D("adb_connect: service %s\n", service);
    if(fd == -2 && client->server_name) {
        fprintf(stderr,"** Cannot start server on remote host\n");
		
        return fd;
    } else if(fd == -2) {
        fprintf(stdout,"* daemon not running. starting it now on port %d *\n",
                client->server_port);
		
    start_server:
        if(launch_server(client->server_port)) {
            fprintf(stderr,"* failed to start daemon *\n");

            return -1;
        } else {
            fprintf(stdout,"* daemon started successfully *\n");

        }
        /* give the server some time to start properly and detect devices */
        adb_sleep_ms(3000);
        // fall through to _adb_connect
    } else {
        // if server was running, check its version to make sure it is not out of date
        char buf[100];
        int n;
        int version = ADB_SERVER_VERSION - 1;

        // if we have a file descriptor, then parse version result
		if(fd >= 0) {
			if(readx(fd, buf, 4)) goto error;

			buf[4] = 0;
			n = strtoul(buf, 0, 16);
            if(n > (int)sizeof(buf)) goto error;
            if(readx(fd, buf, n)) goto error;
            adb_close(fd);

            //if (sscanf(buf, "%04x", &version) != 1) goto error;
        } else {
            // if fd is -1, then check for "unknown host service",
            // which would indicate a version of adb that does not support the version command
            if (strcmp(client->adb_error, "unknown host service") != 0)
                return fd;
        }

// Comment by CSC : Check adb server version.
//         if(version != ADB_SERVER_VERSION) {
//             printf("adb server is out of date.  killing...\n");
//             fd = _adb_connect(client, "host:kill");
//             adb_close(fd);
// 
//             /* XXX can we better detect its death? */
//             adb_sleep_ms(2000);
//             goto start_server;
//         }
    }

    // if the command is start-server, we are done.
    if (!strcmp(service, "host:start-server"))
        return 0;

    fd = _adb_connect(client, service);
    if(fd == -2) {
        fprintf(stderr,"** daemon still not running\n");
    }
    D("adb_connect: return fd %d\n", fd);

    return fd;
error:
    adb_close(fd);
    return -1;
}

int adb_command(adb_client_info * client, const char *service)
{
    int fd = adb_connect(client, service);
    if(fd < 0) {
        return -1;
    }

    if(adb_status(client, fd)) {
        adb_close(fd);
        return -1;
    }

    return 0;
}

char *adb_query(adb_client_info * client, const char *service)
{
    char buf[5];
    unsigned n;
    char *tmp;
	int fd;
    D("adb_query: %s\n", service);
    fd = adb_connect(client, service);
    if(fd < 0) {
        fprintf(stderr,"error: %s\n", client->adb_error);
        return 0;
    }

    if(readx(fd, buf, 4)) goto oops;

    buf[4] = 0;
    n = strtoul(buf, 0, 16);
    if(n > 1024) goto oops;

	//Modified by CSC
	//tmp = malloc(n + 1);
    tmp = (char*)malloc(n + 1);
    if(tmp == 0) goto oops;

    if(readx(fd, tmp, n) == 0) {
        tmp[n] = 0;
        adb_close(fd);
        return tmp;
    }
    free(tmp);

oops:
    adb_close(fd);
    return 0;
}

/*
void report_to_cs(adb_client_info * client, char * str, char * apkname)
{
	HANDLE hPipe; 
	HANDLE hErrorWrite;
	BOOL   fSuccess = FALSE;
	DWORD  cbRead, cbToWrite, cbWritten, dwMode; 

	char   pszServername[100];
	char respstr[200];

	report_message msg;

	if ( !client->should_report )
		return;
//	_snprintf(pszServername,100,"\\\\.\\pipe\\%s",client->serial);
	_snprintf(pszServername,100,"\\\\.\\pipe\\APKPLInstaller");
	
	// Try to open a named pipe; wait for it, if necessary. 

	while (1) 
	{ 
		hPipe = CreateFileA( 
			pszServername,   // pipe name 
			GENERIC_READ |  // read and write access 
			GENERIC_WRITE | SYNCHRONIZE, 
			FILE_SHARE_READ | FILE_SHARE_WRITE ,              // no sharing 
			NULL,           // default security attributes
			OPEN_EXISTING,  // opens existing pipe 
			0,              // default attributes 
			NULL);          // no template file 

		// Break if the pipe handle is valid. 

		if (hPipe != INVALID_HANDLE_VALUE)  {
			break; 
		}

		// Exit if an error other than ERROR_PIPE_BUSY occurs. 

		if (GetLastError() != ERROR_PIPE_BUSY) 
		{
			int err = GetLastError();
			printf( TEXT("Could not open pipe. GLE=%d\n"), err ); 
			return ;
		}

		// All pipe instances are busy, so wait for 20 seconds. 

		if ( ! WaitNamedPipe(pszServername, 5000)) 
		{ 
			printf("Could not open pipe: 20 second wait timed out."); 
			return ;
		} 
	} 

	if (!DuplicateHandle(GetCurrentProcess(), hPipe, GetCurrentProcess(), &hErrorWrite, 0, 1, DUPLICATE_SAME_ACCESS))
	{
		GetLastError();
	}

	cbToWrite = sizeof(char)*(strlen(client->serial) + strlen(str)+strlen(apkname) + 2);
	memset(respstr, 0, 200);

	strncat(respstr, client->serial, strlen(client->serial));
	strncat(respstr, "|", 1);
	strncat(respstr, str, strlen(str));
	strncat(respstr, "|", 1);
	strncat(respstr, apkname, strlen(apkname));

	fSuccess = WriteFile( 
		hPipe,                  // pipe handle 
		respstr,             // message 
		cbToWrite,              // message length 
		&cbWritten,             // bytes written 
		NULL);                  // not overlapped 

	if ( ! fSuccess) 
	{
		printf( TEXT("WriteFile to pipe failed. GLE=%d\n"), GetLastError() ); 
		return ;
	}

	CloseHandle(hPipe); 
}
*/

void report_to_cs(adb_client_info * client, char * str, char * apkname)
{
	char respstr[200];

	SOCKET clientSock;
	SOCKADDR_IN myAddr;
	int port = 51400;
	int n;

	clientSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	memset((char*)&myAddr, 0, sizeof(SOCKADDR_IN));
	myAddr.sin_family = AF_INET;
	myAddr.sin_port = htons(port);
	myAddr.sin_addr.s_addr = inet_addr("127.0.0.1");

	if (connect(clientSock, (struct sockaddr *)&myAddr, sizeof(myAddr)) < 0)
	{
		return;
	}

	memset(respstr, 0, 200);

	strncat(respstr, client->serial, strlen(client->serial));
	strncat(respstr, "|", 1);
	strncat(respstr, str, strlen(str));
	strncat(respstr, "|", 1);
	strncat(respstr, apkname, strlen(apkname));

	n = send(clientSock, respstr, strlen(respstr), 0);

	if (n < 0)
	{
		n = 0;
	}

	closesocket(clientSock);
	return;

// 	sockfd = socket(AF_INET, SOCK_STREAM, 0);
// 	if (sockfd < 0) {
// 		error("ERROR opening socket");
// 	}
// 
// 	server = gethostbyname("127.0.0.1");
// 
// 	if (server == NULL) {
// 		fprintf(stderr,"ERROR, no such host\n");
// 		exit(0);
// 	}
// 
// 	memset((char *) &serv_addr, 0, sizeof(serv_addr));
// 	serv_addr.sin_family = AF_INET;
// 	strncpy((char *)server->h_addr, (char *)&serv_addr.sin_addr.s_addr, server->h_length);
// 	serv_addr.sin_port = htons(10240);
// 
// 	if (connect(sockfd,(struct sockaddr *) &serv_addr,sizeof(serv_addr)) < 0) {
// 		return;
// 	}
// 
// 	memset(respstr, 0, 200);
// 
// 	strncat(respstr, client->serial, strlen(client->serial));
// 	strncat(respstr, "|", 1);
// 	strncat(respstr, str, strlen(str));
// 	strncat(respstr, "|", 1);
// 	strncat(respstr, apkname, strlen(apkname));
// 
// 	n = send(sockfd,respstr,strlen(respstr), 0);
// 
// 	if (n < 0) {
// 		return ;
// 	}
// 	close(sockfd);
// 
// 	return;
}