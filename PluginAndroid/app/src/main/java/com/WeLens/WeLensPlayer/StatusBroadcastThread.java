package com.WeLens.WeLensPlayer;
import android.content.Context;
import android.net.DhcpInfo;
import android.net.wifi.WifiManager;
import android.os.Build;
import android.util.Log;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;

import static android.content.ContentValues.TAG;










/**
 * Created by Brian on 3/1/2017.
 */

public class StatusBroadcastThread extends Thread
{
	public int broadcastDurationSec = 5;

	public int delayMS = 1000;

	boolean quitFlag = false;
	boolean isQuitRequested = false;









	public void RequestQuit()
	{
		quitFlag = true;
	}










	MainActivity mainActivity;











	static public DeviceState deviceState = DeviceState.Idle;

	static public String videoTitle = "";
	static public int filePositionSec = 0;

	enum DeviceState
	{
		Unknown,
		Idle,
		ReceivedPlayCommandWaiting,
		Playing,
	}





	public StatusBroadcastThread(MainActivity mainActivity)
	{
		this.mainActivity = mainActivity;
	}










	@Override
	public void run()
	{
		try
		{


			final InetAddress broadcastAddress = MainActivity.netAddress;
			while (!isQuitRequested)
			{
				DatagramSocket socket = null;
				socket = new DatagramSocket();
				socket.setSoTimeout(5000);
				JSONObject jsonMessage = new JSONObject();
				jsonMessage.put("deviceID", Build.SERIAL);
				jsonMessage.put("battery", (int)(MainActivity.instance.GetBatteryLevelPercent() * 100.0f));
				jsonMessage.put("temperatureC", Float.toString(MainActivity.instance.GetBatteryTemperature()) );


				if (quitFlag)
				{
					deviceState = DeviceState.Unknown;
					isQuitRequested = true;
				}
				jsonMessage.put("state", deviceState.ordinal());
				jsonMessage.put("videoTitle", videoTitle);
				jsonMessage.put("videoPositionSeconds", filePositionSec);
				String msg = jsonMessage.toString();
				byte[] buf = msg.getBytes();
				final DatagramPacket outboundPacket = new DatagramPacket(buf, buf.length, broadcastAddress, MainActivity.udpClientStatePort);
				socket.send(outboundPacket);
				socket.close();
				if (!isQuitRequested)
				{
					Thread.sleep(delayMS);
				}
			}
		}
		catch (Exception e)
		{

		}
	}










}
