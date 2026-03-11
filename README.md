# DMY APK Installer System

A comprehensive Android application management system for batch installing APK files to multiple Android devices simultaneously. The system consists of a Windows desktop client, Android agent app, web management portal, and backend services.

## System Architecture

```
┌─────────────────┐      ┌──────────────────┐      ┌─────────────────┐
│   PC Client     │◄────►│  Web Service     │◄────►│  Web Management │
│ (C# WinForms)   │      │   (WCF)          │      │   Site (MVC)    │
└────────┬────────┘      └────────┬─────────┘      └─────────────────┘
         │                       │
         │                       │
         ▼                       ▼
┌─────────────────┐      ┌──────────────────┐
│  Android Agent  │◄────►│   SQL Database   │
│   (APK)         │      │                  │
└─────────────────┘      └──────────────────┘
```

## Components

### 1. PC Client (`PC/ApkInstaller/`)
Windows desktop application for managing APK installations.

**Features:**
- One-click batch APK installation to multiple Android devices
- Real-time installation progress monitoring
- Device management (connect, disconnect, status monitoring)
- APK group management and downloading
- Device information display (storage space, device model, IMEI)
- Installed applications listing
- Auto-uninstall option before installation
- System tray support with minimize to tray
- Multi-language support (English, Chinese)

**Tech Stack:**
- C# / .NET Framework
- Windows Forms (C1.Win.C1Command)
- SQLite for local caching
- ADB (Android Debug Bridge) integration

### 2. Android Agent (`Android/APKAgent/`)
Android application that runs on target devices to enable remote installation.

**Features:**
- TCP server for PC communication (port 25000)
- Device information retrieval (IMEI, storage, model, brand, serial)
- Installed application listing with icons
- Screen capture capability
- Installation completion alarm/notifications

**Tech Stack:**
- Java / Android SDK
- TCP Socket Communication

### 3. Web Management Site (`ManageSite/`)
ASP.NET MVC website for administrators to manage APK packages.

**Features:**
- APK package upload and management
- User administration
- Installation statistics and logs

**Tech Stack:**
- ASP.NET MVC
- SQL Server Database

### 4. Backend Service (`Service/ApkInstallerService/`)
WCF service for data operations and business logic.

**Features:**
- APK data management
- User authentication and authorization
- License management
- Installation logging

**Tech Stack:**
- WCF (Windows Communication Foundation)
- SQL Server

### 5. Database (`Database/`)
SQL Server database for storing:
- User information
- APK package metadata
- Installation logs
- Device information

## Project Structure

```
dmy-apk-installer-system/
├── Android/
│   ├── APKAgent/           # Android Agent App
│   ├── TCPClient/          # Android TCP Client (testing)
│   └── TCPServer/          # Android TCP Server (testing)
├── Database/               # SQL Server database files
├── Installer/              # Installer project and files
├── ManageSite/             # ASP.NET MVC Web Application
│   └── MvcSiteMapProvider/ # SiteMap provider
├── PC/
│   ├── ApkInstaller/       # Main PC Client Application
│   │   ├── ADBWrapper/     # ADB wrapper library
│   │   ├── ApkPLUpdater/   # APK update module
│   │   ├── DriverInstaller/# USB Driver installer
│   │   ├── DriverTools/    # Driver utilities
│   │   └── MiscUtil/       # Utility classes
│   ├── android_usb_test/   # USB API testing
│   └── Patch/              # Patching files
└── Service/
    └── ApkInstallerService/ # WCF Backend Service
```

## Requirements

### PC Client
- Windows 7 or later
- .NET Framework 4.0+
- Android Debug Bridge (ADB) drivers
- SQLite

### Android Agent
- Android 2.3 (Gingerbread) or later
- ROOT access required for full functionality

### For Development
- Visual Studio 2010+
- Android SDK
- SQL Server 2008+

## Installation & Setup

### 1. Install USB Drivers
Install the appropriate USB drivers for your Android devices on the PC.

### 2. Deploy Android Agent
Install `APKAgent.apk` on all target Android devices:
```bash
adb install APKAgent.apk
```

### 3. Configure PC Client
Edit `apkinstaller.ini` to configure:
- Server URL
- Authentication token
- Local storage paths

### 4. Start Services
1. Start the WCF Service
2. Start the Web Management Site (IIS)
3. Run the PC Client application

## Usage

### PC Client Operations
1. **Connect Devices**: Connect Android devices via USB, the client will automatically detect them
2. **Select APK Group**: Choose an APK group from the server
3. **Download APKs**: APKs will be automatically downloaded
4. **One-Click Install**: Click "One-Click Install" to install all APKs to selected devices
5. **Monitor Progress**: View real-time installation status for each device

### Device Panel Controls
- **Play**: Start installation
- **Pause**: Pause current installation
- **Stop**: Stop current installation

## Communication Protocol

### PC ↔ Android Agent (TCP Port 25000)

**Packet Structure:**
```
┌─────────────────────┬─────────────────────┬─────────────────────┐
│ Packet Length (4B) │ Packet Type (4B)   │ Packet Data (Var)   │
└─────────────────────┴─────────────────────┴─────────────────────┘
```

**Packet Types:**
| Code   | Description                      |
|--------|----------------------------------|
| CONN   | Connection request               |
| DCON   | Disconnect                       |
| SDFS   | SD Card free space               |
| SDTS   | SD Card total space              |
| IMFS   | Internal memory free space       |
| IMTS   | Internal memory total space      |
| IDPL   | Installed program list           |
| CASC   | Screen capture                   |
| INFO   | Device unique information        |
| AIED   | APK install completed            |

## API Endpoints (Web Service)

### APK Operations
- `GetApkGroupList` - Get list of APK groups
- `GetApkList` - Get APK files in a group
- `InsertApkInstallLog` - Log installation result
- `InsertApkUpdateLog` - Log APK update

### License Operations
- `ApkLicenseCheck` - Validate license

## Configuration Files

- `apkinstaller.ini` - Main configuration file
- `ConnStrings.config` - Database connection strings
- `Web.config` - Web application configuration

## License

This project is provided as-is for educational and development purposes.

## Troubleshooting

### Device Not Detected
1. Check USB debugging is enabled on Android device
2. Install proper USB drivers
3. Restart ADB server: `adb kill-server && adb start-server`

### Installation Fails
1. Check device has sufficient storage space
2. Verify APK files are valid
3. Ensure device has required permissions

### Connection Issues
1. Check firewall settings
2. Verify network connectivity
3. Ensure correct server URL configuration

