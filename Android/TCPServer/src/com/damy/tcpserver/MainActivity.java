/*
 *											 Packet Structure
 *
 *			-------------------------------------------------------------------------------------------------
 *		   |	Packet Length (4 Bytes)	|	Packet Type (4 Bytes)	|			Packet Data(Variable)  		 |			
 *			-------------------------------------------------------------------------------------------------
 *
 *		  
 */

package com.damy.tcpserver;

import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.ArrayList;
import java.util.List;

import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.Message;
import android.os.StatFs;
import android.view.KeyEvent;
import android.widget.ImageView;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.graphics.Bitmap;
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
	
	private static final int SERVER_PORT = 25000;
	private static final int SEND_REQUEST = 0;
    private static final int SEND_CONNECT = 1;
    private static final int SEND_DISCONNECT = 2;
    private static final int REQUEST_PERIOD_MS = 1000;
    
	private ServerSocket serverSocket = null;
	private TCPServerThread serverThread = null;
	private SendMessageHandler mMainHandler = null;
		
	private ImageView imgConnectState = null;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		imgConnectState = (ImageView) findViewById(R.id.imgConnectState);
		
		try 
		{
			serverSocket = new ServerSocket(SERVER_PORT);
			mMainHandler = new SendMessageHandler();
			serverThread = new TCPServerThread();
		}
		catch (IOException e) 
		{
			e.printStackTrace();
		}
		
		serverThread.start();
	}
	    
    class TCPServerThread extends Thread implements Runnable 
    {
    	private boolean isPlay = false;
    	
        public TCPServerThread() 
        {
            isPlay = true;
        }
         
        public void isThreadState(boolean isPlay) 
        {
            this.isPlay = isPlay;
        }
         
        public void stopThread() 
        {
            isPlay = false;
        }
         
		@Override
        public void run() 
        {
            super.run();
            System.out.println("Server Thread start");
            while (isPlay) 
            {                
            	try 
            	{
                    System.out.println("Waiting a connection");
                    
                    Socket socket = serverSocket.accept();                    
                    System.out.println(socket.getInetAddress() + " received connection request");
                    
                    Message msg = mMainHandler.obtainMessage();
                    msg.what = SEND_REQUEST;
                    msg.arg1 = 0;
                    msg.arg2 = 0;
                    msg.obj = socket;
                    mMainHandler.sendMessage(msg);
                    
                    Thread.sleep(REQUEST_PERIOD_MS);
                }
            	catch (InterruptedException e) 
            	{
					e.printStackTrace();
				}
            	catch (IOException e) {
					e.printStackTrace();
				}
            }
            
            System.out.println("Server Thread is killed");
        }
    }
    
    @SuppressLint("HandlerLeak")
	class SendMessageHandler extends Handler 
	{
        @Override
        public void handleMessage(Message msg) 
        {
            super.handleMessage(msg);
             
            switch (msg.what) {
            case SEND_REQUEST:
            	TCPClientThread clientThread = new TCPClientThread((Socket)msg.obj);
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
    
    class TCPClientThread extends Thread implements Runnable 
    {
        private Socket socket;
        private boolean isRunnable = false;
         
        public TCPClientThread(Socket socket) 
        {
        	this.socket = socket;
        	this.isRunnable = true;
        }
         
        public void setThreadState(boolean isRunnable) 
        {
        	this.isRunnable = isRunnable;
        }
         
        public void stopThread() 
        {
        	this.isRunnable = false;
        }
         
		@Override
        public void run() 
        {
            super.run();

            System.out.println("Client Thread start");
        	
            while (isRunnable)
            {
	        	if (socket != null)
	        	{
	        		Packet packet =null;
	        		packet = parsePacket(this, socket);
	        		if (packet != null)
	        		{        		
		        		try
		        		{
							PrintWriter pw = new PrintWriter(new OutputStreamWriter(socket.getOutputStream()));
							pw.println(packet.PacketType);
							pw.println(packet.Length);
							pw.println(packet.PacketData);
							pw.flush();
						}
		        		catch (IOException e) 
		        		{
							e.printStackTrace();
						}
		        		
		        		if ( packet.PacketType.equals(PKT_DISCONNECT) )
		        		{
		        			setThreadState(false);
		        		}
	        		}
	        	}
	        	else
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
    
	///////////////////////////////////////////////////////////
	// Start parse packet with PC      
    public class Packet 
	{
		public int Length;
		public String PacketType;
		public String PacketData;
		
		public Packet()
		{
			Length = 0;
			PacketType = "";
			PacketData = "";
		}
	}    

    private Packet parsePacket(TCPClientThread thread, Socket socket)
    {
    	Packet packet = new Packet();
    	
    	String strData;
    	try
    	{
    		BufferedReader br = new BufferedReader(new InputStreamReader(socket.getInputStream()));
    		while( (strData = br.readLine()) != null )
    		{
    			if (strData.length() == 4)
    			{
    				if (strData.equals(PKT_CONNECT))
    				{
    					mMainHandler.sendEmptyMessage(SEND_CONNECT);
    					packet.PacketType = PKT_CONNECT;
    		        	packet.PacketData = "";
    		        	packet.Length = packet.PacketData.length() + 8;
    		        	
    		        	System.out.println("CONNECT");
    		        	
    		        	return packet;
    				}
    				else if (strData.equals(PKT_DISCONNECT))
    				{
    					mMainHandler.sendEmptyMessage(SEND_DISCONNECT);
    					packet.PacketType = PKT_DISCONNECT;
    		        	packet.PacketData = "";
    		        	packet.Length = packet.PacketData.length() + 8;
    		        	
    		        	System.out.println("DISCONNECT");
    		        	    		        	
    		        	return packet;
    				}
    				else if (strData.equals(PKT_SDCARD_FREESPACE))
    				{
    					packet.PacketType = PKT_SDCARD_FREESPACE;
    					StatFs stat = new StatFs(Environment.getExternalStorageDirectory().getPath());
    		        	long bytesAvailable = (long) stat.getAvailableBlocks() * (long)stat.getBlockSize();
    		        	packet.PacketData = Long.toString(bytesAvailable);
    		        	packet.Length = packet.PacketData.length() + 8;
    		        	System.out.println("FREE SPACE");
    		        	return packet;
    				}
    				else if (strData.equals(PKT_SDCARD_TOTALSPACE))
    				{
    					packet.PacketType = PKT_SDCARD_TOTALSPACE;
    					StatFs stat = new StatFs(Environment.getExternalStorageDirectory().getPath());
    		        	long bytesTotal = (long) stat.getBlockCount() * (long)stat.getBlockSize();
    		        	packet.PacketData = Long.toString(bytesTotal);
    		        	packet.Length = packet.PacketData.length() + 8;
    		        	
    		        	return packet;
    				}
    				else if (strData.equals(PKT_INTERNALMEMORY_FREESPACE))
    				{
    					packet.PacketType = PKT_INTERNALMEMORY_FREESPACE;
    					StatFs stat = new StatFs(Environment.getDataDirectory().getPath());
    		        	long bytesAvailable = (long) stat.getAvailableBlocks() * (long)stat.getBlockSize();
    		        	packet.PacketData = Long.toString(bytesAvailable);
    		        	packet.Length = packet.PacketData.length() + 8;
    		        	
    		        	return packet;
    				}
    				else if (strData.equals(PKT_INTERNALMEMORY_TOTALSPACE))
    				{
    					packet.PacketType = PKT_INTERNALMEMORY_TOTALSPACE;
    					StatFs stat = new StatFs(Environment.getDataDirectory().getPath());
    		        	Long bytesTotal = (long) stat.getBlockCount() * (long)stat.getBlockSize();
    		        	packet.PacketData = Long.toString(bytesTotal);
    		        	packet.Length = packet.PacketData.length() + 8;
    		        	
    		        	return packet;
    				}
    				else if (strData.equals(PKT_INSTALLED_PROGRAM_LISTS))
    				{
    					packet.PacketType = PKT_INSTALLED_PROGRAM_LISTS;
    					ArrayList<PInfo> apps = getInstalledApps();
    					for ( int i = 0; i < apps.size(); i++ )
    					{
    						String strIcon;
    						Bitmap bmp = ((BitmapDrawable)apps.get(i).icon).getBitmap();
    						ByteArrayOutputStream stream = new ByteArrayOutputStream();
    						bmp.compress(Bitmap.CompressFormat.PNG, 1, stream);
    						byte[] bitmapData = stream.toByteArray();
    						strIcon = new String(bitmapData, "UTF-8");
    						
    						String data = "";
    						data = String.format("%s, %s, %s, %s, %s\n", apps.get(i).appName, apps.get(i).pName, apps.get(i).versionName, apps.get(i).versionCode, strIcon);
    						packet.PacketData += data;
    					}
    					
    		        	packet.Length = packet.PacketData.length() + 8;
    		        	
    		        	return packet;
    				}
    			}
    		}
    	}
    	catch (Exception e){};    	
    	
    	return null;
    }
	// End parse packet with PC
	///////////////////////////////////////////////////////////
    
    ///////////////////////////////////////////////////////////
    // Start get installed package lists
    public class PInfo
    {
    	public String appName = "";
    	public String pName = "";
    	public String versionName = "";
    	public int versionCode = 0;
    	public Drawable icon;
    }
    
    private boolean isSystemPackage(PackageInfo pkgInfo)
    {
    	return ((pkgInfo.applicationInfo.flags & ApplicationInfo.FLAG_SYSTEM) != 0) ? true : false;
    }
    
    private ArrayList<PInfo> getInstalledApps()
    {
    	ArrayList<PInfo> res = new ArrayList<PInfo>();
    	List<PackageInfo> packs = getPackageManager().getInstalledPackages(0);
    	for ( int i = 0; i < packs.size(); i++ )
    	{
    		PackageInfo info = packs.get(i);
    		if ( isSystemPackage(info) )
    			continue;
    		
	    	PInfo newInfo = new PInfo();
	    	newInfo.appName = info.applicationInfo.loadLabel(getPackageManager()).toString();
	    	newInfo.pName = info.packageName;
	    	newInfo.versionName = info.versionName;
	    	newInfo.versionCode = info.versionCode;
	    	newInfo.icon = info.applicationInfo.loadIcon(getPackageManager());
	    	res.add(newInfo);
    	} 
    	
    	return res;
    }
	// End get installed package lists
	///////////////////////////////////////////////////////////
        
    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event)
	{
    	if( keyCode == KeyEvent.KEYCODE_BACK )
    	{
    		if ( serverThread != null && serverThread.isAlive() )
			{
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
}
