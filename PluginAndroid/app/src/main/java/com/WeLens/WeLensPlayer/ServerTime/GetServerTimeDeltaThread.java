package com.WeLens.WeLensPlayer.ServerTime;

import android.app.Activity;

import com.unity3d.player.UnityPlayer;

import java.text.SimpleDateFormat;
import java.util.Arrays;
import java.util.Date;










/**
 * Created by alien on 4/7/2017.
 */




public class GetServerTimeDeltaThread extends Thread
{
	int timeResponseCount = 0;
	int numResponsesRequired = 5;
	long[] timeDeltas = new long[numResponsesRequired];


	Activity activity;


	public GetServerTimeDeltaThread(Activity activity)
	{
		this.activity = activity;
	}


	public void RecordServerTime(long time)
	{
		SimpleDateFormat sdf = new SimpleDateFormat("MMM dd,yyyy HH:mm:ss");

		Date resultdate = new Date(time);
		String result = sdf.format(resultdate);
		UnityPlayer.UnitySendMessage("Platform", "SetServerTime", result);
	}


	public void RecordServerTimeDelta(long delta)
	{
		if (timeResponseCount < numResponsesRequired)
		{
			timeDeltas[timeResponseCount++] = delta;
		}
	}


	@Override
	public void run()
	{
		// Create the receiver thread and start it.
		ReceiveServerTimeRequestThread receiveThread = new ReceiveServerTimeRequestThread(this);
		receiveThread.start();


		// Create the request thread and start it.
		SendServerTimeRequestThread getThread = new SendServerTimeRequestThread(activity);
		getThread.start();


		// Wait until a certain number of responses have been received.
		while (timeResponseCount != numResponsesRequired)
		{
			try
			{
				Thread.sleep(500);
			}
			catch (InterruptedException e)
			{
			}
		}


		receiveThread.RequestQuit();
		getThread.RequestQuit();


		// Compute the median time delta.
		long timeDelta = 0;
		if (false)
		{
			Arrays.sort(timeDeltas);
			timeDelta = timeDeltas[numResponsesRequired/2];

		}
		else
		{
			for (int i=0; i< numResponsesRequired; ++i)
			{
				timeDelta += timeDeltas[i];
			}
		}

		timeDelta /= numResponsesRequired;
		UnityPlayer.UnitySendMessage("Platform", "SetServerTimeDelta", Long.toString(timeDelta));
	}



}
