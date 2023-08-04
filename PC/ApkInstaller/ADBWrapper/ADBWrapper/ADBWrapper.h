// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the ADBWRAPPER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// ADBWRAPPER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef ADBWRAPPER_EXPORTS
#define ADBWRAPPER_API __declspec(dllexport)
#else
#define ADBWRAPPER_API __declspec(dllimport)
#endif

extern "C"
{
	ADBWRAPPER_API char* adb_list_devices();
	ADBWRAPPER_API char* adb_screen_image(char * hardwareid);
	ADBWRAPPER_API char * adb_get_device_vendor( char * hardwareid);
	ADBWRAPPER_API char * adb_get_device_model( char * hardwareid);
	ADBWRAPPER_API void adb_reboot_device( char * hardwareid);
	ADBWRAPPER_API char* adb_get_freespace(char * hardwareid);
	ADBWRAPPER_API void adb_test_server();
	//ADBWRAPPER_API int adb_install_apk(char * hardwareid, char * localapk_path);
	ADBWRAPPER_API int adb_install_apk(char * hardwareid, char * apkname, char* vcode, char* vname, char* package, int location, char * localapk_path);	//Modified by CSC
	ADBWRAPPER_API int adb_uninstall_apk(char * hardwareid, char * apkname, char * package_name, bool bReportStatus);
	ADBWRAPPER_API int adb_start_agent(char * hardwareid, char * apkagent_file, int apkagent_versioncode, char * agent_package);
	ADBWRAPPER_API int adb_forward_tcp(char * hardwareid, int localport, int remote_port);
	ADBWRAPPER_API int adb_get_install_location(char * hardwareid);
	ADBWRAPPER_API int adb_set_install_location(char * hardwareid, int install_location);
};

