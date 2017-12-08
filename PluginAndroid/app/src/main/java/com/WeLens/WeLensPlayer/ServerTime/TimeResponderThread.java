package com.WeLens.WeLensPlayer.ServerTime;

import android.app.Activity;
import android.content.Context;
import android.net.DhcpInfo;
import android.net.wifi.WifiManager;
import android.util.Log;

import com.WeLens.WeLensPlayer.MainActivity;
import com.unity3d.player.UnityPlayer;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;

import static android.content.ContentValues.TAG;










/**
 * Created by alien on 4/7/2017.
 */

public class TimeResponderThread extends Thread
{
	boolean isQuitRequested = false;
	Activity activity;

	public TimeResponderThread(Activity activity)
	{
		this.activity = activity;
	}



	@Override
	public void run()
	{
		DatagramSocket receiveSocket = null;
		DatagramSocket sendSocket = null;

		try
		{
			// Open the socket.
			receiveSocket = new DatagramSocket(MainActivity.udpServerTimeClientSendPort);
			receiveSocket.setBroadcast(true);
			sendSocket = new DatagramSocket();
			sendSocket.setSoTimeout(5000);

			// Listen until quit is requested.
			final InetAddress broadcastAddress = MainActivity.netAddress;
			while (!isQuitRequested)
			{
				// Receive a broadcast.
				final byte[] buffer = new byte[1024];
				final DatagramPacket receivedPacket = new DatagramPacket(buffer, buffer.length);
				receiveSocket.receive(receivedPacket);

				// XXX TEST
				sleep(20);

				// Construct the response.
				String responseString = ", \"response\": " + System.currentTimeMillis() + "}";
				System.arraycopy(responseString.getBytes(), 0, buffer, receivedPacket.getLength()-1, responseString.length());


				// Respond.
				final DatagramPacket outboundPacket = new DatagramPacket(buffer, buffer.length, broadcastAddress, MainActivity.udpServerTimeClientReceivePort);
				sendSocket.send(outboundPacket);
			}
		}
		catch (Exception e)
		{
		}
	}







}

