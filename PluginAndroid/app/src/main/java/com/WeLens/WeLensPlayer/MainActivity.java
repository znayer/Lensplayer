package com.WeLens.WeLensPlayer;

import android.content.BroadcastReceiver;
import android.content.ContentResolver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.media.ThumbnailUtils;
import android.net.ConnectivityManager;
import android.net.DhcpInfo;
import android.net.NetworkInfo;
import android.net.Uri;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.AsyncTask;
import android.os.BatteryManager;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.util.Log;
import android.widget.ImageView;

import com.WeLens.WeLensPlayer.ServerTime.GetServerTimeDeltaThread;
import com.WeLens.WeLensPlayer.ServerTime.TimeResponderThread;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.net.InetAddress;
import java.net.InterfaceAddress;
import java.net.NetworkInterface;
import java.net.SocketException;
import java.net.UnknownHostException;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.Enumeration;
import java.util.List;
import java.util.TimeZone;

import static android.content.ContentValues.TAG;










public class MainActivity extends UnityPlayerActivity
{
	boolean hasIntentBeenRead = false;

	BroadcastReceiverBattery batteryBroadcastReceiver;

	float currentBatteryTemperatureCelsius = 0;

	float currentBatteryLevelPercent = 0;

	CommandListenerThread udpListener;

	StatusBroadcastThread pingBroadcast;

	GetServerTimeDeltaThread getServerTimeDeltaThread;

	static public MainActivity instance;

	static public ArrayList<Long> processedMessageIDs = new ArrayList<>();

	public static final int udpCommandClientPort = 40004;
	public static final int udpClientStatePort = 40005;
	public static final int udpServerTimeClientSendPort = 40006;
	public static final int udpServerTimeClientReceivePort = 40007;



	public static InetAddress netAddress;







	@Override
	protected void onDestroy()
	{
		// Call the required super.
		super.onDestroy();


		unregisterReceiver(batteryBroadcastReceiver);

		udpListener.RequestQuit();
		pingBroadcast.RequestQuit();
	}










	@Override
	protected void onCreate(Bundle savedInstanceState)
	{
		// Call the parent's function.
		super.onCreate(savedInstanceState);


		// Store a reference to this activity.
		instance = this;

		netAddress = getSubnetBroadcastAddress();
		String msg = "Subnet Broadcast Address: " + netAddress.toString();
		UnityPlayer.UnitySendMessage("Platform", "Log", msg);



		// Register the battery broadcast receiver.
		batteryBroadcastReceiver = new BroadcastReceiverBattery();
		registerReceiver(batteryBroadcastReceiver, new IntentFilter(Intent.ACTION_BATTERY_CHANGED));


		// Create the udp listener threed.
		udpListener = new CommandListenerThread();
		udpListener.start();


		// Create the broadcaster.
		pingBroadcast = new StatusBroadcastThread(this);
		pingBroadcast.start();


//		TimeResponderThread timeResponderThread = new TimeResponderThread(this);
//		timeResponderThread.start();


		getServerTimeDeltaThread = new GetServerTimeDeltaThread(this);
		getServerTimeDeltaThread.start();
	}










	@Override
	protected void onResume()
	{
		super.onResume();

		if (false == hasIntentBeenRead)
		{
			hasIntentBeenRead = true;
			final String filename = getIntent().getStringExtra("FILENAME");
			if ((null != filename) && (filename.length() > 0))
			{
				UnityPlayer.UnitySendMessage("StartVideoReceiver", "SetStartVideo", filename);
			}
		}
	}










	private static Cursor mediaCursor;
	private static Cursor genresCursor;

	private static String[] mediaProjection = {
			MediaStore.Audio.Media._ID,
			MediaStore.Audio.Media.ARTIST,
			MediaStore.Audio.Media.ALBUM,
			MediaStore.Audio.Media.TITLE,
			MediaStore.Audio.Media.DATA,
			MediaStore.Audio.Media.DURATION,
	};
	private static String[] genresProjection = {
			MediaStore.Audio.Genres.NAME,
			MediaStore.Audio.Genres._ID
	};










	public String GetMusicDataJSON()
	{
		mediaCursor = getContentResolver().query(MediaStore.Audio.Media.EXTERNAL_CONTENT_URI,
				mediaProjection, null, null, null);

		int artist_column_index = mediaCursor
				.getColumnIndexOrThrow(MediaStore.Audio.Media.ARTIST);
		int album_column_index = mediaCursor
				.getColumnIndexOrThrow(MediaStore.Audio.Media.ALBUM);
		int title_column_index = mediaCursor
				.getColumnIndexOrThrow(MediaStore.Audio.Media.TITLE);
		int id_column_index = mediaCursor
				.getColumnIndexOrThrow(MediaStore.Audio.Media._ID);
		int filepath = mediaCursor
				.getColumnIndexOrThrow(MediaStore.Audio.Media.DATA);
		int durationMS = mediaCursor
				.getColumnIndexOrThrow(MediaStore.Audio.AudioColumns.DURATION);

		JSONArray retval = new JSONArray();
		try
		{


			if (mediaCursor.moveToFirst())
			{
				do
				{
					JSONObject musicData = new JSONObject();
					musicData.put("filePath", mediaCursor.getString(filepath));
					musicData.put("durationMS", mediaCursor.getString(durationMS));
					musicData.put("title", mediaCursor.getString(title_column_index));
					int musicId = Integer.parseInt(mediaCursor.getString(id_column_index));

					Uri uri = MediaStore.Audio.Genres.getContentUriForAudioId("external", musicId);
					genresCursor = getContentResolver().query(uri,
							genresProjection, null, null, null);
					int genre_column_index = genresCursor.getColumnIndexOrThrow(MediaStore.Audio.Genres.NAME);

					String genres = "";
					if (genresCursor.moveToFirst())
					{
						do
						{
							genres += genresCursor.getString(genre_column_index).toLowerCase() + " ";
						} while (genresCursor.moveToNext());
					}
					musicData.put("genres", genres);

					retval.put(musicData);
				} while (mediaCursor.moveToNext());
			}
			mediaCursor.close();
		}
		catch (JSONException e)
		{
		}


		return retval.toString();
	}










	public String GetImageDataJSON()
	{
		String[] projection = {
				MediaStore.Images.Media.TITLE,
				MediaStore.Images.Media.DATA,
		};

		mediaCursor = getContentResolver().query(MediaStore.Images.Media.EXTERNAL_CONTENT_URI,
				projection, null, null, null);

		int title_column_index = mediaCursor.getColumnIndexOrThrow(MediaStore.Images.Media.TITLE);
		int filepath = mediaCursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);

		JSONArray retval = new JSONArray();
		try
		{
			if (mediaCursor.moveToFirst())
			{
				do
				{
					JSONObject musicData = new JSONObject();
					musicData.put("filePath", mediaCursor.getString(filepath));
					musicData.put("title", mediaCursor.getString(title_column_index));
					retval.put(musicData);
				} while (mediaCursor.moveToNext());
			}
			mediaCursor.close();
		}
		catch (JSONException e)
		{
		}


		return retval.toString();
	}










	private class GetVideoThumbnailPixelsAsyncTask extends AsyncTask<String, Void, Void>
	{
		String videoPath;










		@Override
		protected Void doInBackground(String... params)
		{
			try
			{
				videoPath = params[0];
				Bitmap bmp = ThumbnailUtils.createVideoThumbnail(videoPath, MediaStore.Images.Thumbnails.MINI_KIND);
				if (null == bmp)
				{
					return null;
				}
				File f = File.createTempFile("vid", ".jpg");
				FileOutputStream out = new FileOutputStream(f);
				bmp.compress(Bitmap.CompressFormat.JPEG, 100, out);

				try
				{

					JSONObject retval = new JSONObject();
					retval.put("videoFilePath", videoPath);
					retval.put("thumbnailFilePath", f.getAbsolutePath());
//					UnityPlayer.UnitySendMessage("PlatformSupportAndroid", "OnThumbnailObtained", retval.toString());
					UnityPlayer.UnitySendMessage("Platform", "OnThumbnailCreated", retval.toString());
				}
				catch (JSONException e)
				{
					e.printStackTrace();
				}
			}
			catch (IOException e)
			{
				e.printStackTrace();
			}


/*
			try
			{

				JSONObject retval = new JSONObject();
				retval.put("filePath", videoPath);
				retval.put("width", 5);
				retval.put("height", 5);
//				retval.put("pixels", Arrays.toString(thumbnailPixelData));
				UnityPlayer.UnitySendMessage("PlatformSupportAndroid", "OnThumbnailObtained", retval.toString());
			}
			catch (JSONException e)
			{
				e.printStackTrace();
			}
*/
			return null;
		}










		@Override
		protected void onPostExecute(Void result)
		{
			// Remove this request from the request list, and if there are more requests, start the next.
			for (int i = 0; i < thumbnailRequests.size(); ++i)
			{
				if (thumbnailRequests.get(i).filePath.equals(videoPath))
				{
					thumbnailRequests.remove(i);
					break;
				}
			}
			if (thumbnailRequests.size() > 0)
			{
				runOnUiThread(new Runnable()
				{
					@Override
					public void run()
					{
						currentCreateThumbnailAsyncTask = new GetVideoThumbnailPixelsAsyncTask();
						currentCreateThumbnailAsyncTask.execute(thumbnailRequests.get(0).filePath);
					}
				});
			}
		}










		@Override
		protected void onPreExecute()
		{
		}










		@Override
		protected void onProgressUpdate(Void... values)
		{

		}
	}










	GetVideoThumbnailPixelsAsyncTask currentCreateThumbnailAsyncTask = null;










	class ThumbnailRequest
	{
		public String filePath;
	}










	ArrayList<ThumbnailRequest> thumbnailRequests = new ArrayList<>();










	public void CreateVideoThumbnail(String videoFilePath)
	{
		if (currentCreateThumbnailAsyncTask == null)
		{
			currentCreateThumbnailAsyncTask = new GetVideoThumbnailPixelsAsyncTask();
			currentCreateThumbnailAsyncTask.execute(videoFilePath);
		}
		else
		{
			ThumbnailRequest newRequest = new ThumbnailRequest();
			newRequest.filePath = videoFilePath;
			thumbnailRequests.add(newRequest);
		}
	}










	public int[] GetVideoThumbnailPixels(String videoPath)
	{
		// Create a thumbnail for the video and get the pixels.
		Bitmap bmp = ThumbnailUtils.createVideoThumbnail(videoPath, MediaStore.Images.Thumbnails.MINI_KIND);
		int[] pixels = new int[bmp.getWidth() * bmp.getHeight()];
		bmp.getPixels(pixels, 0, bmp.getWidth(), 0, 0, bmp.getWidth(), bmp.getHeight());

		int[] retval = new int[bmp.getWidth() * bmp.getHeight() + 2];

		retval[0] = bmp.getWidth();
		retval[1] = bmp.getHeight();
		for (int i = 0; i < pixels.length; ++i)
		{
			int redValue = Color.red(pixels[i]);
			int blueValue = Color.blue(pixels[i]);
			int greenValue = Color.green(pixels[i]);
			retval[i + 2] = ((redValue << 24) | (greenValue << 16) | (blueValue << 8) | (0xff));
		}

		return retval;
	}










	public String GetVideoDataJSON()
	{
		Uri uri = MediaStore.Video.Media.EXTERNAL_CONTENT_URI;
		String[] projection = {MediaStore.Video.Media._ID, MediaStore.Video.VideoColumns.DATA, MediaStore.Video.VideoColumns.TITLE, MediaStore.Video.VideoColumns.DURATION};
		Cursor mediaCursor = getContentResolver().query(uri, projection, null, null, null);
		int vidsCount = 0;
		JSONArray retval = new JSONArray();
		if (mediaCursor != null)
		{
			try
			{
				int filepath = mediaCursor
						.getColumnIndexOrThrow(MediaStore.Video.VideoColumns.DATA);
				int title = mediaCursor
						.getColumnIndexOrThrow(MediaStore.Video.VideoColumns.TITLE);
				int duration = mediaCursor
						.getColumnIndexOrThrow(MediaStore.Video.VideoColumns.DURATION);

				if (mediaCursor.moveToFirst())
				{
					do
					{
						JSONObject musicData = new JSONObject();
						musicData.put("filePath", mediaCursor.getString(filepath));
						musicData.put("title", mediaCursor.getString(title));
						musicData.put("durationMS", mediaCursor.getString(duration));
						retval.put(musicData);
					} while (mediaCursor.moveToNext());
				}
			}
			catch (JSONException e)
			{
			}

			mediaCursor.close();
		}

		return retval.toString();
	}










	public void SetDeviceState(int stateIdx, String videoTitle, int filePositionSec)
	{
		// XXX test
//		StatusBroadcastThread.deviceState = StatusBroadcastThread.DeviceState.ReceivedPlayCommandWaiting;


		StatusBroadcastThread.deviceState = StatusBroadcastThread.DeviceState.values()[stateIdx];
		StatusBroadcastThread.videoTitle = videoTitle;
		StatusBroadcastThread.filePositionSec = filePositionSec;
	}










	public String GetDeviceID()
	{
		return Build.SERIAL;
	}










	public float GetBatteryTemperature()
	{
		return currentBatteryTemperatureCelsius;
	}










	public float GetBatteryLevelPercent()
	{
		return currentBatteryLevelPercent;
	}










	;










	private class BroadcastReceiverBattery extends BroadcastReceiver
	{

		@Override
		public void onReceive(Context arg0, Intent intent)
		{

			currentBatteryTemperatureCelsius = intent.getIntExtra(BatteryManager.EXTRA_TEMPERATURE, 0) / 10.0f;

			int level = intent.getIntExtra(BatteryManager.EXTRA_LEVEL, -1);
			int scale = intent.getIntExtra(BatteryManager.EXTRA_SCALE, -1);
			currentBatteryLevelPercent = level / (float) scale;
		}
	}










	;










	public void WriteText(String fn, String tag, String msg)
	{
		try
		{
			String filename = Environment.getExternalStorageDirectory().getAbsolutePath() + "/" + fn;

			String path = filename.substring(0, filename.lastIndexOf('/'));
			File p = new File(path);
			if (!p.exists())
			{
				p.mkdirs();
			}

			FileOutputStream outputStream = new FileOutputStream(new File(filename), true);
/*
			DateFormat df = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss z");
			df.setTimeZone(TimeZone.getTimeZone("gmt"));
			String gmtTime = df.format(new Date());
			String message = gmtTime + "\t" + tag + ": " + msg;
			if (!message.endsWith("\n"))
			{
				message += "\n";
			}
*/
			String message = msg;
			outputStream.write(message.getBytes());
			outputStream.close();
		}
		catch (IOException e)
		{
			e.printStackTrace();
		}
	}





	InetAddress getSubnetBroadcastAddress()
	{
//		return getSubnetBroadcastAddressMethod0();
		return getSubnetBroadcastAddressMethod1();
//		return getSubnetBroadcastAddressMethod2();

	}







	/*************************************************************************
	 * Subnet Address compute method 0
	 *
	 **************************************************************************/

	static public InetAddress getSubnetBroadcastAddressMethod0()
	{
		try
		{

			final WifiManager wifi = (WifiManager) instance.getApplicationContext().getSystemService(Context.WIFI_SERVICE);
			final DhcpInfo dhcp = wifi.getDhcpInfo();
//			if (dhcp == null)
			if (dhcp.ipAddress == 0)
			{
				// No successful DHCP request. Go with best effort.
				Log.e("tag", "#getBroadcastAddress - No DHCP info so using generic broadcast address");
				return InetAddress.getByName("192.168.43.255");
			}

			final int broadcast = (dhcp.ipAddress & dhcp.netmask) | ~dhcp.netmask;
			final byte[] quads = new byte[4];
			for (int k = 0; k < 4; k++)
			{
				quads[k] = (byte) ((broadcast >> k * 8) & 0xFF);
			}
			InetAddress addr = InetAddress.getByAddress(quads);
			String msg = "SubnetBroadcastAddress = " + addr.getHostAddress();
			UnityPlayer.UnitySendMessage("Platform", "Log", msg);
			return addr;
		}
		catch (Exception e)
		{

		}

		return null;
	}







	/*************************************************************************
	 * Subnet Address compute method 1
	 *
	 **************************************************************************/

	public InetAddress getSubnetBroadcastAddressMethod1()
	{
		return getBroadcast(getIpAddress());
	}


	static public InetAddress getIpAddress()
	{
		InetAddress inetAddress = null;
		InetAddress myAddr = null;

		try {
			for (Enumeration< NetworkInterface > networkInterface = NetworkInterface
					.getNetworkInterfaces(); networkInterface.hasMoreElements();) {

				NetworkInterface singleInterface = networkInterface.nextElement();

				for (Enumeration< InetAddress > IpAddresses = singleInterface.getInetAddresses(); IpAddresses
						.hasMoreElements();) {
					inetAddress = IpAddresses.nextElement();

					if (!inetAddress.isLoopbackAddress() && (
							singleInterface.getDisplayName().contains("wlan0") ||
									singleInterface.getDisplayName().contains("eth0") ||
									singleInterface.getDisplayName().contains("ap0")
					)) {
						if (inetAddress.getHostAddress().charAt(3) == '.')
						{
							myAddr = inetAddress;
							UnityPlayer.UnitySendMessage("Platform", "Log", "Potential IP address: " + myAddr.toString());
						}
//						break;
					}
				}
			}

		} catch (SocketException ex) {
			UnityPlayer.UnitySendMessage("Platform", "Log", "IP address error: " + ex.toString());
		}
		return myAddr;
	}


	static public InetAddress getBroadcast(InetAddress inetAddr) {
		NetworkInterface temp;
		InetAddress iAddr = null;
		try {
			if (null == inetAddr)
			{
				UnityPlayer.UnitySendMessage("Platform", "Log", "Using default internet address.");
				return InetAddress.getByName("192.168.43.255");
			}

			temp = NetworkInterface.getByInetAddress(inetAddr);
			List< InterfaceAddress > addresses = temp.getInterfaceAddresses();

			for (InterfaceAddress inetAddress: addresses)
			{
				if (inetAddress.getBroadcast() != null)
				{
					if (inetAddress.getBroadcast().getHostAddress().charAt(3) == '.')
					{
						iAddr = inetAddress.getBroadcast();
						UnityPlayer.UnitySendMessage("Platform", "Log", "Potential Broadcast address: " + iAddr.toString());
					}

				}
//				break;
			}

			return iAddr;

		} catch (SocketException e) {

			e.printStackTrace();
			UnityPlayer.UnitySendMessage("Platform", "Log", "Address Error: " + e.getMessage());
		}
		catch (UnknownHostException e)
		{
			UnityPlayer.UnitySendMessage("Platform", "Log", "Address Error: " + e.getMessage());
		}
		return null;
	}





	/*************************************************************************
	 * Subnet Address compute method 2
	 *
	 **************************************************************************/

	static public InetAddress getSubnetBroadcastAddressMethod2()
	{
		return NetUtils.getDeviceSubnetMask(instance);
	}


}


