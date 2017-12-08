package com.WeLens.WeLensPlayer.ServerTime;

import android.os.Build;
import android.util.Log;

import com.WeLens.WeLensPlayer.MainActivity;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.nio.charset.Charset;










/**
 * Created by alien on 4/7/2017.
 */

public class ReceiveServerTimeRequestThread extends Thread
{
	GetServerTimeDeltaThread getServerTimeDeltaThread;

	public ReceiveServerTimeRequestThread(GetServerTimeDeltaThread getServerTimeDeltaThread)
	{
		this.getServerTimeDeltaThread = getServerTimeDeltaThread;
	}


	public void RequestQuit()
	{
		this.interrupt();
	}


	int testval = 0;


	@Override
	public void run()
	{
		DatagramSocket socket = null;
		boolean isQuitRequested = false;

		try
		{
			// Open the socket.
			socket = new DatagramSocket(MainActivity.udpServerTimeClientReceivePort);
			socket.setBroadcast(true);

			// Listen until quit is requested.
			while (!isQuitRequested)
			{
				// Receive a broadcast.
				final byte[] buffer = new byte[1024];
				final DatagramPacket receivedPacket = new DatagramPacket(buffer, buffer.length);
				socket.receive(receivedPacket);


				// Record the time.
				long receiveTime = System.currentTimeMillis();


				// Convert the broadcast into a JSON object.
				String v = new String(buffer, Charset.forName("UTF-8"));
				JSONObject message = new JSONObject(v);
				String messageString = message.toString();


				// If the message was for another device, then continue.
				String messageDeviceID = message.getString("deviceID");
				if (!messageDeviceID.equals(Build.SERIAL))    // XXX may want to use LensPass value.
				{
					continue;
				}


				// Compute the time delta and record.

				long messageTimestamp = 0;
				long messageResponse = 0;

				try
				{
					messageTimestamp = message.getLong("timestamp");
					messageResponse = message.getLong("response");
				}
				catch (JSONException ignored)
				{
					continue;
				}

				long response = message.getLong("response");
				long latencyMS = (receiveTime - message.getLong("timestamp")) / 2;
				long timeDelta = receiveTime - response + latencyMS;
				getServerTimeDeltaThread.RecordServerTimeDelta(timeDelta);
				getServerTimeDeltaThread.RecordServerTime(response);
			}
		}
		catch (Exception e)
		{
			if (socket != null)
			{
				socket.close();
			}
		}
	}



}
