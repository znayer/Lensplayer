
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class PlatformSupportPC : PlatformSupport
{



    public string GetShortDeviceID()
    {
        //        return "a920";
        return "PC:" + SystemInfo.deviceUniqueIdentifier;
    }

    public string GetFullDeviceID()
    {
        //        return "a920";
        return "PC:" + SystemInfo.deviceUniqueIdentifier;
    }



    public Platform.MusicData[] GetAvailableMusic()
    {
        DirectoryInfo info = new DirectoryInfo("C:/sdcard/welens");
        FileInfo[] fileInfo = info.GetFiles();
        ArrayList filepaths = new ArrayList();
        foreach (FileInfo file in fileInfo)
        {
            if (file.FullName.ToLower().EndsWith(".mp3"))
            {
                filepaths.Add(file.FullName);
            }
        }
        Platform.MusicData[] retval = new Platform.MusicData[filepaths.Count];
        for (int i = 0; i < filepaths.Count; ++i)
        {
            string fp = (string)filepaths[i];
            retval[i] = new Platform.MusicData();
            retval[i].filePath = fp;
            retval[i].title = fp.Substring(fp.LastIndexOf('\\') + 1);
        }
        return retval;
    }






    public Platform.VideoData[] GetAvailableVideos()
    {
        if (!Directory.Exists("C:/sdcard/welens"))
        {
            return new Platform.VideoData[0];
        }
        DirectoryInfo info = new DirectoryInfo("C:/sdcard/welens");
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



    public void BroadcastDeviceState(Platform.DeviceState state, string videoTitle, int videoPositionSec)
    {
    }



    void PlatformSupport.CreateVideoThumbnail(string videoFilePath)
    {
        Texture2D testTex = Platform.GetTestThumbnail();
        int width = testTex.width;
        int height = testTex.height;
//        OnThumbnailCreated
//        return testTex.GetPixels();
    }

    public Platform.ImageData[] GetAvailableImages()
    {
        DirectoryInfo info = new DirectoryInfo("C:/sdcard/welens/data");
        FileInfo[] fileInfo = info.GetFiles();
        ArrayList filepaths = new ArrayList();
        foreach (FileInfo file in fileInfo)
        {
            if (file.FullName.ToLower().EndsWith(".jpg"))
            {
                filepaths.Add(file.FullName);
            }
        }
        Platform.ImageData[] retval = new Platform.ImageData[filepaths.Count];
        for (int i = 0; i < filepaths.Count; ++i)
        {
            string fp = (string)filepaths[i];
            retval[i] = new Platform.ImageData();
            retval[i].filePath = fp;
            retval[i].title = fp.Substring(fp.LastIndexOf('\\') + 1);
        }
        return retval;
    }

    /*
        public Color[] GetVideoThumbnail(string filePath, out int width, out int height)
        {
            Texture2D testTex = Platform.GetTestThumbnail();
            width = testTex.width;
            height = testTex.height;
            return testTex.GetPixels();
        }
    */




}
