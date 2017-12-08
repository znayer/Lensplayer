using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class PlatformSupportOSX : PlatformSupport
{
    public string GetShortDeviceID()
    {
        return "mac" + SystemInfo.deviceUniqueIdentifier;
    }
    public string GetFullDeviceID()
    {
        return "mac" + SystemInfo.deviceUniqueIdentifier;
    }

    public Platform.MusicData[] GetAvailableMusic()
	{
		return null;
	}



	public Platform.VideoData[] GetAvailableVideos()
	{
		DirectoryInfo info = new DirectoryInfo(LensPlayer.welensPath);
		FileInfo[] fileInfo = info.GetFiles();
		ArrayList filepaths = new ArrayList();
		foreach  (FileInfo file in fileInfo)
		{
			if (file.FullName.ToLower().EndsWith(".mp4"))
			{
				filepaths.Add(file.FullName);
			}
		}
		Platform.VideoData[] retval = new Platform.VideoData[filepaths.Count];
		for (int i=0; i<filepaths.Count; ++i)
		{
			string fp = (string)filepaths[i];
			retval[i] = new Platform.VideoData();
			retval[i].filePath = fp;
			retval[i].title = fp.Substring(fp.LastIndexOf('\\')+1);
		}
		return retval;
	}


	public void CreateVideoThumbnail(string videoFilePath)
	{
	}


	public void BroadcastDeviceState(Platform.DeviceState state, string videoTitle, int videoPositionSec)
	{
	}

    public Platform.ImageData[] GetAvailableImages()
    {
        return null;
    }


}
