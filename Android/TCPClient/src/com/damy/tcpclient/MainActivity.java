package com.damy.tcpclient;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.Socket;
import java.net.UnknownHostException;

import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.app.Activity;

public class MainActivity extends Activity {
	
	private String SERVER_IP = "127.0.0.1";
	private static final int SERVER_PORT = 25000;
	
	private String read;
	
	private Socket socketClient = null;
	private BufferedWriter networkWriter = null;
	private BufferedReader networkReader = null;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		try {
			socketClient = new Socket(SERVER_IP, SERVER_PORT);
			networkWriter = new BufferedWriter(new OutputStreamWriter(socketClient.getOutputStream()));
			networkReader = new BufferedReader(new InputStreamReader(socketClient.getInputStream()));
			
			PrintWriter out = new PrintWriter(networkWriter, true);
			
			Thread checkThread = new echoThread(out);
			checkThread.start();
						
			out.println("CONN");
			out.flush();
			Thread.sleep(3000);
			
			out.println("IDPL");
			out.flush();
			Thread.sleep(3000);
			
			out.println("DCON");
			out.flush();
			Thread.sleep(3000);
			
			try {
    			if (socketClient != null)
    			{
    				socketClient.close();
    				System.out.println("Socket closed");
    			}    			
			} catch (IOException e) {
				e.printStackTrace();
			}
						
		} catch (UnknownHostException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		} catch (InterruptedException e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}
	}
	
	@Override
	public void onStop()
	{
		super.onStop();
		
		/*
		try {
			if (socketClient != null)
				socketClient.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
		*/
	}
	
	class echoThread extends Thread
	{
		PrintWriter pw;
		
		public echoThread(PrintWriter out)
		{
			this.pw = out;
		}
		
		public void run() {
			try 
			{
				while (true)
				{
					Thread.sleep(100);
				
					while( (read = networkReader.readLine()) != null )
						Log.d("AAA", read);
				}
			}
			catch (Exception e) {}
		}
	};	
	
	@Override
    public boolean onKeyDown(int keyCode, KeyEvent event)
	{
    	if( keyCode == KeyEvent.KEYCODE_BACK )
    	{
    		try {
    			if (socketClient != null)
    				socketClient.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
    		
    		MainActivity.this.finish();
    	}
	  
    	return super.onKeyDown(keyCode, event);
	 }
	
	public class Packet 
	{
		public int Length;
		public char []PacketType;
		public String PacketData;
		
		public Packet()
		{
			Length = 0;
			PacketType = new char[4];
			PacketData = "";
		}
	}
}
