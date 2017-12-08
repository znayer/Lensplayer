package com.WeLens.WeLensPlayer;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.SocketException;
import java.net.SocketTimeoutException;
import java.nio.charset.Charset;
import java.util.ArrayList;










/**
 * Created by Brian on 3/1/2017.
 */

public class CommandListenerThread extends Thread
{

	boolean isQuitRequested = false;



	enum MessageType
	{
		PlayVideo,
		StartEvent,
		Reset,
	};





	void RequestQuit()
	{
		isQuitRequested = true;
	}





	@Override
	public void run()
	{
		DatagramSocket socket = null;

		try
		{
			// Open the socket.
			socket = new DatagramSocket(MainActivity.udpCommandClientPort);
			socket.setBroadcast(true);

			// Listen until quit is requested.
			while (!isQuitRequested)
			{
				// Receive a broadcast.
				final byte[] buffer = new byte[1024];
				final DatagramPacket receivedPacket = new DatagramPacket(buffer, buffer.length);
				socket.receive(receivedPacket);


				// Convert the broadcast into a JSON object.
				String v = new String(buffer, Charset.forName("UTF-8"));
				JSONObject message = new JSONObject(v);
				String messageString = message.toString();

				UnityPlayer.UnitySendMessage("Platform", "OnUDPCommand", messageString);
			}
		}
		catch (Exception e)
		{
		}
	}


}
