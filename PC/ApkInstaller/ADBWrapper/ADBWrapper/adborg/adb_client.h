#ifndef _ADB_CLIENT_H_
#define _ADB_CLIENT_H_

#include "adb.h"


typedef struct syncsendbuf syncsendbuf;

#define SYNC_DATA_MAX (64*1024)
struct syncsendbuf {
	unsigned id;
	unsigned size;
	char data[SYNC_DATA_MAX];
};

#define MAX_SERIAL_NO	64
typedef struct _tag_REPORT_MESSAGE{
	unsigned int message;
	char		 serial[MAX_SERIAL_NO];
	unsigned int wparam;
	unsigned int lparam;
} report_message;


typedef struct tag_adb_client_info{
	transport_type	transtype;
	const char*		serial;
	int				server_port;
	const char*		server_name;
	syncsendbuf		send_buffer;
	char			adb_error[256];
	int				should_report;
} adb_client_info;
/* connect to adb, connect to the named service, and return
** a valid fd for interacting with that service upon success
** or a negative number on failure
*/
int adb_connect(adb_client_info * client, const char *service);
int _adb_connect(adb_client_info * client, const char *service);

/* connect to adb, connect to the named service, return 0 if
** the connection succeeded AND the service returned OKAY
*/
int adb_command(adb_client_info * client, const char *service);

/* connect to adb, connect to the named service, return
** a malloc'd string of its response upon success or NULL
** on failure.
*/
char *adb_query(adb_client_info * client, const char *service);

/* Set the preferred transport to connect to.
*/
void adb_set_transport(transport_type type, const char* serial);

/* Set TCP specifics of the transport to use
*/
void adb_set_tcp_specifics(int server_port);

/* Set TCP Hostname of the transport to use
*/
void adb_set_tcp_name(const char* hostname);

/* Return the console port of the currently connected emulator (if any)
 * of -1 if there is no emulator, and -2 if there is more than one.
 * assumes adb_set_transport() was alled previously...
 */
int  adb_get_emulator_console_port(void);

/* send commands to the current emulator instance. will fail if there
 * is zero, or more than one emulator connected (or if you use -s <serial>
 * with a <serial> that does not designate an emulator)
 */
int  adb_send_emulator_command(adb_client_info * client, int  argc, char**  argv);

/* return verbose error string from last operation */
const char *adb_error(adb_client_info * client);

/* read a standard adb status response (OKAY|FAIL) and
** return 0 in the event of OKAY, -1 in the event of FAIL
** or protocol error
*/
int adb_status(adb_client_info * client, int fd);

void report_to_cs(adb_client_info * client, char * str);
#endif
