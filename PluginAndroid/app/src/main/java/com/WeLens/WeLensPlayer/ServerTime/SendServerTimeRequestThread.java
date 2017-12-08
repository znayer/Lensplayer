package com.WeLens.WeLensPlayer.ServerTime;

import android.app.Activity;
import android.content.Context;
import android.net.DhcpInfo;
import android.net.wifi.WifiManager;
import android.os.Build;
import android.util.Log;

import com.WeLens.WeLensPlayer.MainActivity;
import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;

import static android.content.ContentValues.TAG;










/**
 * Created by alien on 4/7/2017.
 */

public class SendServerTimeRequestThread extends Thread
{
	boolean isQuitRequested = false;

	Activity activity;

	public SendServerTimeRequestThread(Activity activity)
	{
		this.activity = activity;
	}


	public void RequestQuit()
	{
		isQuitRequested = true;
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
				jsonMessage.put("timestamp", System.currentTimeMillis());

				String msg = jsonMessage.toString();
				byte[] buf = msg.getBytes();
				final DatagramPacket outboundPacket = new DatagramPacket(buf, buf.length, broadcastAddress, MainActivity.udpServerTimeClientSendPort);
				socket.send(outboundPacket);
				socket.close();
				Thread.sleep(100);
			}
		}
		catch (Exception e)
		{
		}
	}





}
