/*
 * Copyright (C) 2007 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <limits.h>
#include <stdarg.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <ctype.h>
#include <assert.h>
#include <dirent.h>
#include "sysdeps.h"

#ifdef HAVE_TERMIO_H
#include <termios.h>
#endif

#define  TRACE_TAG  TRACE_ADB
#include "adb.h"
#include "adb_client.h"
#include "file_sync_service.h"
#include "ApkLog.h"


#define SAFE_FREE(x) do{char * ____v = (char*)x; if ( ____v ) free(____v);}while(0)
static int do_cmd(transport_type ttype, char* serial, char *cmd, ...);

void get_my_path(char *s, size_t maxLen);
int find_sync_dirs(const char *srcarg,
        char **android_srcdir_out, char **data_srcdir_out);
int install_app(adb_client_info* client, transport_type transport, char* serial, int argc, char** argv);
int uninstall_app(adb_client_info* client, transport_type transport, char* serial, int argc, char** argv);

static const char *gProductOutPath = NULL;
extern int gListenAll;

static char *product_file(const char *extra)
{
    int n;
    char *x;

    if (gProductOutPath == NULL) {
        fprintf(stderr, "adb: Product directory not specified; "
                "use -p or define ANDROID_PRODUCT_OUT\n");
        exit(1);
    }

    n = strlen(gProductOutPath) + strlen(extra) + 2;
    x = (char*)malloc(n);
    if (x == 0) {
        fprintf(stderr, "adb: Out of memory (product_file())\n");
        exit(1);
    }

    _snprintf(x, (size_t)n, "%s" OS_PATH_SEPARATOR_STR "%s", gProductOutPath, extra);
    return x;
}

void version(FILE * out) {
    fprintf(out, "Android Debug Bridge version %d.%d.%d\n",
         ADB_VERSION_MAJOR, ADB_VERSION_MINOR, ADB_SERVER_VERSION);
}

void help()
{
    version(stderr);

    fprintf(stderr,
        "\n"
        " -a                            - directs adb to listen on all interfaces for a connection\n"
        " -d                            - directs command to the only connected USB device\n"
        "                                 returns an error if more than one USB device is present.\n"
        " -e                            - directs command to the only running emulator.\n"
        "                                 returns an error if more than one emulator is running.\n"
        " -s <specific device>          - directs command to the device or emulator with the given\n"
        "                                 serial number or qualifier. Overrides ANDROID_SERIAL\n"
        "                                 environment variable.\n"
        " -p <product name or path>     - simple product name like 'sooner', or\n"
        "                                 a relative/absolute path to a product\n"
        "                                 out directory like 'out/target/product/sooner'.\n"
        "                                 If -p is not specified, the ANDROID_PRODUCT_OUT\n"
        "                                 environment variable is used, which must\n"
        "                                 be an absolute path.\n"
        " -H                            - Name of adb server host (default: localhost)\n"
        " -P                            - Port of adb server (default: 5037)\n"
        " devices [-l]                  - list all connected devices\n"
        "                                 ('-l' will also list device qualifiers)\n"
        " connect <host>[:<port>]       - connect to a device via TCP/IP\n"
        "                                 Port 5555 is used by default if no port number is specified.\n"
        " disconnect [<host>[:<port>]]  - disconnect from a TCP/IP device.\n"
        "                                 Port 5555 is used by default if no port number is specified.\n"
        "                                 Using this command with no additional arguments\n"
        "                                 will disconnect from all connected TCP/IP devices.\n"
        "\n"
        "device commands:\n"
        "  adb push <local> <remote>    - copy file/dir to device\n"
        "  adb pull <remote> [<local>]  - copy file/dir from device\n"
        "  adb sync [ <directory> ]     - copy host->device only if changed\n"
        "                                 (-l means list but don't copy)\n"
        "                                 (see 'adb help all')\n"
        "  adb shell                    - run remote shell interactively\n"
        "  adb shell <command>          - run remote shell command\n"
        "  adb emu <command>            - run emulator console command\n"
        "  adb logcat [ <filter-spec> ] - View device log\n"
        "  adb forward --list           - list all forward socket connections.\n"
        "                                 the format is a list of lines with the following format:\n"
        "                                    <serial> \" \" <local> \" \" <remote> \"\\n\"\n"
        "  adb forward <local> <remote> - forward socket connections\n"
        "                                 forward specs are one of: \n"
        "                                   tcp:<port>\n"
        "                                   localabstract:<unix domain socket name>\n"
        "                                   localreserved:<unix domain socket name>\n"
        "                                   localfilesystem:<unix domain socket name>\n"
        "                                   dev:<character device name>\n"
        "                                   jdwp:<process pid> (remote only)\n"
        "  adb forward --no-rebind <local> <remote>\n"
        "                               - same as 'adb forward <local> <remote>' but fails\n"
        "                                 if <local> is already forwarded\n"
        "  adb forward --remove <local> - remove a specific forward socket connection\n"
        "  adb forward --remove-all     - remove all forward socket connections\n"
        "  adb jdwp                     - list PIDs of processes hosting a JDWP transport\n"
        "  adb install [-l] [-r] [-s] [--algo <algorithm name> --key <hex-encoded key> --iv <hex-encoded iv>] <file>\n"
        "                               - push this package file to the device and install it\n"
        "                                 ('-l' means forward-lock the app)\n"
        "                                 ('-r' means reinstall the app, keeping its data)\n"
        "                                 ('-s' means install on SD card instead of internal storage)\n"
        "                                 ('--algo', '--key', and '--iv' mean the file is encrypted already)\n"
        "  adb uninstall [-k] <package> - remove this app package from the device\n"
        "                                 ('-k' means keep the data and cache directories)\n"
        "  adb bugreport                - return all information from the device\n"
        "                                 that should be included in a bug report.\n"
        "\n"
        "  adb backup [-f <file>] [-apk|-noapk] [-obb|-noobb] [-shared|-noshared] [-all] [-system|-nosystem] [<packages...>]\n"
        "                               - write an archive of the device's data to <file>.\n"
        "                                 If no -f option is supplied then the data is written\n"
        "                                 to \"backup.ab\" in the current directory.\n"
        "                                 (-apk|-noapk enable/disable backup of the .apks themselves\n"
        "                                    in the archive; the default is noapk.)\n"
        "                                 (-obb|-noobb enable/disable backup of any installed apk expansion\n"
        "                                    (aka .obb) files associated with each application; the default\n"
        "                                    is noobb.)\n"
        "                                 (-shared|-noshared enable/disable backup of the device's\n"
        "                                    shared storage / SD card contents; the default is noshared.)\n"
        "                                 (-all means to back up all installed applications)\n"
        "                                 (-system|-nosystem toggles whether -all automatically includes\n"
        "                                    system applications; the default is to include system apps)\n"
        "                                 (<packages...> is the list of applications to be backed up.  If\n"
        "                                    the -all or -shared flags are passed, then the package\n"
        "                                    list is optional.  Applications explicitly given on the\n"
        "                                    command line will be included even if -nosystem would\n"
        "                                    ordinarily cause them to be omitted.)\n"
        "\n"
        "  adb restore <file>           - restore device contents from the <file> backup archive\n"
        "\n"
        "  adb help                     - show this help message\n"
        "  adb version                  - show version num\n"
        "\n"
        "scripting:\n"
        "  adb wait-for-device          - block until device is online\n"
        "  adb start-server             - ensure that there is a server running\n"
        "  adb kill-server              - kill the server if it is running\n"
        "  adb get-state                - prints: offline | bootloader | device\n"
        "  adb get-serialno             - prints: <serial-number>\n"
        "  adb get-devpath              - prints: <device-path>\n"
        "  adb status-window            - continuously print device status for a specified device\n"
        "  adb remount                  - remounts the /system partition on the device read-write\n"
        "  adb reboot [bootloader|recovery] - reboots the device, optionally into the bootloader or recovery program\n"
        "  adb reboot-bootloader        - reboots the device into the bootloader\n"
        "  adb root                     - restarts the adbd daemon with root permissions\n"
        "  adb usb                      - restarts the adbd daemon listening on USB\n"
        "  adb tcpip <port>             - restarts the adbd daemon listening on TCP on the specified port"
        "\n"
        "networking:\n"
        "  adb ppp <tty> [parameters]   - Run PPP over USB.\n"
        " Note: you should not automatically start a PPP connection.\n"
        " <tty> refers to the tty for PPP stream. Eg. dev:/dev/omap_csmi_tty1\n"
        " [parameters] - Eg. defaultroute debug dump local notty usepeerdns\n"
        "\n"
        "adb sync notes: adb sync [ <directory> ]\n"
        "  <localdir> can be interpreted in several ways:\n"
        "\n"
        "  - If <directory> is not specified, both /system and /data partitions will be updated.\n"
        "\n"
        "  - If it is \"system\" or \"data\", only the corresponding partition\n"
        "    is updated.\n"
        "\n"
        "environmental variables:\n"
        "  ADB_TRACE                    - Print debug information. A comma separated list of the following values\n"
        "                                 1 or all, adb, sockets, packets, rwx, usb, sync, sysdeps, transport, jdwp\n"
        "  ANDROID_SERIAL               - The serial number to connect to. -s takes priority over this if given.\n"
        "  ANDROID_LOG_TAGS             - When used with the logcat option, only these debug tags are printed.\n"
        );
}

int usage()
{
    help();
    return 1;
}

#ifdef HAVE_TERMIO_H
static struct termios tio_save;

static void stdin_raw_init(int fd)
{
    struct termios tio;

    if(tcgetattr(fd, &tio)) return;
    if(tcgetattr(fd, &tio_save)) return;

    tio.c_lflag = 0; /* disable CANON, ECHO*, etc */

        /* no timeout but request at least one character per read */
    tio.c_cc[VTIME] = 0;
    tio.c_cc[VMIN] = 1;

    tcsetattr(fd, TCSANOW, &tio);
    tcflush(fd, TCIFLUSH);
}

static void stdin_raw_restore(int fd)
{
    tcsetattr(fd, TCSANOW, &tio_save);
    tcflush(fd, TCIFLUSH);
}
#endif

static char *  read_and_dump(adb_client_info * client, int fd)
{
    char buf[4097];
    int len;
	char * ret = NULL;
	int totallen = 0;
    while(fd >= 0) {
		char *newstr=NULL;
        D("read_and_dump(): pre adb_read(fd=%d)\n", fd);
        len = adb_read(fd, buf, 4096);
        D("read_and_dump(): post adb_read(fd=%d): len=%d\n", fd, len);
        if(len == 0) {
            break;
        }

		if(len < 0) {
            if(errno == EINTR) continue;
            break;
        }
		
		totallen +=len;
		newstr = malloc(totallen+1);
		newstr[0] =0 ;
		if ( ret ){
			strcpy(newstr, ret);
			free(ret);
		}
		strncat(newstr, buf, len);
		ret = newstr;
        fwrite(buf, 1, len, stdout);
        fflush(stdout);
    }

	return ret;
}

static void copy_to_file(int inFd, int outFd) {
    const size_t BUFSIZE = 32 * 1024;
    char* buf = (char*) malloc(BUFSIZE);
    int len;
    long total = 0;

    D("copy_to_file(%d -> %d)\n", inFd, outFd);
    for (;;) {
        len = adb_read(inFd, buf, BUFSIZE);
        if (len == 0) {
            D("copy_to_file() : read 0 bytes; exiting\n");
            break;
        }
        if (len < 0) {
            if (errno == EINTR) {
                D("copy_to_file() : EINTR, retrying\n");
                continue;
            }
            D("copy_to_file() : error %d\n", errno);
            break;
        }
        adb_write(outFd, buf, len);
        total += len;
    }
    D("copy_to_file() finished after %lu bytes\n", total);
    free(buf);
}

static void *stdin_read_thread(void *x)
{
    int fd, fdi;
    unsigned char buf[1024];
    int r, n;
    int state = 0;

    int *fds = (int*) x;
    fd = fds[0];
    fdi = fds[1];
    free(fds);

    for(;;) {
        /* fdi is really the client's stdin, so use read, not adb_read here */
        D("stdin_read_thread(): pre unix_read(fdi=%d,...)\n", fdi);
        r = unix_read(fdi, buf, 1024);
        D("stdin_read_thread(): post unix_read(fdi=%d,...)\n", fdi);
        if(r == 0) break;
        if(r < 0) {
            if(errno == EINTR) continue;
            break;
        }
        for(n = 0; n < r; n++){
            switch(buf[n]) {
            case '\n':
                state = 1;
                break;
            case '\r':
                state = 1;
                break;
            case '~':
                if(state == 1) state++;
                break;
            case '.':
                if(state == 2) {
                    fprintf(stderr,"\n* disconnect *\n");
#ifdef HAVE_TERMIO_H
                    stdin_raw_restore(fdi);
#endif
                    exit(0);
                }
            default:
                state = 0;
            }
        }
        r = adb_write(fd, buf, r);
        if(r <= 0) {
            break;
        }
    }
    return 0;
}

int interactive_shell(adb_client_info * client)
{
    adb_thread_t thr;
    int fdi, fd;
    int *fds;

    fd = adb_connect(client, "shell:");
    if(fd < 0) {
        fprintf(stderr,"error: %s\n", adb_error(client));
        return 1;
    }
    fdi = 0; //dup(0);

    fds = malloc(sizeof(int) * 2);
    fds[0] = fd;
    fds[1] = fdi;

#ifdef HAVE_TERMIO_H
    stdin_raw_init(fdi);
#endif
    adb_thread_create(&thr, stdin_read_thread, fds);
    SAFE_FREE(read_and_dump(client, fd));
#ifdef HAVE_TERMIO_H
    stdin_raw_restore(fdi);
#endif
    return 0;
}


static void format_host_command(char* buffer, size_t  buflen, const char* command, transport_type ttype, const char* serial)
{
    if (serial) {
        _snprintf(buffer, buflen, "host-serial:%s:%s", serial, command);
    } else {
        const char* prefix = "host";
        if (ttype == kTransportUsb)
            prefix = "host-usb";
        else if (ttype == kTransportLocal)
            prefix = "host-local";

        _snprintf(buffer, buflen, "%s:%s", prefix, command);
    }
}

int adb_download_buffer(adb_client_info * client, const char *service, const char *fn, const void* data, int sz,
                        unsigned progress)
{
    char buf[4096];
    unsigned total;
    int fd;
    const unsigned char *ptr;
	int opt;

    sprintf(buf,"%s:%d", service, sz);
    fd = adb_connect(client, buf);
    if(fd < 0) {
        fprintf(stderr,"error: %s\n", adb_error(client));
        return -1;
    }

    opt = CHUNK_SIZE;
    opt = setsockopt(fd, SOL_SOCKET, SO_SNDBUF, &opt, sizeof(opt));

    total = sz;
    ptr = data;

    if(progress) {
        char *x = strrchr(service, ':');
        if(x) service = x + 1;
    }

    while(sz > 0) {
        unsigned xfer = (sz > CHUNK_SIZE) ? CHUNK_SIZE : sz;
        if(writex(fd, ptr, xfer)) {
            adb_status(client, fd);
            fprintf(stderr,"* failed to write data '%s' *\n", adb_error(client));
            return -1;
        }
        sz -= xfer;
        ptr += xfer;
        if(progress) {
            printf("sending: '%s' %4d%%    \r", fn, (int)(100LL - ((100LL * sz) / (total))));
            fflush(stdout);
        }
    }
    if(progress) {
        printf("\n");
    }

    if(readx(fd, buf, 4)){
        fprintf(stderr,"* error reading response *\n");
        adb_close(fd);
        return -1;
    }
    if(memcmp(buf, "OKAY", 4)) {
        buf[4] = 0;
        fprintf(stderr,"* error response '%s' *\n", buf);
        adb_close(fd);
        return -1;
    }

    adb_close(fd);
    return 0;
}


int adb_download(adb_client_info* client, const char *service, const char *fn, unsigned progress)
{
    void *data;
    unsigned sz;
	int status;

    data = load_file(fn, &sz);
    if(data == 0) {
        fprintf(stderr,"* cannot read '%s' *\n", fn);
        return -1;
    }

    status = adb_download_buffer(client, service, fn, data, sz, progress);
    free(data);
    return status;
}

static void status_window(adb_client_info * client,  transport_type ttype, const char* serial)
{
    char command[4096];
    char *state = 0;
    char *laststate = 0;

        /* silence stderr */
#ifdef _WIN32
    /* XXX: TODO */
#else
    int  fd;
    fd = unix_open("/dev/null", O_WRONLY);
    dup2(fd, 2);
    adb_close(fd);
#endif

    format_host_command(command, sizeof command, "get-state", ttype, serial);

    for(;;) {
        adb_sleep_ms(250);

        if(state) {
            free(state);
            state = 0;
        }

        state = adb_query(client, command);

        if(state) {
            if(laststate && !strcmp(state,laststate)){
                continue;
            } else {
                if(laststate) free(laststate);
                laststate = strdup(state);
            }
        }

        printf("%c[2J%c[2H", 27, 27);
        printf("Android Debug Bridge\n");
        printf("State: %s\n", state ? state : "offline");
        fflush(stdout);
    }
}

/** duplicate string and quote all \ " ( ) chars + space character. */
static char *
dupAndQuote(const char *s)
{
    const char *ts;
    size_t alloc_len;
    char *ret;
    char *dest;

    ts = s;

    alloc_len = 0;

    for( ;*ts != '\0'; ts++) {
        alloc_len++;
        if (*ts == ' ' || *ts == '"' || *ts == '\\' || *ts == '(' || *ts == ')') {
            alloc_len++;
        }
    }

    ret = (char *)malloc(alloc_len + 1);

    ts = s;
    dest = ret;

    for ( ;*ts != '\0'; ts++) {
        if (*ts == ' ' || *ts == '"' || *ts == '\\' || *ts == '(' || *ts == ')') {
            *dest++ = '\\';
        }

        *dest++ = *ts;
    }

    *dest++ = '\0';

    return ret;
}

/**
 * Run ppp in "notty" mode against a resource listed as the first parameter
 * eg:
 *
 * ppp dev:/dev/omap_csmi_tty0 <ppp options>
 *
 */
int ppp(int argc, char **argv)
{
#ifdef HAVE_WIN32_PROC
    fprintf(stderr, "error: adb %s not implemented on Win32\n", argv[0]);
    return -1;
#else
    char *adb_service_name;
    pid_t pid;
    int fd;

    if (argc < 2) {
        fprintf(stderr, "usage: adb %s <adb service name> [ppp opts]\n",
                argv[0]);

        return 1;
    }

    adb_service_name = argv[1];

    fd = adb_connect(adb_service_name);

    if(fd < 0) {
        fprintf(stderr,"Error: Could not open adb service: %s. Error: %s\n",
                adb_service_name, adb_error(client));
        return 1;
    }

    pid = fork();

    if (pid < 0) {
        perror("from fork()");
        return 1;
    } else if (pid == 0) {
        int err;
        int i;
        const char **ppp_args;

        // copy args
        ppp_args = (const char **) alloca(sizeof(char *) * argc + 1);
        ppp_args[0] = "pppd";
        for (i = 2 ; i < argc ; i++) {
            //argv[2] and beyond become ppp_args[1] and beyond
            ppp_args[i - 1] = argv[i];
        }
        ppp_args[i-1] = NULL;

        // child side

        dup2(fd, STDIN_FILENO);
        dup2(fd, STDOUT_FILENO);
        adb_close(STDERR_FILENO);
        adb_close(fd);

        err = execvp("pppd", (char * const *)ppp_args);

        if (err < 0) {
            perror("execing pppd");
        }
        exit(-1);
    } else {
        // parent side

        adb_close(fd);
        return 0;
    }
#endif /* !HAVE_WIN32_PROC */
}

static int send_shellcommand(adb_client_info * client, transport_type transport, char* serial, char* buf)
{
    int fd, ret;
	char * retstr=NULL;
    for(;;) {
        fd = adb_connect(client, buf);
        if(fd >= 0)
            break;
        fprintf(stderr,"- waiting for device -\n");
        adb_sleep_ms(1000);
        do_cmd(transport, serial, "wait-for-device", 0);
    }

    retstr = read_and_dump(client, fd);
	if ( retstr){
		if ( client->should_report)
		{
			char * failurepos = strstr(retstr, "Failure");
			if ( failurepos!=NULL )
			{
				report_to_cs(client, failurepos, "");

				//Add by CSC: return immediately when install apk is failed.
				free(retstr);
				adb_close(fd);
				return -1;
			}
		}
		free(retstr);
	}
	
    ret = adb_close(fd);
    if (ret)
        perror("close");

    return ret;
}

static int logcat(adb_client_info * client, transport_type transport, char* serial, int argc, char **argv)
{
    char buf[4096];

    char *log_tags;
    char *quoted_log_tags;

    log_tags = getenv("ANDROID_LOG_TAGS");
    quoted_log_tags = dupAndQuote(log_tags == NULL ? "" : log_tags);

    _snprintf(buf, sizeof(buf),
        "shell:export ANDROID_LOG_TAGS=\"\%s\" ; exec logcat",
        quoted_log_tags);

    free(quoted_log_tags);

    if (!strcmp(argv[0],"longcat")) {
        strncat(buf, " -v long", sizeof(buf)-1);
    }

    argc -= 1;
    argv += 1;
    while(argc-- > 0) {
        char *quoted;

        quoted = dupAndQuote (*argv++);

        strncat(buf, " ", sizeof(buf)-1);
        strncat(buf, quoted, sizeof(buf)-1);
        free(quoted);
    }

    send_shellcommand(client, transport, serial, buf);
    return 0;
}

static int mkdirs(char *path)
{
    int ret;
    char *x = path + 1;

    for(;;) {
        x = adb_dirstart(x);
        if(x == 0) return 0;
        *x = 0;
        ret = adb_mkdir(path, 0775);
        *x = OS_PATH_SEPARATOR;
        if((ret < 0) && (errno != EEXIST)) {
            return ret;
        }
        x++;
    }
    return 0;
}

static int backup(adb_client_info * client, int argc, char** argv) {
    char buf[4096];
    char default_name[32];
    const char* filename = strcpy(default_name, "./backup.ab");
    int fd, outFd;
    int i, j;

    /* find, extract, and use any -f argument */
    for (i = 1; i < argc; i++) {
        if (!strcmp("-f", argv[i])) {
            if (i == argc-1) {
                fprintf(stderr, "adb: -f passed with no filename\n");
                return usage();
            }
            filename = argv[i+1];
            for (j = i+2; j <= argc; ) {
                argv[i++] = argv[j++];
            }
            argc -= 2;
            argv[argc] = NULL;
        }
    }

    /* bare "adb backup" or "adb backup -f filename" are not valid invocations */
    if (argc < 2) return usage();

    adb_unlink(filename);
    mkdirs((char *)filename);
    outFd = adb_creat(filename, 0640);
    if (outFd < 0) {
        fprintf(stderr, "adb: unable to open file %s\n", filename);
        return -1;
    }

    _snprintf(buf, sizeof(buf), "backup");
    for (argc--, argv++; argc; argc--, argv++) {
        strncat(buf, ":", sizeof(buf) - strlen(buf) - 1);
        strncat(buf, argv[0], sizeof(buf) - strlen(buf) - 1);
    }

    D("backup. filename=%s buf=%s\n", filename, buf);
    fd = adb_connect(client, buf);
    if (fd < 0) {
        fprintf(stderr, "adb: unable to connect for backup\n");
        adb_close(outFd);
        return -1;
    }

    printf("Now unlock your device and confirm the backup operation.\n");
    copy_to_file(fd, outFd);

    adb_close(fd);
    adb_close(outFd);
    return 0;
}

static int restore(adb_client_info * client, int argc, char** argv) {
    const char* filename;
    int fd, tarFd;

    if (argc != 2) return usage();

    filename = argv[1];
    tarFd = adb_open(filename, O_RDONLY);
    if (tarFd < 0) {
        fprintf(stderr, "adb: unable to open file %s\n", filename);
        return -1;
    }

    fd = adb_connect(client, "restore:");
    if (fd < 0) {
        fprintf(stderr, "adb: unable to connect for backup\n");
        adb_close(tarFd);
        return -1;
    }

    printf("Now unlock your device and confirm the restore operation.\n");
    copy_to_file(tarFd, fd);

    adb_close(fd);
    adb_close(tarFd);
    return 0;
}

#define SENTINEL_FILE "config" OS_PATH_SEPARATOR_STR "envsetup.make"
static int top_works(const char *top)
{
    if (top != NULL && adb_is_absolute_host_path(top)) {
        char path_buf[MAX_PATH];
        _snprintf(path_buf, sizeof(path_buf),
                "%s" OS_PATH_SEPARATOR_STR SENTINEL_FILE, top);
        return access(path_buf, F_OK) == 0;
    }
    return 0;
}

static char *find_top_from(const char *indir, char path_buf[PATH_MAX])
{
    strcpy(path_buf, indir);
    while (1) {
		char *s;
        if (top_works(path_buf)) {
            return path_buf;
        }
        s = adb_dirstop(path_buf);
        if (s != NULL) {
            *s = '\0';
        } else {
            path_buf[0] = '\0';
            return NULL;
        }
    }
}

static char *find_top(char path_buf[PATH_MAX])
{
	char dir[PATH_MAX];
    char *top = getenv("ANDROID_BUILD_TOP");

    if (top != NULL && top[0] != '\0') {
        if (!top_works(top)) {
            fprintf(stderr, "adb: bad ANDROID_BUILD_TOP value \"%s\"\n", top);
            return NULL;
        }
    } else {
        top = getenv("TOP");
        if (top != NULL && top[0] != '\0') {
            if (!top_works(top)) {
                fprintf(stderr, "adb: bad TOP value \"%s\"\n", top);
                return NULL;
            }
        } else {
            top = NULL;
        }
    }

    if (top != NULL) {
        /* The environment pointed to a top directory that works.
         */
        strcpy(path_buf, top);
        return path_buf;
    }

    /* The environment didn't help.  Walk up the tree from the CWD
     * to see if we can find the top.
     */
    
    top = find_top_from(getcwd(dir, sizeof(dir)), path_buf);
    if (top == NULL) {
        /* If the CWD isn't under a good-looking top, see if the
         * executable is.
         */
        get_my_path(dir, PATH_MAX);
        top = find_top_from(dir, path_buf);
    }
    return top;
}

/* <hint> may be:
 * - A simple product name
 *   e.g., "sooner"
TODO: debug?  sooner-debug, sooner:debug?
 * - A relative path from the CWD to the ANDROID_PRODUCT_OUT dir
 *   e.g., "out/target/product/sooner"
 * - An absolute path to the PRODUCT_OUT dir
 *   e.g., "/src/device/out/target/product/sooner"
 *
 * Given <hint>, try to construct an absolute path to the
 * ANDROID_PRODUCT_OUT dir.
 */
static const char *find_product_out_path(const char *hint)
{
    static char path_buf[PATH_MAX];
	char top_buf[PATH_MAX];

	char *top;
    if (hint == NULL || hint[0] == '\0') {
        return NULL;
    }

    /* If it's already absolute, don't bother doing any work.
     */
    if (adb_is_absolute_host_path(hint)) {
        strcpy(path_buf, hint);
        return path_buf;
    }

    /* If there are any slashes in it, assume it's a relative path;
     * make it absolute.
     */
    if (adb_dirstart(hint) != NULL) {
        if (getcwd(path_buf, sizeof(path_buf)) == NULL) {
            fprintf(stderr, "adb: Couldn't get CWD: %s\n", strerror(errno));
            return NULL;
        }
        if (strlen(path_buf) + 1 + strlen(hint) >= sizeof(path_buf)) {
            fprintf(stderr, "adb: Couldn't assemble path\n");
            return NULL;
        }
        strcat(path_buf, OS_PATH_SEPARATOR_STR);
        strcat(path_buf, hint);
        return path_buf;
    }

    /* It's a string without any slashes.  Try to do something with it.
     *
     * Try to find the root of the build tree, and build a PRODUCT_OUT
     * path from there.
     */
    
    top = find_top(top_buf);

    if (top == NULL) {
        fprintf(stderr, "adb: Couldn't find top of build tree\n");
        return NULL;
    }
//TODO: if we have a way to indicate debug, look in out/debug/target/...
    _snprintf(path_buf, sizeof(path_buf),
            "%s" OS_PATH_SEPARATOR_STR
            "out" OS_PATH_SEPARATOR_STR
            "target" OS_PATH_SEPARATOR_STR
            "product" OS_PATH_SEPARATOR_STR
            "%s", top_buf, hint);
    if (access(path_buf, F_OK) < 0) {
        fprintf(stderr, "adb: Couldn't find a product dir "
                "based on \"-p %s\"; \"%s\" doesn't exist\n", hint, path_buf);
        return NULL;
    }
    return path_buf;
}

int adb_commandline(int argc, char **argv)
{
    char buf[4096];
    int no_daemon = 0;
    int is_daemon = 0;
    int is_server = 0;
    int persist = 0;
    int r;
    int quote;
    transport_type ttype = kTransportAny;
    char* serial = NULL;
    char* server_port_str = NULL;
	adb_client_info * client;
	int server_port;

        /* If defined, this should be an absolute path to
         * the directory containing all of the various system images
         * for a particular product.  If not defined, and the adb
         * command requires this information, then the user must
         * specify the path using "-p".
         */
    gProductOutPath = getenv("ANDROID_PRODUCT_OUT");
    if (gProductOutPath == NULL || gProductOutPath[0] == '\0') {
        gProductOutPath = NULL;
    }
    // TODO: also try TARGET_PRODUCT/TARGET_DEVICE as a hint

    serial = getenv("ANDROID_SERIAL");

    /* Validate and assign the server port */
    server_port_str = getenv("ANDROID_ADB_SERVER_PORT");
    server_port = DEFAULT_ADB_PORT;
    if (server_port_str && strlen(server_port_str) > 0) {
        server_port = (int) strtol(server_port_str, NULL, 0);
        if (server_port <= 0 || server_port > 65535) {
            fprintf(stderr,
                    "adb: Env var ANDROID_ADB_SERVER_PORT must be a positive number less than 65535. Got \"%s\"\n",
                    server_port_str);
            return usage();
        }
    }

    /* modifiers and flags */
    while(argc > 0) {
        if(!strcmp(argv[0],"server")) {
            is_server = 1;
        } else if(!strcmp(argv[0],"nodaemon")) {
            no_daemon = 1;
        } else if (!strcmp(argv[0], "fork-server")) {
            /* this is a special flag used only when the ADB client launches the ADB Server */
            is_daemon = 1;
        } else if(!strcmp(argv[0],"persist")) {
            persist = 1;
        } else if(!strncmp(argv[0], "-p", 2)) {
            const char *product = NULL;
            if (argv[0][2] == '\0') {
                if (argc < 2) return usage();
                product = argv[1];
                argc--;
                argv++;
            } else {
                product = argv[0] + 2;
            }
            gProductOutPath = find_product_out_path(product);
            if (gProductOutPath == NULL) {
                fprintf(stderr, "adb: could not resolve \"-p %s\"\n",
                        product);
                return usage();
            }
        } else if (argv[0][0]=='-' && argv[0][1]=='s') {
            if (isdigit(argv[0][2])) {
                serial = argv[0] + 2;
            } else {
                if(argc < 2 || argv[0][2] != '\0') return usage();
                serial = argv[1];
                argc--;
                argv++;
            }
        } else if (!strcmp(argv[0],"-d")) {
            ttype = kTransportUsb;
        } else if (!strcmp(argv[0],"-e")) {
            ttype = kTransportLocal;
        } else if (!strcmp(argv[0],"-a")) {
            gListenAll = 1;
        } else if(!strncmp(argv[0], "-H", 2)) {
            const char *hostname = NULL;
            if (argv[0][2] == '\0') {
                if (argc < 2) return usage();
                hostname = argv[1];
                argc--;
                argv++;
            } else {
                hostname = argv[0] + 2;
            }
            adb_set_tcp_name(hostname);

        } else if(!strncmp(argv[0], "-P", 2)) {
            if (argv[0][2] == '\0') {
                if (argc < 2) return usage();
                server_port_str = argv[1];
                argc--;
                argv++;
            } else {
                server_port_str = argv[0] + 2;
            }
            if (strlen(server_port_str) > 0) {
                server_port = (int) strtol(server_port_str, NULL, 0);
                if (server_port <= 0 || server_port > 65535) {
                    fprintf(stderr,
                            "adb: port number must be a positive number less than 65536. Got \"%s\"\n",
                            server_port_str);
                    return usage();
                }
            } else {
                fprintf(stderr,
                "adb: port number must be a positive number less than 65536. Got empty string.\n");
                return usage();
            }
        } else {
                /* out of recognized modifiers and flags */
            break;
        }
        argc--;
        argv++;
    }


    if (is_server) {
        if (no_daemon || is_daemon) {
            r = adb_main(is_daemon, server_port);
        } else {
            r = launch_server(server_port);
        }
        if(r) {
            fprintf(stderr,"* could not start server *\n");
        }

        return r;
    }

top:
    if(argc == 0) {
        return usage();
    }

    /* adb_connect() commands */

	client = (adb_client_info*)malloc(sizeof(adb_client_info));
	client->serial = serial;
	client->transtype = ttype;
	client->server_port = server_port;
	client->server_name = NULL;


    if(!strcmp(argv[0], "devices")) {
        char *tmp;
        char *listopt;
        if (argc < 2)
            listopt = "";
        else if (argc == 2 && !strcmp(argv[1], "-l"))
            listopt = argv[1];
        else {
            fprintf(stderr, "Usage: adb devices [-l]\n");
			free(client);
            return 1;
        }
        _snprintf(buf, sizeof buf, "host:%s%s", argv[0], listopt);
        tmp = adb_query(client, buf);
        if(tmp) {
            printf("List of devices attached \n");
            printf("%s\n", tmp);
			free(client);
            return 0;
        } else {
			free(client);
            return 1;
        }
    }

    if(!strcmp(argv[0], "connect")) {
        char *tmp;
        if (argc != 2) {
            fprintf(stderr, "Usage: adb connect <host>[:<port>]\n");
			free(client);
            return 1;
        }
        _snprintf(buf, sizeof buf, "host:connect:%s", argv[1]);
        tmp = adb_query(client, buf);
        if(tmp) {

            printf("%s\n", tmp);
			free(client);
            return 0;
        } else {
			free(client);
            return 1;
        }
    }

    if(!strcmp(argv[0], "disconnect")) {
        char *tmp;
        if (argc > 2) {
            fprintf(stderr, "Usage: adb disconnect [<host>[:<port>]]\n");
			free(client);
            return 1;
        }
        if (argc == 2) {
            _snprintf(buf, sizeof buf, "host:disconnect:%s", argv[1]);
        } else {
            _snprintf(buf, sizeof buf, "host:disconnect:");
        }
        tmp = adb_query(client, buf);
        if(tmp) {
            printf("%s\n", tmp);
			free(client);
            return 0;
        } else {
			free(client);
            return 1;
        }
    }

    if (!strcmp(argv[0], "emu")) {
        int ret= adb_send_emulator_command(client, argc, argv);
		free(client);
		return ret;
    }

    if(!strcmp(argv[0], "shell") || !strcmp(argv[0], "hell")) {
        int r;
        int fd;

        char h = (argv[0][0] == 'h');

        if (h) {
            printf("\x1b[41;33m");
            fflush(stdout);
        }

        if(argc < 2) {
            D("starting interactive shell\n");
            r = interactive_shell(client);
            if (h) {
                printf("\x1b[0m");
                fflush(stdout);
            }
			free(client);
            return r;
        }

        _snprintf(buf, sizeof buf, "shell:%s", argv[1]);
        argc -= 2;
        argv += 2;
        while(argc-- > 0) {
            strcat(buf, " ");

            /* quote empty strings and strings with spaces */
            quote = (**argv == 0 || strchr(*argv, ' '));
            if (quote)
                strcat(buf, "\"");
            strcat(buf, *argv++);
            if (quote)
                strcat(buf, "\"");
        }

        for(;;) {
            D("interactive shell loop. buff=%s\n", buf);
            fd = adb_connect(client, buf);
            if(fd >= 0) {
                D("about to read_and_dump(fd=%d)\n", fd);
                SAFE_FREE(read_and_dump(client, fd));
                D("read_and_dump() done.\n");
                adb_close(fd);
                r = 0;
            } else {
                fprintf(stderr,"error: %s\n", adb_error(client));
                r = -1;
            }

            if(persist) {
                fprintf(stderr,"\n- waiting for device -\n");
                adb_sleep_ms(1000);
                do_cmd(ttype, serial, "wait-for-device", 0);
            } else {
                if (h) {
                    printf("\x1b[0m");
                    fflush(stdout);
                }
                D("interactive shell loop. return r=%d\n", r);
				free(client);
                return r;
            }
        }
    }

    if(!strcmp(argv[0], "kill-server")) {
        int fd;
        fd = _adb_connect(client, "host:kill");
        if(fd == -1) {
            fprintf(stderr,"* server not running *\n");
			free(client);
            return 1;
        }
		free(client);
        return 0;
    }

    if(!strcmp(argv[0], "sideload")) {
        if(argc != 2) {
			free(client);
			return usage();
		}
        if(adb_download(client, "sideload", argv[1], 1)) {
			free(client);
            return 1;
        } else {
			free(client);
            return 0;
        }
    }

    if(!strcmp(argv[0], "remount") || !strcmp(argv[0], "reboot")
            || !strcmp(argv[0], "reboot-bootloader")
            || !strcmp(argv[0], "tcpip") || !strcmp(argv[0], "usb")
            || !strcmp(argv[0], "root")) {
        char command[100];
		int fd;
        if (!strcmp(argv[0], "reboot-bootloader"))
            _snprintf(command, sizeof(command), "reboot:bootloader");
        else if (argc > 1)
            _snprintf(command, sizeof(command), "%s:%s", argv[0], argv[1]);
        else
            _snprintf(command, sizeof(command), "%s:", argv[0]);
        fd = adb_connect(client, command);
        if(fd >= 0) {
            SAFE_FREE(read_and_dump(client, fd));
            adb_close(fd);
			free(client);
            return 0;
        }
        fprintf(stderr,"error: %s\n", adb_error(client));
		free(client);
        return 1;
    }

    if(!strcmp(argv[0], "bugreport")) {
        if (argc != 1){
			return usage();
			free(client);
		}
        do_cmd(ttype, serial, "shell", "bugreport", 0);
		free(client);
        return 0;
    }

    /* adb_command() wrapper commands */

    if(!strncmp(argv[0], "wait-for-", strlen("wait-for-"))) {
        char* service = argv[0];
        if (!strncmp(service, "wait-for-device", strlen("wait-for-device"))) {
            if (ttype == kTransportUsb) {
                service = "wait-for-usb";
            } else if (ttype == kTransportLocal) {
                service = "wait-for-local";
            } else {
                service = "wait-for-any";
            }
        }

        format_host_command(buf, sizeof buf, service, ttype, serial);

        if (adb_command(client, buf)) {
            D("failure: %s *\n",adb_error(client));
            fprintf(stderr,"error: %s\n", adb_error(client));
			free(client);
            return 1;
        }

        /* Allow a command to be run after wait-for-device,
            * e.g. 'adb wait-for-device shell'.
            */
        if(argc > 1) {
            argc--;
            argv++;
            goto top;
        }
		free(client);
        return 0;
    }

    if(!strcmp(argv[0], "forward")) {
        char host_prefix[64];
        char remove = 0;
        char remove_all = 0;
        char list = 0;
        char no_rebind = 0;

        // Parse options here.
        while (argc > 1 && argv[1][0] == '-') {
            if (!strcmp(argv[1], "--list"))
                list = 1;
            else if (!strcmp(argv[1], "--remove"))
                remove = 1;
            else if (!strcmp(argv[1], "--remove-all"))
                remove_all = 1;
            else if (!strcmp(argv[1], "--no-rebind"))
                no_rebind = 1;
            else {
				free(client);
                return usage();
            }
            argc--;
            argv++;
        }

        // Ensure we can only use one option at a time.
        if (list + remove + remove_all + no_rebind > 1) {
			free(client);
            return usage();
        }

        // Determine the <host-prefix> for this command.
        if (serial) {
            _snprintf(host_prefix, sizeof host_prefix, "host-serial:%s",
                    serial);
        } else if (ttype == kTransportUsb) {
            _snprintf(host_prefix, sizeof host_prefix, "host-usb");
        } else if (ttype == kTransportLocal) {
            _snprintf(host_prefix, sizeof host_prefix, "host-local");
        } else {
            _snprintf(host_prefix, sizeof host_prefix, "host");
        }

        // Implement forward --list
        if (list) {
			char* forwards;
            if (argc != 1)
                return usage();
            _snprintf(buf, sizeof buf, "%s:list-forward", host_prefix);
            forwards = adb_query(client, buf);
            if (forwards == NULL) {
                fprintf(stderr, "error: %s\n", adb_error(client));
                return 1;
            }
            printf("%s", forwards);
            free(forwards);
            return 0;
        }

        // Implement forward --remove-all
        else if (remove_all) {
            if (argc != 1){
				free(client);
                return usage();
			}
            _snprintf(buf, sizeof buf, "%s:killforward-all", host_prefix);
        }

        // Implement forward --remove <local>
        else if (remove) {
            if (argc != 2){
				free(client);
                return usage();
			}
            _snprintf(buf, sizeof buf, "%s:killforward:%s", host_prefix, argv[1]);
        }
        // Or implement one of:
        //    forward <local> <remote>
        //    forward --no-rebind <local> <remote>
        else
        {
		  const char* 	command = no_rebind ? "forward:norebind:" : "forward";
          if (argc != 3){
			  free(client);
            return usage();
		  }
          
          _snprintf(buf, sizeof buf, "%s:%s:%s;%s", host_prefix, command, argv[1], argv[2]);
        }

        if(adb_command(client, buf)) {
            fprintf(stderr,"error: %s\n", adb_error(client));
			free(client);
            return 1;
        }
		free(client);
        return 0;
    }

    /* do_sync_*() commands */

    if(!strcmp(argv[0], "ls")) {
		int ret;
        if(argc != 2){ 
			free(client);
			return usage();
		}
        ret = do_sync_ls(client, argv[1]);
		free(client);
		return ret;
    }

    if(!strcmp(argv[0], "push")) {
		int ret;
        if(argc != 3){
			free(client);
			return usage();
		}
        ret = do_sync_push(client, argv[1], argv[2], 0 /* no verify APK */);
		free(client);
		return ret;
    }

    if(!strcmp(argv[0], "pull")) {
        if (argc == 2) {
            int ret =do_sync_pull(client, argv[1], ".");
			free(client);
			return ret;
        } else if (argc == 3) {
            int ret = do_sync_pull(client, argv[1], argv[2]);
			free(client);
			return ret;
        } else {
			free(client);
            return usage();
        }
    }

    if(!strcmp(argv[0], "install")) {
		int ret;
        if (argc < 2) {
			free(client);
			return usage();
		}
		client->should_report = 1;
        ret = install_app(client, ttype, serial, argc, argv);
		free(client);
		return ret;
    }

	if(!strcmp(argv[0], "uninstall")) {
		int ret;
        if (argc < 2) {
			free(client);
			return usage();
		}
		ret = uninstall_app(client, ttype, serial, argc, argv);
		free(client);
		return ret;
    }

    if(!strcmp(argv[0], "sync")) {
        char *srcarg, *android_srcpath, *data_srcpath;
        int listonly = 0;

        int ret;
        if(argc < 2) {
            /* No local path was specified. */
            srcarg = NULL;
        } else if (argc >= 2 && strcmp(argv[1], "-l") == 0) {
            listonly = 1;
            if (argc == 3) {
                srcarg = argv[2];
            } else {
                srcarg = NULL;
            }
        } else if(argc == 2) {
            /* A local path or "android"/"data" arg was specified. */
            srcarg = argv[1];
        } else {
			free(client);
            return usage();
        }
        ret = find_sync_dirs(srcarg, &android_srcpath, &data_srcpath);
        if(ret != 0){
			free(client);
			return usage();
		}

        if(android_srcpath != NULL)
            ret = do_sync_sync(client, android_srcpath, "/system", listonly);
        if(ret == 0 && data_srcpath != NULL)
            ret = do_sync_sync(client, data_srcpath, "/data", listonly);

        free(android_srcpath);
        free(data_srcpath);
		free(client);
        return ret;
    }

    /* passthrough commands */

    if(!strcmp(argv[0],"get-state") ||
        !strcmp(argv[0],"get-serialno") ||
        !strcmp(argv[0],"get-devpath"))
    {
        char *tmp;

        format_host_command(buf, sizeof buf, argv[0], ttype, serial);
        tmp = adb_query(client, buf);
        if(tmp) {
            printf("%s\n", tmp);
			free(client);
            return 0;
        } else {
			free(client);
            return 1;
        }
    }

    /* other commands */

    if(!strcmp(argv[0],"status-window")) {
        status_window(client, ttype, serial);
		free(client);
        return 0;
    }

    if(!strcmp(argv[0],"logcat") || !strcmp(argv[0],"lolcat") || !strcmp(argv[0],"longcat")) {
        int ret = logcat(client, ttype, serial, argc, argv);
		free(client);
		return ret;
    }

    if(!strcmp(argv[0],"ppp")) {
        int ret = ppp(argc, argv);
		free(client);
		return ret;
    }

    if (!strcmp(argv[0], "start-server")) {
        int ret = adb_connect(client, "host:start-server");
		free(client);
		return ret;
    }

    if (!strcmp(argv[0], "backup")) {
		int ret = backup(client, argc, argv);
		free(client);
		return ret;
    }

    if (!strcmp(argv[0], "restore")) {
		int ret = restore(client, argc, argv);
		free(client);
		return ret;
    }

    if (!strcmp(argv[0], "jdwp")) {
        int  fd = adb_connect(client, "jdwp");
        if (fd >= 0) {
            SAFE_FREE(read_and_dump(client, fd));
            adb_close(fd);
			free(client);
            return 0;
        } else {
            fprintf(stderr, "error: %s\n", adb_error(client));
			free(client);
            return -1;
        }
    }

    /* "adb /?" is a common idiom under Windows */
    if(!strcmp(argv[0], "help") || !strcmp(argv[0], "/?")) {
        help();
		free(client);
        return 0;
    }

    if(!strcmp(argv[0], "version")) {
        version(stdout);
		free(client);
        return 0;
    }

    usage();
	free(client);
    return 1;
}

static int do_cmd(transport_type ttype, char* serial, char *cmd, ...)
{
    char *argv[16];
    int argc;
    va_list ap;

    va_start(ap, cmd);
    argc = 0;

    if (serial) {
        argv[argc++] = "-s";
        argv[argc++] = serial;
    } else if (ttype == kTransportUsb) {
        argv[argc++] = "-d";
    } else if (ttype == kTransportLocal) {
        argv[argc++] = "-e";
    }

    argv[argc++] = cmd;
    while((argv[argc] = va_arg(ap, char*)) != 0) argc++;
    va_end(ap);

#if 0
    int n;
    fprintf(stderr,"argc = %d\n",argc);
    for(n = 0; n < argc; n++) {
        fprintf(stderr,"argv[%d] = \"%s\"\n", n, argv[n]);
    }
#endif

    return adb_commandline(argc, argv);
}

int find_sync_dirs(const char *srcarg,
        char **android_srcdir_out, char **data_srcdir_out)
{
    char *android_srcdir, *data_srcdir;

    if(srcarg == NULL) {
        android_srcdir = product_file("system");
        data_srcdir = product_file("data");
    } else {
        /* srcarg may be "data", "system" or NULL.
         * if srcarg is NULL, then both data and system are synced
         */
        if(strcmp(srcarg, "system") == 0) {
            android_srcdir = product_file("system");
            data_srcdir = NULL;
        } else if(strcmp(srcarg, "data") == 0) {
            android_srcdir = NULL;
            data_srcdir = product_file("data");
        } else {
            /* It's not "system" or "data".
             */
            return 1;
        }
    }

    if(android_srcdir_out != NULL)
        *android_srcdir_out = android_srcdir;
    else
        free(android_srcdir);

    if(data_srcdir_out != NULL)
        *data_srcdir_out = data_srcdir;
    else
        free(data_srcdir);

    return 0;
}

static int pm_command(adb_client_info* client, transport_type transport, char* serial,
                      int argc, char** argv)
{
    char buf[4096];

    _snprintf(buf, sizeof(buf), "shell:pm");

    while(argc-- > 0) {
        char *quoted;

        quoted = dupAndQuote(*argv++);

        strncat(buf, " ", sizeof(buf)-1);
        strncat(buf, quoted, sizeof(buf)-1);
        free(quoted);
    }

	//Modified by CSC: To check if installation is failed.
    //return 0;
	return send_shellcommand(client, transport, serial, buf);
}

int uninstall_app(adb_client_info * client, transport_type transport, char* serial, int argc, char** argv)
{
	int ret = -1;
    /* if the user choose the -k option, we refuse to do it until devices are
       out with the option to uninstall the remaining data somehow (adb/ui) */
    if (argc == 3 && strcmp(argv[1], "-k") == 0)
    {
        printf(
            "The -k option uninstalls the application while retaining the data/cache.\n"
            "At the moment, there is no way to remove the remaining data.\n"
            "You will have to reinstall the application with the same signature, and fully uninstall it.\n"
            "If you truly wish to continue, execute 'adb shell pm uninstall -k %s'\n", argv[2]);
        return -1;
    }

    /* 'adb uninstall' takes the same arguments as 'pm uninstall' on device */

  	if (strcmp(argv[2],"1") == 0) {
  		report_to_cs(client,"UninstallApkStart", argv[3]);
  	}
    ret = pm_command(client, transport, serial, argc, argv);
	if (ret > -1) {
		if (strcmp(argv[2],"1") == 0) {
			report_to_cs(client,"UninstallApkFinish", argv[3]);
		}
	} else {
		report_to_cs(client,"UninstallApkFail", argv[3]);
	}
 	return ret;
}

static int delete_file(adb_client_info * client, transport_type transport, char* serial, char* filename)
{
    char buf[4096];
    char* quoted;

    _snprintf(buf, sizeof(buf), "shell:rm ");
    quoted = dupAndQuote(filename);
    strncat(buf, quoted, sizeof(buf)-1);
    free(quoted);

    send_shellcommand(client, transport, serial, buf);
    return 0;
}

static const char* get_basename(const char* filename)
{
    const char* basename = adb_dirstop(filename);
    if (basename) {
        basename++;
        return basename;
    } else {
        return filename;
    }
}

static int check_file(const char* filename)
{
    struct stat st;

    if (filename == NULL) {
        return 0;
    }

    if (stat(filename, &st) != 0) {
        fprintf(stderr, "can't find '%s' to install\n", filename);
        return 1;
    }

    if (!S_ISREG(st.st_mode)) {
        fprintf(stderr, "can't install '%s' because it's not a file\n", filename);
        return 1;
    }

    return 0;
}

int install_app(adb_client_info * client, transport_type transport, char* serial, int argc, char** argv)
{
    static const char *const DATA_DEST = "/data/local/tmp/%s";
    static const char *const SD_DEST = "/sdcard/tmp/%s";
    const char* where = DATA_DEST;
    char apk_dest[PATH_MAX];
    char verification_dest[PATH_MAX];
    char* apk_file;
    char* verification_file = NULL;
    int file_arg = -1;
    int err = 0;
    int i;
    int verify_apk = 1;
	int ret = 0;	//Add by CSC
	char chkCmd[100];	//Add by CSC
	int vCode = 0;
	char *versionCodeIndex;
	char versionName[50] = {'\0'};
	int fd;
	int r;
	char buf[4100];
	char * resultstr = NULL;
	int totallen = 0;
	int flgupdate = 0;
	char *tmp;
	char *vnameend;

    for (i = 1; i < argc; i++) {
        if (*argv[i] != '-') {
            file_arg = i;
            break;
        } else if (!strcmp(argv[i], "-i")) {
            // Skip the installer package name.
			i++;
		} else if (!strcmp(argv[i], "-s")) {
			where = SD_DEST;
		} else if (!strcmp(argv[i], "--algo")) {
            verify_apk = 0;
            i++;
        } else if (!strcmp(argv[i], "--iv")) {
            verify_apk = 0;
            i++;
        } else if (!strcmp(argv[i], "--key")) {
            verify_apk = 0;
            i++;
        }
    }


    if (file_arg < 0) {
        fprintf(stderr, "can't find filename in arguments\n");
        return 1;
    } 
	//Comment by CSC : To use location parameter, 
	/*
	else if (file_arg + 2 < argc) {
        fprintf(stderr, "too many files specified; only takes APK file and verifier file\n");
        return 1;
    }
	*/

    apk_file = argv[file_arg];

	//Add by CSC: To check apk version
	if (argc > 4) {
		_snprintf(chkCmd,100,"shell:dumpsys package %s",argv[argc-2]);
		fd = adb_connect(client, chkCmd);

		if(fd >= 0) {
			int len;
			while( 1 ) {
				len = adb_read(fd, buf, 4096);
				if(len == 0) {
					break;
				}

				if(len < 0) {
					if(errno == EINTR) continue;
					break;
				}
				buf[len]=0;
				if ( totallen == 0){
					resultstr = (char*)malloc(len+1);
					strcpy(resultstr, buf);
					totallen = len+1;
				}
				else {
					char * newstr;
					totallen += len+1;
					newstr = (char*)malloc(totallen);
					strcpy(newstr, resultstr);
					free(resultstr);
					resultstr = newstr;
					strcat(resultstr, buf);
				}
			}
			adb_close(fd);
			r = 0;
		} else {
			fprintf(stderr,"error: %s\n", adb_error(client));
			r = -1;
		}

		if(resultstr) {
			versionCodeIndex = strstr(resultstr, "versionCode=");
			tmp = strstr(resultstr, "versionName=");
			if (tmp) {
				sscanf(tmp, "versionName=%s", versionName);
			}
			if (versionCodeIndex) {
				vCode = atoi(versionCodeIndex + 12);

				if (vCode < atoi(argv[argc-4]))
				{
					flgupdate = 1;
				}
			}
		}
	}


//Comment by CSC : To avoid check verification_file
//     if (file_arg != argc - 1) {
//         verification_file = argv[file_arg + 1];
//     }

//    if (check_file(apk_file) || check_file(verification_file)) {
	if (check_file(apk_file)) {
        return 1;
    }

    _snprintf(apk_dest, sizeof apk_dest, where, get_basename(apk_file));
    if (verification_file != NULL) {
        _snprintf(verification_dest, sizeof(verification_dest), where, get_basename(verification_file));

        if (!strcmp(apk_dest, verification_dest)) {
            fprintf(stderr, "APK and verification file can't have the same name\n");
            return 1;
        }
    }

	report_to_cs(client,"PushApk", argv[argc-1]);
    err = do_sync_push(client, apk_file, apk_dest, verify_apk);
    if (err) {
		report_to_cs(client,"Failure:[UNABLE_TO_LOAD_APK]", "");
		return err;
        //goto cleanup_apk;
    } else {
        argv[file_arg] = apk_dest; /* destination name, not source location */
    }

    if (verification_file != NULL) {
        err = do_sync_push(client, verification_file, verification_dest, 0 /* no verify APK */);
        if (err) {
            goto cleanup_apk;
        } else {
            argv[file_arg + 1] = verification_dest; /* destination name, not source location */
        }
    }

//Comment by CSC
 	report_to_cs(client,"PkgInstall", argv[argc-1]);

	//Modified by CSC: Used ret variable to get return value (success/fail)
	//pm_command(client,transport, serial, argc, argv);
	ret = pm_command(client,transport, serial, argc, argv);

cleanup_apk:
    if (verification_file != NULL) {
        delete_file(client, transport, serial, verification_dest);
    }

//Comment by CSC
	report_to_cs(client,"DeleteTemp", argv[argc-1]);

    delete_file(client, transport, serial, apk_dest);

//Comment by CSC
	if (ret > -1) {
		if (flgupdate == 1) {
			char descstr[300] = {'\0'};
			_snprintf(descstr, sizeof(descstr), "%s#%s#%s", argv[argc-1], versionName, argv[argc-3]);
			report_to_cs(client,"UpgradeApk", descstr);
		} else {
			report_to_cs(client,"Finished", argv[argc-1]);
		}
	}
    return err;
}

adb_client_info * init_adb_client_info(char * serial)
{

	adb_client_info* client = malloc(sizeof(adb_client_info));
	memset(client, 0, sizeof(adb_client_info));
	client->serial = serial;
	client->transtype = kTransportAny;
	client->server_port = DEFAULT_ADB_PORT;
	client->server_name = NULL;
	return client;
}

char* ac_get_devices( )
{
	int ret;
	adb_client_info * client; 
	adb_trace_init();
	adb_sysdeps_init();
	client = init_adb_client_info(NULL);
	ret = adb_query(client, "host:devices");
	free(client);
	return ret;

}


int getImageHeaderSize(int version) 
{
	switch (version) {
	case 16: // compatibility mode
		return 3; // size, width, height
	case 1:
		return 12; // bpp, size, width, height, 4*(length, offset)
	}

	return 0;
}

typedef struct tdfAndroidImageHeader {
	int version;
	int bpp;
	int size;
	int width;
	int height;
	int red_offset;
	int red_length;
	int blue_offset;
	int blue_length;
	int green_offset;
	int green_length;
	int alpha_offset;
	int alpha_length;
} AndroidImageHeader;

AndroidImageHeader readHeader(int version, char buf[])
{
	AndroidImageHeader resultImageHeader;
	int nowIndex = 0;	



	if (version == 16) {
		// compatibility mode with original protocol
		resultImageHeader.bpp = 16;

		// read actual values.
		//resultImageHeader.size = buf.getInt();
		resultImageHeader.size = strtoul(buf, 0, 16);
		resultImageHeader.width = strtoul(buf + 4, 0, 16);
		resultImageHeader.height = strtoul(buf + 8, 0, 16);

		// create default values for the rest. Format is 565
		resultImageHeader.red_offset = 11;
		resultImageHeader.red_length = 5;
		resultImageHeader.green_offset = 5;
		resultImageHeader.green_length = 6;
		resultImageHeader.blue_offset = 0;
		resultImageHeader.blue_length = 5;
		resultImageHeader.alpha_offset = 0;
		resultImageHeader.alpha_length = 0;
	} else if (version == 1) {		
		resultImageHeader.bpp = strtoul(buf, 0, 16);
		resultImageHeader.size = strtoul(buf + 4, 0, 16);
		resultImageHeader.width = strtoul(buf + 8, 0, 16);
		resultImageHeader.height = strtoul(buf + 12, 0, 16);
		resultImageHeader.red_offset = strtoul(buf + 16, 0, 16);
		resultImageHeader.red_length = strtoul(buf + 20, 0, 16);
		resultImageHeader.blue_offset = strtoul(buf + 24, 0, 16);
		resultImageHeader.blue_length = strtoul(buf + 28, 0, 16);
		resultImageHeader.green_offset = strtoul(buf + 32, 0, 16);
		resultImageHeader.green_length = strtoul(buf + 36, 0, 16);
		resultImageHeader.alpha_offset = strtoul(buf + 40, 0, 16);
		resultImageHeader.alpha_length = strtoul(buf + 44, 0, 16);
	} 
}

int strtoIntReverse(unsigned char str[])
{
	unsigned long result = 0;
	result += str[3];
	result <<= 8;
	result += str[2];
	result <<= 8;
	result += str[1];
	result <<= 8;
	result += str[0];	
	return result;
}

#define MAXFNAMELEN 64

char* ac_get_screen_image(char * hardwareid)
{
	AndroidImageHeader tmpImageHeader;
	int	fd;	
	adb_client_info * client; 
	char buf[5];
	unsigned version;	
	int headerSize;
	char reply[100];
	char valStr[5];
	char nudge[1] = {0};
	int len;
	BYTE *imageData, *tmpImageData;
	FILE *outFile;
	int i, j;
	int pixelSize;
	BYTE blueByte, greenByte, redByte, alphaByte;
	unsigned long long tmpPixelData, pixelData, bitMask;
	int bytesPerPixel, bitsPerColor;
	int nowByteIndex;

	char outBmpFile[MAXFNAMELEN];

	int imageWidth, imageHeight;
	int imageFileSize;
	unsigned char bmpfileheader[14] = {'B','M', 0,0,0,0, 0,0, 0,0, 54,0,0,0};
	unsigned char bmpinfoheader[40] = {40,0,0,0, 0,0,0,0, 0,0,0,0, 1,0, 32,0};
	unsigned char bmppad[4] = {0,0,0,0};	

	adb_trace_init();
	adb_sysdeps_init();
	client = init_adb_client_info(hardwareid);
	//ret = adb_query(client, "framebuffer:");
	fd = _adb_connect(client, "framebuffer:");

	if(fd < 0)
		return;

	// getting response
	if(readx(fd, buf, 4)) goto oops;

	buf[4] = 0;
	version = buf[0];
	//version = strtoul(buf, 0, 16);
	if(version > 1024) goto oops;


	headerSize = getImageHeaderSize(version);	

	if(readx(fd, reply, 4 * headerSize)) goto oops;

	valStr[4] = 0;
	if (version == 16) {
		// compatibility mode with original protocol
		tmpImageHeader.bpp = 16;

		// read actual values.
		//resultImageHeader.size = reply.getInt();
		strncpy(valStr, reply, 4);
		tmpImageHeader.size = strtoIntReverse(valStr);
		strncpy(valStr, reply + 4, 4);
		tmpImageHeader.width = strtoIntReverse(valStr);
		strncpy(valStr, reply + 8, 4);
		tmpImageHeader.height = strtoIntReverse(valStr);

		// create default values for the rest. Format is 565
		tmpImageHeader.red_offset = 11;
		tmpImageHeader.red_length = 5;
		tmpImageHeader.green_offset = 5;
		tmpImageHeader.green_length = 6;
		tmpImageHeader.blue_offset = 0;
		tmpImageHeader.blue_length = 5;
		tmpImageHeader.alpha_offset = 0;
		tmpImageHeader.alpha_length = 0;
	} else if (version == 1) {		
		memcpy(valStr, reply, 4 * sizeof(char));
		tmpImageHeader.bpp = strtoIntReverse(valStr);
		memcpy(valStr, reply + 4, 4 * sizeof(char));
		tmpImageHeader.size = strtoIntReverse(valStr);
		memcpy(valStr, reply + 8, 4 * sizeof(char));
		tmpImageHeader.width = strtoIntReverse(valStr);
		memcpy(valStr, reply + 12, 4 * sizeof(char));
		tmpImageHeader.height = strtoIntReverse(valStr);
		memcpy(valStr, reply + 16, 4 * sizeof(char));
		tmpImageHeader.red_offset = strtoIntReverse(valStr);
		memcpy(valStr, reply + 20, 4 * sizeof(char));
		tmpImageHeader.red_length = strtoIntReverse(valStr);
		memcpy(valStr, reply + 24, 4 * sizeof(char));
		tmpImageHeader.blue_offset = strtoIntReverse(valStr);
		memcpy(valStr, reply + 28, 4 * sizeof(char));
		tmpImageHeader.blue_length = strtoIntReverse(valStr);
		memcpy(valStr, reply + 32, 4 * sizeof(char));
		tmpImageHeader.green_offset = strtoIntReverse(valStr);
		memcpy(valStr, reply + 36, 4 * sizeof(char));
		tmpImageHeader.green_length = strtoIntReverse(valStr);
		memcpy(valStr, reply + 40, 4 * sizeof(char));
		tmpImageHeader.alpha_offset = strtoIntReverse(valStr);
		memcpy(valStr, reply + 44, 4 * sizeof(char));
		tmpImageHeader.alpha_length = strtoIntReverse(valStr);
	} 

	bytesPerPixel = tmpImageHeader.bpp / 8;
	bitsPerColor = tmpImageHeader.bpp / 4;
		
	if(writex(fd, nudge, 1)) goto oops;		

	imageData = (BYTE*)malloc(sizeof(BYTE) * tmpImageHeader.size);
	tmpImageData = (BYTE*)malloc(sizeof(BYTE) * tmpImageHeader.size);

	if(readx(fd, imageData, tmpImageHeader.size)) goto oops;
	//memcpy(tmpImageData, imageData, tmpImageHeader.size);
	pixelSize = tmpImageHeader.width * tmpImageHeader.height;
	nowByteIndex = 0;
	
	bitMask = 1;
	for(i = 0; i < bitsPerColor - 1; i++)
	{
		bitMask <<= 1;
		bitMask |= 1;
	}
	for(i = 0; i < pixelSize; i++)
	{	
		//tmpPixelData = 0;	
		memset(&tmpPixelData, 0, sizeof(long long));
		nowByteIndex = i * bytesPerPixel;
		for(j = 0; j < bytesPerPixel; j++)
		{
			tmpPixelData |= imageData[nowByteIndex++];
			tmpPixelData <<= 8;
		}
		tmpPixelData >>= 8;

		blueByte = (tmpPixelData >> (tmpImageHeader.bpp - tmpImageHeader.blue_offset - bitsPerColor)) & bitMask;
		greenByte = (tmpPixelData >> (tmpImageHeader.bpp - tmpImageHeader.green_offset - bitsPerColor)) & bitMask;
		redByte = (tmpPixelData >> (tmpImageHeader.bpp - tmpImageHeader.red_offset - bitsPerColor)) & bitMask;
		alphaByte = (tmpPixelData >> (tmpImageHeader.bpp - tmpImageHeader.alpha_offset - bitsPerColor)) & bitMask;

		pixelData = 0;
		pixelData |= blueByte;
		pixelData <<= bitsPerColor;
		pixelData |= greenByte;
		pixelData <<= bitsPerColor;
		pixelData |= redByte;
		pixelData <<= bitsPerColor;
		pixelData |= alphaByte;

		nowByteIndex = i * bytesPerPixel + bytesPerPixel - 1;
		for(j = 0; j < bytesPerPixel; j++)
		{
			tmpPixelData = pixelData;
			tmpPixelData &= bitMask;			
			pixelData >>= bitsPerColor;
			tmpImageData[nowByteIndex--] = tmpPixelData;
		}

// 		blueByte = imageData[i * 4 + tmpImageHeader.blue_offset / 8];
// 		greenByte = imageData[i * 4 + tmpImageHeader.green_offset / 8];
// 		redByte = imageData[i * 4 + tmpImageHeader.red_offset / 8];
// 		alphaByte = imageData[i * 4 + tmpImageHeader.alpha_offset / 8];
// 		tmpImageData[i * 4] = blueByte;
// 		tmpImageData[i * 4 + 1] = greenByte;
// 		tmpImageData[i * 4 + 2] = redByte;
// 		tmpImageData[i * 4 + 3] = alphaByte;
	}

	imageHeight = tmpImageHeader.height;
	imageWidth = tmpImageHeader.width;
	imageFileSize = 54 + 3 * imageWidth * imageHeight;

	memset(outBmpFile, 0, MAXFNAMELEN);
	strncpy(outBmpFile, "androidFbImage.bmp", sizeof(outBmpFile));
	outFile = fopen(outBmpFile, "wb+");

	bmpfileheader[2] = (unsigned char)(imageFileSize);
	bmpfileheader[3] = (unsigned char)(imageFileSize>>8);
	bmpfileheader[4] = (unsigned char)(imageFileSize>>16);
	bmpfileheader[5] = (unsigned char)(imageFileSize>>24);

	bmpinfoheader[4] = (unsigned char)(imageWidth);
	bmpinfoheader[5] = (unsigned char)(imageWidth>> 8);
	bmpinfoheader[6] = (unsigned char)(imageWidth>>16);
	bmpinfoheader[7] = (unsigned char)(imageWidth>>24);
	bmpinfoheader[8] = (unsigned char)(imageHeight);
	bmpinfoheader[9] = (unsigned char)(imageHeight>> 8);
	bmpinfoheader[10] = (unsigned char)(imageHeight>>16);
	bmpinfoheader[11] = (unsigned char)(imageHeight>>24);
	bmpinfoheader[14] = (unsigned char)(tmpImageHeader.bpp);

	fwrite(bmpfileheader,1,14,outFile);
	fwrite(bmpinfoheader,1,40,outFile);

	/*fwrite(imageData, 1, tmpImageHeader.size, outFile);*/
	for(i=0; i<imageHeight; i++)
	{
		fwrite(tmpImageData+(imageWidth*(imageHeight-i-1)*bytesPerPixel),bytesPerPixel,imageWidth,outFile);
		//fwrite(bmppad,1,(5-(imageWidth*4)%5)%5,outFile);
	}
	fclose(outFile);
	free(imageData);
	free(tmpImageData);
	return 0;

oops:
	adb_close(fd);

	free(client);
	return 0;

}

int ac_reboot(char * hardwareid)
{
	int fd;
	char buf[4100];
	adb_client_info * client;
	adb_trace_init();
	adb_sysdeps_init();
	client = init_adb_client_info(hardwareid);
	fd = adb_connect( client, "reboot:");
	if ( fd >=0 )
	{
		SAFE_FREE(read_and_dump(client, fd));
		adb_close(fd);
		free(client);
		return 0;
	}
	free(client);
	return -1;

}

char * ac_get_shell_cmd_result(char * hardwareid, char * cmd)
{
	int fd;
	int r;
	char buf[4100];
	char * resultstr = NULL;
	int totallen = 0;
	adb_client_info * client;
	adb_trace_init();
	adb_sysdeps_init();
	client = init_adb_client_info(hardwareid);

	fd = adb_connect(client, cmd);
	
	if(fd >= 0) {
		int len;
		while( 1 ) {
			len = adb_read(fd, buf, 4096);
			if(len == 0) {
				break;
			}

			if(len < 0) {
				if(errno == EINTR) continue;
				break;
			}
			buf[len]=0;
			if ( totallen == 0){
				resultstr = (char*)malloc(len+1);
				strcpy(resultstr, buf);
				totallen = len+1;
			}
			else {
				char * newstr;
				totallen += len+1;
				newstr = (char*)malloc(totallen);
				strcpy(newstr, resultstr);
				free(resultstr);
				resultstr = newstr;
				strcat(resultstr, buf);
			}
		}
		adb_close(fd);
		r = 0;
	} else {
		fprintf(stderr,"error: %s\n", adb_error(client));
		r = -1;
	}
	free(client);
	return resultstr;
}



int ac_install_app(char * hardwareid, char * apkpath)
{
	int argc = 2;
	adb_client_info * client;
	char * argv[2]={"install", NULL};
	adb_trace_init();
	adb_sysdeps_init();
	client = init_adb_client_info(hardwareid);
	argv[1] = apkpath;
	return install_app(client, client->transtype, client->serial, argc, argv);
}

int ac_update_app(char * hardwareid, char * apkpath)
{
	int argc = 3;
	adb_client_info * client;
	char * argv[3]={"install", "-r", NULL};
	adb_trace_init();
	adb_sysdeps_init();
	client = init_adb_client_info(hardwareid);
	argv[2] = apkpath;
	return install_app(client, client->transtype, client->serial, argc, argv);
}