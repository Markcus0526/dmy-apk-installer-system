/*
 *											 Packet Structure
 *
 *			-------------------------------------------------------------------------------------------------
 *		   |	Packet Length (4 Bytes)	|	Packet Type (4 Bytes)	|			Packet Data(Variable)  		 |			
 *			-------------------------------------------------------------------------------------------------
 *
 *		  
 */

package com.damy.apkagent;

import java.io.BufferedOutputStream;
import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

import android.content.Intent;
import android.media.AudioManager;
import android.media.MediaPlayer;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.Message;
import android.os.StatFs;
import android.provider.Settings.Secure;
import android.telephony.TelephonyManager;
import android.util.Log;
import android.view.KeyEvent;
import android.view.View;
import android.view.View.MeasureSpec;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import android.widget.RelativeLayout.LayoutParams;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.graphics.Bitmap;
import android.graphics.Bitmap.CompressFormat;
import android.graphics.Canvas;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;


public class MainActivity extends Activity {

	private static final String PKT_CONNECT = "CONN";
	private static final String PKT_DISCONNECT = "DCON";
	private static final String PKT_SDCARD_FREESPACE = "SDFS";
	private static final String PKT_SDCARD_TOTALSPACE = "SDTS";
	private static final String PKT_INTERNALMEMORY_FREESPACE = "IMFS";
	private static final String PKT_INTERNALMEMORY_TOTALSPACE = "IMTS";
	private static final String PKT_INSTALLED_PROGRAM_LISTS = "IDPL";
	private static final String PKT_SCREEN_CAPTURE = "CASC";
	private static final String PKT_DEVICE_UNIQUE = "INFO";
    private static final String PKT_APK_INSTALL_COMPLETED = "AIED";

	private static final int SERVER_PORT = 25000;
	private static final int SEND_REQUEST = 0;
	private static final int SEND_CONNECT = 1;
	private static final int SEND_DISCONNECT = 2;
	private static final int REQUEST_PERIOD_MS = 1000;

	private ServerSocket serverSocket = null;
	private TCPServerThread serverThread = null;
	private SendMessageHandler mMainHandler = null;

	private ImageView imgConnectState = null;
    private static MediaPlayer m_CompletedAlarmSoundPlayer = null;
    private AudioManager audioManager;

    private static boolean m_bStartAlarm = false;
    private static Timer timer = new Timer();
    private static final int TIMER_CYCLE = 200; 	// ms

	 public byte[] intToBytes2(int n){
		  byte[] b = new byte[4];
		  for(int i = 0;i < 4;i++){
		   b[i] = (byte)(n >> (i * 8)); 
		  }
		  return b;
	 }
	 
	 
	 public byte[] longToBytes2(long n){
		  byte[] b = new byte[8];
		  for(int i = 0;i < 8;i++){
		   b[i] = (byte)(n >> (i * 8)); 
		  }
		  return b;
	 }
	 
	 public int byteToInt2(byte[] b){
		  return (((int)b[0]) << 24) + (((int)b[1]) << 16) + (((int)b[2]) << 8) + b[3];
	}

	 public  byte[] stringToBytesASCII(String str) {
		 
		 char[] buffer = str.toCharArray();
		 byte[] b = new byte[buffer.length];
		 for (int i = 0; i < b.length; i++) {
			 b[i] = (byte) buffer[i];
		 }
		 return b;
		 
	}
	 
	@SuppressWarnings("static-access")
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);		
				
		imgConnectState = (ImageView) findViewById(R.id.imgConnectState);

		try {
			serverSocket = new ServerSocket(SERVER_PORT);
			mMainHandler = new SendMessageHandler();
			serverThread = new TCPServerThread();
		} catch (IOException e) {
			e.printStackTrace();
		}

        m_CompletedAlarmSoundPlayer = new MediaPlayer();
        m_CompletedAlarmSoundPlayer = MediaPlayer.create(MainActivity.this, R.raw.install_completed_alarm);
        audioManager = (AudioManager) getSystemService(this.AUDIO_SERVICE);

        for(int i = 0; i < 20; i++)
            audioManager.adjustStreamVolume(AudioManager.STREAM_MUSIC,
                    AudioManager.ADJUST_LOWER, 0);

        for(int i = 0; i < 10; i++)
            audioManager.adjustStreamVolume(AudioManager.STREAM_MUSIC,
                AudioManager.ADJUST_RAISE, 0);

		serverThread.start();

        timer.schedule( new TimerTask() {
            @Override
            public void run() {
                if(m_bStartAlarm)
                {
                    try {
                        m_bStartAlarm = false;

                        if (m_CompletedAlarmSoundPlayer != null) {
                            m_CompletedAlarmSoundPlayer.stop();
                        }
                        m_CompletedAlarmSoundPlayer.prepare();
                        m_CompletedAlarmSoundPlayer.start();

                    } catch (IllegalStateException e) {
                        e.printStackTrace();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }

                    startCompletedAlarmActivity();

                }
            }
        }, 10, TIMER_CYCLE);
	}

	class TCPServerThread extends Thread implements Runnable {
		private boolean isPlay = false;

		public TCPServerThread() {
			isPlay = true;
		}

		public void isThreadState(boolean isPlay) {
			this.isPlay = isPlay;
		}

		public void stopThread() {
			isPlay = false;
		}

		@Override
		public void run() {
			super.run();
			System.out.println("Server Thread start");
			while (isPlay) {
				try {
					System.out.println("Waiting a connection");

					Socket socket = serverSocket.accept();
					System.out.println(socket.getInetAddress()
							+ " received connection request");

					Message msg = mMainHandler.obtainMessage();
					msg.what = SEND_REQUEST;
					msg.arg1 = 0;
					msg.arg2 = 0;
					msg.obj = socket;
					mMainHandler.sendMessage(msg);

					Thread.sleep(REQUEST_PERIOD_MS);
				} catch (InterruptedException e) {
					e.printStackTrace();
				} catch (IOException e) {
					e.printStackTrace();
				}
			}

			System.out.println("Server Thread is killed");
		}
	}

	@SuppressLint("HandlerLeak")
	class SendMessageHandler extends Handler {
		@Override
		public void handleMessage(Message msg) {
			super.handleMessage(msg);

			switch (msg.what) {
			case SEND_REQUEST:
				TCPClientThread clientThread = new TCPClientThread(
						(Socket) msg.obj);
				clientThread.start();

			case SEND_CONNECT:
				imgConnectState.setImageResource(R.drawable.connect);
				break;

			case SEND_DISCONNECT:
				imgConnectState.setImageResource(R.drawable.disconnect);
				break;

			default:
				break;
			}
		}
	};

	class TCPClientThread extends Thread implements Runnable {
		private Socket socket;
		private boolean isRunnable = false;

		public TCPClientThread(Socket socket) {
			this.socket = socket;
			this.isRunnable = true;
		}

		public void setThreadState(boolean isRunnable) {
			this.isRunnable = isRunnable;
		}

		public void stopThread() {
			this.isRunnable = false;
		}
		
		

		 
		@Override
		public void run() {
			super.run();

			System.out.println("Client Thread start");

			while (isRunnable) {
				if (socket != null) {
					Packet packet = null;
					packet = parsePacket(this, socket);
					if (packet != null) {
						try {
							BufferedOutputStream bw = new BufferedOutputStream(socket.getOutputStream());
							bw.write(intToBytes2(packet.Length));
							bw.write(stringToBytesASCII(packet.PacketType), 0, 4);
							if ( packet.PacketData != null )
								bw.write(packet.PacketData);
							bw.flush();
						} catch (IOException e) {
							e.printStackTrace();
						}

						if (packet.PacketType.equals(PKT_DISCONNECT)) {
							setThreadState(false);
						}
					}
				} else
					break;
			}

			if (socket != null)
				try {
					socket.close();
				} catch (IOException e) {
					e.printStackTrace();
				}

			System.out.println("Client Thread is killed");
		}
	}

	// /////////////////////////////////////////////////////////
	// Start parse packet with PC
	public class Packet {
		public int Length;
		public String PacketType;
		public byte[] PacketData;

		public Packet() {
			Length = 0;
			PacketType = "";
			PacketData = null;
		}
	}
	
	public Packet mPacket; 

	public Bitmap getBitmapFromView(View v) {
	    v.setLayoutParams(new LayoutParams(
	            RelativeLayout.LayoutParams.FILL_PARENT,
	            RelativeLayout.LayoutParams.FILL_PARENT));
	    v.measure(MeasureSpec.makeMeasureSpec(0, MeasureSpec.UNSPECIFIED),
	            MeasureSpec.makeMeasureSpec(0, MeasureSpec.UNSPECIFIED));
	    v.layout(0, 0, v.getMeasuredWidth(), v.getMeasuredHeight());
	    Bitmap b = Bitmap.createBitmap(v.getMeasuredWidth(),
	            v.getMeasuredHeight(), Bitmap.Config.RGB_565);

	    Canvas c = new Canvas(b);
	    v.draw(c);
	    return b;       
	}
	
	private Packet parsePacket(TCPClientThread thread, Socket socket) {
		Packet packet = new Packet();

		char []strData = new char[4];
		try {
			BufferedReader br = new BufferedReader(new InputStreamReader(
					socket.getInputStream()));
			while (br.read(strData, 0, 4) > 0) {
				
				String str= new String(strData);
				
				fileSave(str);
				
				if (str.equals(PKT_CONNECT)) {
					mMainHandler.sendEmptyMessage(SEND_CONNECT);
					packet.PacketType = PKT_CONNECT;
					packet.PacketData = null;
					packet.Length = 8;

					System.out.println("CONNECT");

					return packet;
				} else if (str.equals(PKT_DISCONNECT)) {
					mMainHandler.sendEmptyMessage(SEND_DISCONNECT);
					packet.PacketType = PKT_DISCONNECT;
					packet.Length = 8;

					System.out.println("DISCONNECT");

					return packet;
				} else if (str.equals(PKT_SDCARD_FREESPACE)) {
					packet.PacketType = PKT_SDCARD_FREESPACE;
					StatFs stat = new StatFs(Environment
							.getExternalStorageDirectory().getPath());
					long bytesAvailable = (long) stat.getAvailableBlocks()
							* (long) stat.getBlockSize();
					packet.PacketData = stringToBytesASCII(Long.toString(bytesAvailable));
					packet.Length = packet.PacketData.length + 8;
					System.out.println("FREE SPACE");
					return packet;
				} else if (str.equals(PKT_SDCARD_TOTALSPACE)) {
					packet.PacketType = PKT_SDCARD_TOTALSPACE;
					StatFs stat = new StatFs(Environment
							.getExternalStorageDirectory().getPath());
					long bytesTotal = (long) stat.getBlockCount()
							* (long) stat.getBlockSize();
					packet.PacketData = stringToBytesASCII(Long.toString(bytesTotal));
					packet.Length = packet.PacketData.length + 8;

					return packet;
				} else if (str.equals(PKT_INTERNALMEMORY_FREESPACE)) {
					packet.PacketType = PKT_INTERNALMEMORY_FREESPACE;
					StatFs stat = new StatFs(Environment.getDataDirectory()
							.getPath());
					long bytesAvailable = (long) stat.getAvailableBlocks()
							* (long) stat.getBlockSize();
					packet.PacketData = stringToBytesASCII(Long.toString(bytesAvailable));
					packet.Length = packet.PacketData.length + 8;

					return packet;
				} else if (str.equals(PKT_INTERNALMEMORY_TOTALSPACE)) {
					packet.PacketType = PKT_INTERNALMEMORY_TOTALSPACE;
					StatFs stat = new StatFs(Environment.getDataDirectory()
							.getPath());
					Long bytesTotal = (long) stat.getBlockCount()
							* (long) stat.getBlockSize();
					packet.PacketData = stringToBytesASCII(Long.toString(bytesTotal));
					packet.Length = packet.PacketData.length + 8;

					return packet;
				} else if (str.equals(PKT_INSTALLED_PROGRAM_LISTS)) {
					fileSave("APKInstaller Command Start");
					packet.PacketType = PKT_INSTALLED_PROGRAM_LISTS;
					ArrayList<PInfo> apps = getInstalledApps();
					fileSave("APKInstaller get Installed List");
					ByteArrayOutputStream outputStream = new ByteArrayOutputStream( );
					
					outputStream.write( intToBytes2(apps.size()) );
					for (int i = 0; i < apps.size(); i++) {
						
						fileSave("APKInstaller AppName");
						outputStream.write( intToBytes2(apps.get(i).appName.getBytes().length) );
						outputStream.write( apps.get(i).appName.getBytes() );
						fileSave("APKInstaller pName " + apps.get(i).pName);
						outputStream.write( intToBytes2(apps.get(i).pName.getBytes().length) );
						outputStream.write( apps.get(i).pName.getBytes() );
						fileSave("APKInstaller versionName " + apps.get(i).versionName);
						outputStream.write( intToBytes2(apps.get(i).versionName.getBytes().length) );
						outputStream.write( apps.get(i).versionName.getBytes() );
						fileSave("APKInstaller versionCode");
						outputStream.write( intToBytes2(apps.get(i).versionCode));
						outputStream.write( intToBytes2(apps.get(i).flag_app));
						fileSave("APKInstaller App Size");
						outputStream.write( longToBytes2(apps.get(i).size));
						fileSave("APKInstaller App Install Time");
						outputStream.write( longToBytes2(apps.get(i).firstInstallTime ));
						fileSave("APKInstaller App Last Update Time");
						outputStream.write( longToBytes2(apps.get(i).lastUpdateTime));
						
						try {
							Bitmap bmp = ((BitmapDrawable) apps.get(i).icon)
									.getBitmap();
							ByteArrayOutputStream stream = new ByteArrayOutputStream();
							bmp.compress(Bitmap.CompressFormat.PNG, 1, stream);
							byte[] bitmapData = stream.toByteArray();
							
							outputStream.write( intToBytes2(bitmapData.length) );
							outputStream.write( bitmapData );
						}
						catch (Exception e)
						{
							fileSave("APKInstaller get Bitmap Error");
						}
						
					}					
					
					fileSave("APKInstaller Command End");
					packet.PacketData = outputStream.toByteArray( );
					packet.Length = packet.PacketData.length + 8;

					return packet;
				}
				
				else if (str.equals(PKT_SCREEN_CAPTURE)) {
					
					packet.PacketType = PKT_SCREEN_CAPTURE;
						
					//String mPath = Environment.getExternalStorageDirectory().toString() + "/" + "cap.png";   

					MainActivity.this.getWindow().getDecorView().setDrawingCacheEnabled(true);
					Bitmap b = MainActivity.this.getWindow().getDecorView().getDrawingCache();
					ByteArrayOutputStream stream = new ByteArrayOutputStream();
					b.compress(CompressFormat.JPEG, 95, stream);
					packet.PacketData = stream.toByteArray( );
					packet.Length = packet.PacketData.length + 8;
					
					return packet;
				} else if (str.equals(PKT_DEVICE_UNIQUE)) {
					packet.PacketType = PKT_DEVICE_UNIQUE;
					fileSave("Info Packet Start");
					DInfo info = new DInfo();
					info.UId = Secure.getString(getContentResolver(), Secure.ANDROID_ID);
					info.Brand = Build.BRAND;					
					info.Device = Build.DEVICE;							
					info.FingerPrint = Build.FINGERPRINT;					
					info.Hardware = Build.HARDWARE;							
					info.Manufacrue = Build.MANUFACTURER;					
					info.Model = Build.MODEL;								
					info.Id = Build.ID;					
					info.Serial = Build.SERIAL;			
					
					try 
					{
						TelephonyManager tm = (TelephonyManager) getSystemService(TELEPHONY_SERVICE);
						info.IMEI = tm.getDeviceId();
					}catch (Exception e)
					{
						info.IMEI = "";
					}
					
					if ( info.IMEI == null )
						info.IMEI = "";
					
					String strTotal = info.UId + info.Brand + info.Device + info.FingerPrint
                            + info.Hardware + info.Manufacrue + info.Model
                            + info.Id + info.Serial + info.SDK_Int + info.IMEI;
					
					ByteArrayOutputStream outputStream = new ByteArrayOutputStream( );
					outputStream.write( intToBytes2(strTotal.getBytes().length) );		
						
					outputStream.write( intToBytes2(info.UId.getBytes().length) );		
					outputStream.write( info.UId.getBytes() );				
					
					outputStream.write( intToBytes2(info.Brand.getBytes().length) );		
					//fileSave("Info Packet 4");
					outputStream.write( info.Brand.getBytes() );
					
					outputStream.write( intToBytes2(info.Device.getBytes().length) );		
					outputStream.write( info.Device.getBytes() );		
					
					outputStream.write( intToBytes2(info.FingerPrint.getBytes().length) );		
					outputStream.write( info.FingerPrint.getBytes() );		
					
					outputStream.write( intToBytes2(info.Hardware.getBytes().length) );		
					outputStream.write( info.Hardware.getBytes() );		
					
					outputStream.write( intToBytes2(info.Manufacrue.getBytes().length) );		
					outputStream.write( info.Manufacrue.getBytes() );		
					
					outputStream.write( intToBytes2(info.Model.getBytes().length) );		
					outputStream.write( info.Model.getBytes() );		
					
					outputStream.write( intToBytes2(info.Id.getBytes().length) );		
					outputStream.write( info.Id.getBytes() );		
					
					outputStream.write( intToBytes2(info.Serial.getBytes().length) );		
					outputStream.write( info.Serial.getBytes() );		
					
					outputStream.write( intToBytes2(info.SDK_Int.getBytes().length) );		
					outputStream.write( info.SDK_Int.getBytes() );		
					
					outputStream.write( intToBytes2(info.IMEI.getBytes().length) );		
					outputStream.write( info.IMEI.getBytes() );		
					
					packet.PacketData = outputStream.toByteArray( );
					packet.Length = packet.PacketData.length + 8;

					return packet;
				}
                else if(str.equals(PKT_APK_INSTALL_COMPLETED))
                {

                    System.out.println("PKT_APK_INSTALL_COMPLETED is received.");

                    m_bStartAlarm = true;

                    packet.PacketType = PKT_CONNECT;
                    packet.PacketData = null;
                    packet.Length = 8;
                    return packet;
                }
				
			}
		} catch (Exception e) {
			Log.d("APKAgent", e.toString());
		}
		;

		return null;
	}

	// End parse packet with PC
	// /////////////////////////////////////////////////////////
	
	// /////////////////////////////////////////////////////////
		// Start get device info
		public class DInfo {
			public String UId = "";
			public String Brand = "";
			public String Device = "";
			public String FingerPrint = "";
			public String Hardware = "";
			public String Manufacrue = "";
			public String Model = "";
			public String Id = "";
			public String Serial = "";
			public String SDK_Int = "";
			public String IMEI = "";
		}
	//
	////////////////////////////////////////////////////////////

	// /////////////////////////////////////////////////////////
	// Start get installed package lists
	public class PInfo {
		public String appName = "";
		public String pName = "";
		public String versionName = "";
		public int versionCode = 0;
		public int flag_app;
		public long size;
		public long firstInstallTime;
		public long lastUpdateTime;
		public Drawable icon;
		
	}

	private boolean isSystemPackage(PackageInfo pkgInfo) {
		return ((pkgInfo.applicationInfo.flags & ApplicationInfo.FLAG_SYSTEM) != 0) ? true
				: false;
	}
	
	private boolean isUpdatedPakage(PackageInfo pkgInfo) {
		return ((pkgInfo.applicationInfo.flags & ApplicationInfo.FLAG_UPDATED_SYSTEM_APP) != 0) ? true
				: false;
	}
	

	private ArrayList<PInfo> getInstalledApps() {
		ArrayList<PInfo> res = new ArrayList<PInfo>();
		List<PackageInfo> packs = getPackageManager().getInstalledPackages(0);
		
		for (int i = 0; i < packs.size(); i++) {
			PackageInfo info = packs.get(i);
			if ( isSystemPackage(info) && !isUpdatedPakage(info))
				continue;

			
			PInfo newInfo = new PInfo();
			newInfo.appName = info.applicationInfo.loadLabel(
					getPackageManager()).toString();
			if (newInfo.appName == null)
				newInfo.appName = "";
			
			newInfo.flag_app = info.applicationInfo.flags ;
			newInfo.pName = info.packageName;
			if (newInfo.pName == null)
				newInfo.pName = "";
			newInfo.versionName = info.versionName;
			if (newInfo.versionName == null)
				newInfo.versionName = "";
			newInfo.versionCode = info.versionCode;
			newInfo.icon = info.applicationInfo.loadIcon(getPackageManager());			
			newInfo.size = Long.valueOf( new File(  
		            info.applicationInfo.publicSourceDir).length());  
			newInfo.firstInstallTime = info.firstInstallTime;
			newInfo.lastUpdateTime = info.lastUpdateTime;
			
			
			res.add(newInfo);
		}

		return res;
	}

	// End get installed package lists
	// /////////////////////////////////////////////////////////

	@Override
	public boolean onKeyDown(int keyCode, KeyEvent event) {
		if (keyCode == KeyEvent.KEYCODE_BACK) {
			if (serverThread != null && serverThread.isAlive()) {
				serverThread.stopThread();
				try {
					if (serverSocket != null)
						serverSocket.close();
				} catch (IOException e) {
					e.printStackTrace();
				}
			}

			MainActivity.this.finish();
		}

		return super.onKeyDown(keyCode, event);
	}
	
	public static void fileSave(String strData)
	{
		File f = new File(Environment.getExternalStorageDirectory(), "Download/ApkAgent.log");
//		File f = new File(Environment.getDataDirectory(), "Download/ApkAgent.log");
		String strLog = strData + "\r\n";
		
		try {
			OutputStream os = new FileOutputStream(f, true);
			os.write(strLog.getBytes());
			os.close();
		} catch (Exception e) {
			e.printStackTrace();
		}
		
		return;
	}

    public void startCompletedAlarmActivity()
    {
        Intent it = new Intent(MainActivity.this,
                MainActivity.class);

        it.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        it.setAction(Intent.ACTION_MAIN);
        it.addCategory(Intent.CATEGORY_LAUNCHER);

        startActivity(it);

        Intent alertIntent = new Intent(MainActivity.this, CompletedAlarmActivity.class);
        startActivity(alertIntent);
    }
}
