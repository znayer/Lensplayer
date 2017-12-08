using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Platform : MonoBehaviour
{
    static PlatformSupport support;


    public Texture2D testThumbnail;


    [System.Serializable]
    public class VideoData
    {
        public string filePath;
        public string durationMS;
        public string title;
    }


    [System.Serializable]
    public class MusicData
    {
        public string filePath;
        public long durationMS;
        public string title;
        public string genres;
    }

    [System.Serializable]
    public class ImageData
    {
        public string filePath;
        public string title;
    }


    static Platform instance;



    // Use this for initialization
    void Start ()
    {
#if UNITY_EDITOR_WIN
        support = new PlatformSupportPC();
#else
#if UNITY_EDITOR_OSX
		support = new PlatformSupportOSX();
#else
#if UNITY_ANDROID
        support = new PlatformSupportAndroid();
#endif
#endif
#endif

        instance = this;
    }


    static public bool IsReady()
    {
        return support != null;
    }


    static public Texture2D GetTestThumbnail()
    {
        return instance.testThumbnail;
    }




    // Update is called once per frame
    void Update ()
    {
		
	}




    static public string GetShortDeviceID()
    {
        return support.GetShortDeviceID();
    }


    static public string GetFullDeviceID()
    {
        return support.GetFullDeviceID();
    }




    static public Platform.MusicData[] GetAvailableMusic()
    {
        return support.GetAvailableMusic();
    }


    static public Platform.ImageData[] GetAvailableImages()
    {
        return support.GetAvailableImages();
    }



    static public Platform.VideoData[] GetAvailableVideos()
    {
        return support.GetAvailableVideos();
    }



    public void SetServerTime(string msg)
    {
        LensPlayer.serverTime = msg;
    }


    public void SetServerTimeDelta(string msg)
    {
        LensPlayer.serverTimeDeltaMS = long.Parse(msg);
        if (LensPlayer.serverTimeDeltaMS == 0)
        {
            LensPlayer.serverTimeDeltaMS = 1;
        }
        Logger.Log(Logger.LogLevel.Debug, "Server time delta: " + LensPlayer.serverTimeDeltaMS);
    }







    public class UDPCommand
    {
        public long id;
        public int type;
        public string device_id;
        public long offset;
    }

    List<long> processedIDs = new List<long>();

    public void OnUDPCommand(string msg)
    {
        Logger.Logd("OnUDPCommand called.");
        UDPCommand command = JsonUtility.FromJson<UDPCommand>(msg);


        switch (command.type)
        {
            case 0:
                Logger.Logd("OnCommandPlayVideo called.");
                OnCommandPlayVideo(command.id, msg);
                break;
            case 3:
                Logger.Logd("OnRequestReset called.");
                OnRequestReset(command.id, command.device_id);
                break;
        }
    }

    public void OnCommandPlayVideo(long messageID, string msg)
    {
        bool found = false;
        foreach (long id in processedIDs)
        {
            if (id == messageID)
            {
                found = true;
                break;
            }
        }
        if (found == true)
        {
            return;
        }
        processedIDs.Add(messageID);

        UDPCommandPlay cmd = JsonUtility.FromJson<UDPCommandPlay>(msg);
        if ((cmd.device_id.Equals("0")) || (cmd.device_id.Equals(Platform.GetFullDeviceID())))
        {
            LensPlayer.udpCommandPlay = cmd;
            Logger.Log(Logger.LogLevel.Debug, "UDP play command received and handled for device_id " + cmd.device_id);
        }
        else
        {
            Logger.Log(Logger.LogLevel.Debug, "UDP play command received and ignored for device_id '" + cmd.device_id + "'");
            Logger.Log(Logger.LogLevel.Debug, "ID for this device: '" + Platform.GetFullDeviceID() + "'");
        }
    }


    void OnRequestReset(long messageID, string device_id)
    {
        bool found = false;
        foreach (long id in processedIDs)
        {
            if (id == messageID)
            {
                found = true;
                break;
            }
        }
        if (found == true)
        {
            return;
        }
        processedIDs.Add(messageID);


        if ((device_id.Equals("0")) || (device_id.Equals(Platform.GetFullDeviceID())))
        {
            LensPlayer.isResetRequested = true;
            Logger.Log(Logger.LogLevel.Debug, "UDP reset command received and handled for device_id " + device_id);
        }
        else
        {
            Logger.Log(Logger.LogLevel.Debug, "UDP reset command received and ignored for device_id '" + device_id + "'");
            Logger.Log(Logger.LogLevel.Debug, "ID for this device: '" + Platform.GetFullDeviceID() + "'");
        }
    }


    Dictionary<string, string> videoThumbnails = new Dictionary<string, string>();

    static public string GetVideoThumbnail(string videoFilePath)
    {
        // If the videopath exists in the distionary, then return the thumbnail path.
        if (instance.videoThumbnails.ContainsKey(videoFilePath))
        {
            return instance.videoThumbnails[videoFilePath];
        }


        // Otherwise, request the thumbnail is generated and return null.
        support.CreateVideoThumbnail(videoFilePath);
        return null;
    }



    public enum DeviceState
    {
        Unknown,
        Idle,
        Countdown,
        Playing
    };

    static public void BroadcastDeviceState(DeviceState state, string videoTitle, int videoPositionSec)
    {
        if (null != support)
        {
            support.BroadcastDeviceState(state, videoTitle, videoPositionSec);
        }
    }




    public void OnThumbnailCreated(string msg)
    {
        // Add the thumbnail to the dictionary.
        string videoFilePath = JsonHelper.GetJsonValue(msg, "videoFilePath");
        string thumbnailFilePath = JsonHelper.GetJsonValue(msg, "thumbnailFilePath");


        videoFilePath = videoFilePath.Replace('\\', '/');
        videoFilePath = videoFilePath.Replace("//", "/");
        thumbnailFilePath = thumbnailFilePath.Replace('\\', '/');
        thumbnailFilePath = thumbnailFilePath.Replace("//", "/");

        videoThumbnails.Add(videoFilePath, thumbnailFilePath);


        // Broadcast ThumbnailCreatedMessageTarget.
        GameObject target = GameObject.Find("Scene Controller");
        ExecuteEvents.Execute<ThumbnailCreatedMessageTarget>(target, null, (x, y) => x.ThumbnailCreated(videoFilePath, thumbnailFilePath));
    }


    public void Log(string msg)
    {
        Logger.Logd("PLUGIN: " + msg);
    }


}
