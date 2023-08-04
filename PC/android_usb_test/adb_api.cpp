/*
 * Copyright (C) 2006 The Android Open Source Project
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

/** \file
  This file consists of implementation of rotines that are exported
  from this DLL.
*/

#include "stdafx.h"
#include "adb_api.h"
//#include "adb_object_handle.h"
//#include "adb_interface_enum.h"
//#include "adb_interface.h"
//#include "adb_endpoint_object.h"
//#include "adb_io_completion.h"
//#include "adb_helper_routines.h"

HMODULE hAdvWinApi = NULL;

/************************************************************************/
/*                                                                      */
/************************************************************************/

typedef ADBAPIHANDLE (*AdbEnumInterfaces_FuncT)(GUID, bool, bool, bool);
AdbEnumInterfaces_FuncT AdbEnumInterfaces_Func = NULL;

ADBAPIHANDLE AdbEnumInterfaces(GUID class_id,
                               bool exclude_not_present,
                               bool exclude_removed,
                               bool active_only) {
 // AdbInterfaceEnumObject* enum_obj = NULL;
  
  if ( !hAdvWinApi )	return NULL;
  if ( !AdbEnumInterfaces_Func )
	AdbEnumInterfaces_Func = (AdbEnumInterfaces_FuncT)GetProcAddress(hAdvWinApi, "AdbEnumInterfaces");

  return AdbEnumInterfaces_Func(class_id, exclude_not_present, exclude_removed, active_only);
}

/************************************************************************/
/*                                                                      */
/************************************************************************/
typedef bool (*AdbNextInterface_FuncT)(ADBAPIHANDLE, AdbInterfaceInfo*, unsigned long*);
AdbNextInterface_FuncT AdbNextInterface_Func = NULL;

bool AdbNextInterface(ADBAPIHANDLE adb_handle,
                      AdbInterfaceInfo* info,
                      unsigned long* size) {

	if ( !hAdvWinApi )	return NULL;
	if ( !AdbNextInterface_Func )
		AdbNextInterface_Func = (AdbNextInterface_FuncT)GetProcAddress(hAdvWinApi, "AdbNextInterface");
	
  return AdbNextInterface_Func(adb_handle, info, size);
}

/************************************************************************/
/*                                                                      */
/************************************************************************/

typedef bool (*AdbResetInterfaceEnum_FuncT)(ADBAPIHANDLE adb_handle);
AdbResetInterfaceEnum_FuncT AdbResetInterfaceEnum_Func = NULL;

bool AdbResetInterfaceEnum(ADBAPIHANDLE adb_handle) {

	if ( !hAdvWinApi )	return NULL;
	if ( !AdbResetInterfaceEnum_Func )
		AdbResetInterfaceEnum_Func = (AdbResetInterfaceEnum_FuncT)GetProcAddress(hAdvWinApi, "AdbResetInterfaceEnum");

	return AdbResetInterfaceEnum_Func(adb_handle);
}

/************************************************************************/
/*                                                                      */
/************************************************************************/

typedef ADBAPIHANDLE (*AdbCreateInterfaceByName_FuncT)(const wchar_t* interface_name);
AdbCreateInterfaceByName_FuncT AdbCreateInterfaceByName_Func = NULL;

ADBAPIHANDLE AdbCreateInterfaceByName(
    const wchar_t* interface_name) {

		if ( !hAdvWinApi )	return NULL;
		if ( !AdbCreateInterfaceByName_Func )
			AdbCreateInterfaceByName_Func = (AdbCreateInterfaceByName_FuncT)GetProcAddress(hAdvWinApi, "AdbCreateInterfaceByName");

  return AdbCreateInterfaceByName_Func(interface_name);
}

/************************************************************************/
/*                                                                      */
/************************************************************************/

typedef ADBAPIHANDLE (*AdbCreateInterface_FuncT)(GUID class_id,
	unsigned short vendor_id,
	unsigned short product_id,
	unsigned char interface_id);
AdbCreateInterface_FuncT AdbCreateInterface_Func = NULL;

ADBAPIHANDLE AdbCreateInterface(GUID class_id,
	unsigned short vendor_id,
	unsigned short product_id,
	unsigned char interface_id)
{
	if ( !hAdvWinApi )	return NULL;
	if ( !AdbCreateInterface_Func )
		AdbCreateInterface_Func = (AdbCreateInterface_FuncT)GetProcAddress(hAdvWinApi, "AdbCreateInterface");

	return AdbCreateInterface_Func(class_id, vendor_id, product_id, interface_id);
}

/************************************************************************/
/*                                                                      */
/************************************************************************/

typedef bool (*AdbGetInterfaceName_FuncT)(ADBAPIHANDLE adb_interface,
	void* buffer,
	unsigned long* buffer_char_size,
	bool ansi);

AdbGetInterfaceName_FuncT AdbGetInterfaceName_Func = NULL;

bool AdbGetInterfaceName(ADBAPIHANDLE adb_interface,
                         void* buffer,
                         unsigned long* buffer_char_size,
                         bool ansi) 
{
	if ( !hAdvWinApi )	return NULL;
	if ( !AdbGetInterfaceName_Func )
		AdbGetInterfaceName_Func = (AdbGetInterfaceName_FuncT)GetProcAddress(hAdvWinApi, "AdbGetInterfaceName");

	return AdbGetInterfaceName_Func(adb_interface, buffer, buffer_char_size, ansi);
}

/************************************************************************/
/*                                                                      */
/************************************************************************/

typedef bool (*AdbGetSerialNumber_FuncT)(ADBAPIHANDLE adb_interface,
	void* buffer,
	unsigned long* buffer_char_size,
	bool ansi);

AdbGetSerialNumber_FuncT AdbGetSerialNumber_Func = NULL;

bool AdbGetSerialNumber(ADBAPIHANDLE adb_interface,
                        void* buffer,
                        unsigned long* buffer_char_size,
                        bool ansi) 
{
	if ( !hAdvWinApi )	return NULL;
	if ( !AdbGetSerialNumber_Func )
		AdbGetSerialNumber_Func = (AdbGetSerialNumber_FuncT)GetProcAddress(hAdvWinApi, "AdbGetSerialNumber");

    return AdbGetSerialNumber_Func(adb_interface, buffer, buffer_char_size, ansi);
}

/************************************************************************/
/*                                                                      */
/************************************************************************/

typedef bool (*AdbGetUsbDeviceDescriptor_FuncT)(ADBAPIHANDLE adb_interface,
	USB_DEVICE_DESCRIPTOR* desc);

AdbGetUsbDeviceDescriptor_FuncT AdbGetUsbDeviceDescriptor_Func = NULL;

bool AdbGetUsbDeviceDescriptor(ADBAPIHANDLE adb_interface,
                               USB_DEVICE_DESCRIPTOR* desc) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbGetUsbDeviceDescriptor_Func )
		AdbGetUsbDeviceDescriptor_Func = (AdbGetUsbDeviceDescriptor_FuncT)GetProcAddress(hAdvWinApi, "AdbGetUsbDeviceDescriptor");

	return AdbGetUsbDeviceDescriptor_Func(adb_interface, desc);
}

/************************************************************************/
/*                                                                      */
/************************************************************************/

typedef bool (*AdbGetUsbConfigurationDescriptor_FuncT)(ADBAPIHANDLE adb_interface,
	USB_CONFIGURATION_DESCRIPTOR* desc);

AdbGetUsbConfigurationDescriptor_FuncT AdbGetUsbConfigurationDescriptor_Func = NULL;

bool AdbGetUsbConfigurationDescriptor(ADBAPIHANDLE adb_interface,
                                      USB_CONFIGURATION_DESCRIPTOR* desc)
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbGetUsbConfigurationDescriptor_Func )
		AdbGetUsbConfigurationDescriptor_Func = (AdbGetUsbConfigurationDescriptor_FuncT)GetProcAddress(hAdvWinApi, "AdbGetUsbConfigurationDescriptor");

    return AdbGetUsbConfigurationDescriptor_Func(adb_interface, desc);
  //}
}

/************************************************************************/
/*                                                                      */
/************************************************************************/
typedef bool (*AdbGetUsbInterfaceDescriptor_FuncT)(ADBAPIHANDLE adb_interface,
	USB_INTERFACE_DESCRIPTOR* desc);
AdbGetUsbInterfaceDescriptor_FuncT AdbGetUsbInterfaceDescriptor_Func = NULL;

bool AdbGetUsbInterfaceDescriptor(ADBAPIHANDLE adb_interface,
                                  USB_INTERFACE_DESCRIPTOR* desc)
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbGetUsbInterfaceDescriptor_Func )
		AdbGetUsbInterfaceDescriptor_Func = (AdbGetUsbInterfaceDescriptor_FuncT)GetProcAddress(hAdvWinApi, "AdbGetUsbInterfaceDescriptor");

	return AdbGetUsbInterfaceDescriptor_Func(adb_interface, desc);
}

/************************************************************************/
/*                                                                      */
/************************************************************************/
typedef bool (*AdbGetEndpointInformation_FuncT)(ADBAPIHANDLE adb_interface,
											UCHAR endpoint_index,
											AdbEndpointInformation* info);
AdbGetEndpointInformation_FuncT AdbGetEndpointInformation_Func = NULL;

bool AdbGetEndpointInformation(ADBAPIHANDLE adb_interface,
                               UCHAR endpoint_index,
                               AdbEndpointInformation* info) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbGetEndpointInformation_Func )
		AdbGetEndpointInformation_Func = (AdbGetEndpointInformation_FuncT)GetProcAddress(hAdvWinApi, "AdbGetEndpointInformation");

	return AdbGetEndpointInformation_Func(adb_interface, endpoint_index, info);
}

/************************************************************************/
/*                                                                      */
/************************************************************************/
typedef bool (*AdbGetDefaultBulkReadEndpointInformation_FuncT)(ADBAPIHANDLE adb_interface,
	AdbEndpointInformation* info);
AdbGetDefaultBulkReadEndpointInformation_FuncT AdbGetDefaultBulkReadEndpointInformation_Func = NULL;

bool AdbGetDefaultBulkReadEndpointInformation(ADBAPIHANDLE adb_interface,
                                              AdbEndpointInformation* info) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbGetDefaultBulkReadEndpointInformation_Func )
		AdbGetDefaultBulkReadEndpointInformation_Func = (AdbGetDefaultBulkReadEndpointInformation_FuncT)GetProcAddress(hAdvWinApi, "AdbGetDefaultBulkReadEndpointInformation");

	return AdbGetDefaultBulkReadEndpointInformation_Func(adb_interface, info); 
}



typedef bool (*AdbGetDefaultBulkWriteEndpointInformation_FuncT)(ADBAPIHANDLE adb_interface,
	AdbEndpointInformation* info);
AdbGetDefaultBulkWriteEndpointInformation_FuncT AdbGetDefaultBulkWriteEndpointInformation_Func = NULL;

bool AdbGetDefaultBulkWriteEndpointInformation(ADBAPIHANDLE adb_interface,
                                               AdbEndpointInformation* info) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbGetDefaultBulkWriteEndpointInformation_Func )
		AdbGetDefaultBulkWriteEndpointInformation_Func = (AdbGetDefaultBulkWriteEndpointInformation_FuncT)GetProcAddress(hAdvWinApi, "AdbGetDefaultBulkReadEndpointInformation");

	return AdbGetDefaultBulkWriteEndpointInformation_Func(adb_interface, info); 
}

typedef ADBAPIHANDLE (*AdbOpenEndpoint_FuncT)(ADBAPIHANDLE adb_interface,
	unsigned char endpoint_index,
	AdbOpenAccessType access_type,
	AdbOpenSharingMode sharing_mode) ;
AdbOpenEndpoint_FuncT AdbOpenEndpoint_Func = NULL;

ADBAPIHANDLE AdbOpenEndpoint(ADBAPIHANDLE adb_interface,
                             unsigned char endpoint_index,
                             AdbOpenAccessType access_type,
                             AdbOpenSharingMode sharing_mode) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbOpenEndpoint_Func )
		AdbOpenEndpoint_Func = (AdbOpenEndpoint_FuncT)GetProcAddress(hAdvWinApi, "AdbOpenEndpoint");

	return AdbOpenEndpoint_Func(adb_interface, endpoint_index, access_type, sharing_mode); 
}

typedef ADBAPIHANDLE (*AdbOpenDefaultBulkReadEndpoint_FuncT)(ADBAPIHANDLE adb_interface,
	AdbOpenAccessType access_type,
	AdbOpenSharingMode sharing_mode);
AdbOpenDefaultBulkReadEndpoint_FuncT AdbOpenDefaultBulkReadEndpoint_Func = NULL;

ADBAPIHANDLE AdbOpenDefaultBulkReadEndpoint(ADBAPIHANDLE adb_interface,
                                            AdbOpenAccessType access_type,
                                            AdbOpenSharingMode sharing_mode) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbOpenDefaultBulkReadEndpoint_Func )
		AdbOpenDefaultBulkReadEndpoint_Func = (AdbOpenDefaultBulkReadEndpoint_FuncT)GetProcAddress(hAdvWinApi, "AdbOpenDefaultBulkReadEndpoint");

	return AdbOpenDefaultBulkReadEndpoint_Func(adb_interface, access_type, sharing_mode);
}


typedef ADBAPIHANDLE (*AdbOpenDefaultBulkWriteEndpoint_FuncT)(ADBAPIHANDLE adb_interface,
	AdbOpenAccessType access_type,
	AdbOpenSharingMode sharing_mode) ;
AdbOpenDefaultBulkWriteEndpoint_FuncT AdbOpenDefaultBulkWriteEndpoint_Func = NULL;

ADBAPIHANDLE AdbOpenDefaultBulkWriteEndpoint(ADBAPIHANDLE adb_interface,
                                             AdbOpenAccessType access_type,
                                             AdbOpenSharingMode sharing_mode) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbOpenDefaultBulkWriteEndpoint_Func )
		AdbOpenDefaultBulkWriteEndpoint_Func = (AdbOpenDefaultBulkWriteEndpoint_FuncT)GetProcAddress(hAdvWinApi, "AdbOpenDefaultBulkWriteEndpoint");

	return AdbOpenDefaultBulkWriteEndpoint_Func(adb_interface, access_type, sharing_mode);
}

typedef ADBAPIHANDLE (*AdbGetEndpointInterface_FuncT)(ADBAPIHANDLE adb_endpoint) ;
AdbGetEndpointInterface_FuncT AdbGetEndpointInterface_Func = NULL;

ADBAPIHANDLE AdbGetEndpointInterface(ADBAPIHANDLE adb_endpoint) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbGetEndpointInterface_Func )
		AdbGetEndpointInterface_Func = (AdbGetEndpointInterface_FuncT)GetProcAddress(hAdvWinApi, "AdbOpenDefaultBulkWriteEndpoint");

	return AdbGetEndpointInterface_Func(adb_endpoint);
}


typedef bool (*AdbQueryInformationEndpoint_FuncT)(ADBAPIHANDLE adb_endpoint,
	AdbEndpointInformation* info);
AdbQueryInformationEndpoint_FuncT AdbQueryInformationEndpoint_Func = NULL;

bool AdbQueryInformationEndpoint(ADBAPIHANDLE adb_endpoint,
                                 AdbEndpointInformation* info) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbQueryInformationEndpoint_Func )
		AdbQueryInformationEndpoint_Func = (AdbQueryInformationEndpoint_FuncT)GetProcAddress(hAdvWinApi, "AdbQueryInformationEndpoint");

	return AdbQueryInformationEndpoint_Func(adb_endpoint, info);
}


typedef ADBAPIHANDLE (*AdbReadEndpointAsync_FuncT)(ADBAPIHANDLE adb_endpoint,
	void* buffer,
	unsigned long bytes_to_read,
	unsigned long* bytes_read,
	unsigned long time_out,
	HANDLE event_handle) ;
AdbReadEndpointAsync_FuncT AdbReadEndpointAsync_Func = NULL;

ADBAPIHANDLE AdbReadEndpointAsync(ADBAPIHANDLE adb_endpoint,
                                  void* buffer,
                                  unsigned long bytes_to_read,
                                  unsigned long* bytes_read,
                                  unsigned long time_out,
                                  HANDLE event_handle) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbReadEndpointAsync_Func )
		AdbReadEndpointAsync_Func = (AdbReadEndpointAsync_FuncT)GetProcAddress(hAdvWinApi, "AdbReadEndpointAsync");

	return AdbReadEndpointAsync_Func(adb_endpoint, buffer, bytes_to_read, bytes_read, time_out, event_handle);
}


typedef ADBAPIHANDLE (*AdbWriteEndpointAsync_FuncT)(ADBAPIHANDLE adb_endpoint,
	void* buffer,
	unsigned long bytes_to_write,
	unsigned long* bytes_written,
	unsigned long time_out,
	HANDLE event_handle);
AdbWriteEndpointAsync_FuncT AdbWriteEndpointAsync_Func = NULL;

ADBAPIHANDLE AdbWriteEndpointAsync(ADBAPIHANDLE adb_endpoint,
                                   void* buffer,
                                   unsigned long bytes_to_write,
                                   unsigned long* bytes_written,
								   unsigned long time_out,
								   HANDLE event_handle) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbWriteEndpointAsync_Func )
		AdbWriteEndpointAsync_Func = (AdbWriteEndpointAsync_FuncT)GetProcAddress(hAdvWinApi, "AdbWriteEndpointAsync");

	return AdbWriteEndpointAsync_Func(adb_endpoint, buffer, bytes_to_write, bytes_written, time_out, event_handle);
}


typedef bool (*AdbReadEndpointSync_FuncT)(ADBAPIHANDLE adb_endpoint,
	void* buffer,
	unsigned long bytes_to_read,
	unsigned long* bytes_read,
	unsigned long time_out) ;
AdbReadEndpointSync_FuncT AdbReadEndpointSync_Func = NULL;

bool AdbReadEndpointSync(ADBAPIHANDLE adb_endpoint,
                         void* buffer,
                         unsigned long bytes_to_read,
                         unsigned long* bytes_read,
                         unsigned long time_out) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbReadEndpointSync_Func )
		AdbReadEndpointSync_Func = (AdbReadEndpointSync_FuncT)GetProcAddress(hAdvWinApi, "AdbReadEndpointSync");

	return AdbReadEndpointSync_Func(adb_endpoint, buffer, bytes_to_read, bytes_read, time_out);
}


typedef bool (*AdbWriteEndpointSync_FuncT)(ADBAPIHANDLE adb_endpoint,
	void* buffer,
	unsigned long bytes_to_write,
	unsigned long* bytes_written,
	unsigned long time_out);
AdbWriteEndpointSync_FuncT AdbWriteEndpointSync_Func = NULL;

bool AdbWriteEndpointSync(ADBAPIHANDLE adb_endpoint,
                          void* buffer,
                          unsigned long bytes_to_write,
                          unsigned long* bytes_written,
                          unsigned long time_out)
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbWriteEndpointSync_Func )
		AdbWriteEndpointSync_Func = (AdbWriteEndpointSync_FuncT)GetProcAddress(hAdvWinApi, "AdbWriteEndpointSync");

	return AdbWriteEndpointSync_Func(adb_endpoint, buffer, bytes_to_write, bytes_written, time_out);
}


typedef bool (*AdbGetOvelappedIoResult_FuncT)(ADBAPIHANDLE adb_io_completion,
	LPOVERLAPPED overlapped,
	unsigned long* bytes_transferred,
	bool wait);
AdbGetOvelappedIoResult_FuncT AdbGetOvelappedIoResult_Func = NULL;

bool AdbGetOvelappedIoResult(ADBAPIHANDLE adb_io_completion,
                             LPOVERLAPPED overlapped,
                             unsigned long* bytes_transferred,
                             bool wait) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbGetOvelappedIoResult_Func )
		AdbGetOvelappedIoResult_Func = (AdbGetOvelappedIoResult_FuncT)GetProcAddress(hAdvWinApi, "AdbWriteEndpointSync");

	return AdbGetOvelappedIoResult_Func(adb_io_completion, overlapped, bytes_transferred, wait);
}


typedef bool (*AdbHasOvelappedIoComplated_FuncT)(ADBAPIHANDLE adb_io_completion);
AdbHasOvelappedIoComplated_FuncT AdbHasOvelappedIoComplated_Func = NULL;

bool AdbHasOvelappedIoComplated(ADBAPIHANDLE adb_io_completion)
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbHasOvelappedIoComplated_Func )
		AdbHasOvelappedIoComplated_Func = (AdbHasOvelappedIoComplated_FuncT)GetProcAddress(hAdvWinApi, "AdbHasOvelappedIoComplated");

	return AdbHasOvelappedIoComplated_Func(adb_io_completion);
}


typedef bool (*AdbCloseHandle_FuncT)(ADBAPIHANDLE adb_handle);
AdbCloseHandle_FuncT AdbCloseHandle_Func = NULL;

bool AdbCloseHandle(ADBAPIHANDLE adb_handle) 
{
	if ( !hAdvWinApi )	return false;
	if ( !AdbCloseHandle_Func )
		AdbCloseHandle_Func = (AdbCloseHandle_FuncT)GetProcAddress(hAdvWinApi, "AdbCloseHandle");

	return AdbCloseHandle_Func(adb_handle);
}
