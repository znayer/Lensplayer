using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * This class defines data, functions, and structures that are globally available (static and public) throughout the application.
 *
 * C0pperk3y
 *
 * 
 **/
public class LensPlayer
{

    public const int udpCommandClientPort = 40004;
    public const int udpClientStatePort = 40005;

    public const int udpServerTimeClientSendPort = 40006;
    public const int udpServerTimeClientReceivePort = 40007;


    //---------------------------------------------------------------------------------------------------
    #region GLOBAL DATA

	// XXX BAL: This should be moved to the specific platforms and accessed via a method.
    // Path to the WeLens directory.
#if UNITY_EDITOR_WIN
    public static string welensPath = "C:\\sdcard\\welens\\";
#else
#if UNITY_EDITOR_OSX
	// /Users/blee/Library/Application Support/WeLens/LensPlayer/sdcard/welens"
	public static string welensPath = Application.persistentDataPath + "/sdcard/welens/";
#else
    public static string welensPath = "/sdcard/welens/";
#endif
#endif

    // Temporary demo flag.
    public static bool demo = false;

    // Performance warning flag.
    public static bool performanceWarning = false;

    // Theater mode flag.
    public static bool theaterMode = false;

    // Auto-Start video flag.
    public static bool videoAutoStart = true;

    // Current video index.
    public static int currentVideoIndex = 0;

    // Spat Volume
    public static float spatVolume = 1.0f;

    // Basic font flag
    public static bool basicFont = false;

    // Developer mode flag.
    public static bool developerMode = false;

	// Is LensApp installed
	public static bool isInstalled = true;

    public static int screeningCountdownSeconds = -1;
    public static int screeningScheduleMinutes = -1;

    public static bool disableControls = false;

    // Custom message.
    public static string custom_message = "";

    public static int autoReset = -1;

    // The time difference between the server's time and the local time.
    public static long serverTimeDeltaMS = 0;

    public static string serverTime = null;

    // The UDP play command recieved.
    public static UDPCommandPlay udpCommandPlay = null;

    // The full path and filename for the current video.
    public static string currentVideoFullPath = "NULL";

    // The filename (without path) for the current video.
    public static string currentVideoFilename = "NULL";

    // The resolution for the current video.
    public static string currentVideoResolution = "NULL";

	// Head Up Mode
	public static int headsUp;
    public static bool displayHeadsUpCountdown;


    public static int headsUpDefault = 30;

	public static bool headsUpSetting = true;
	public static bool headsUpCountdown = false;

    public static string[] videoList = null;

    public static string[] stereoList = null;

	public static string[] formatList = null;

	public static string theaterTitle = null;

    public static string trailerFilename = null;

    public static bool isEventStartRequested = false;

    public static long trailerStartOffsetMS = 0;

    public static long trailerStartRequestTime = 0;

    public static bool isResetRequested = false;

    public static string selectedVideo = "";

    public static PlayerJSONData playerJSONData = null;

    public static serialisedPlan plan = null;

    public static bool isStrictModeEnabled = false;

    public static string configString;

    public static bool isTrialMode = false;  // temporarily hardcoded as false. TODO set back to true after trial mode QA.

    public static string eventID = "NULL";

	public static string theaterTheme = null;

//    public static int videoStartDelaySeconds = -1;

    #endregion
    //---------------------------------------------------------------------------------------------------




    //---------------------------------------------------------------------------------------------------
    #region GLOBAL HELPER FUNCTIONS

    /**
     * Converts a Unix timestamp to a DateTime object.
     *
     **/
    public static System.DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    public static System.DateTime UnixTimeStampToDateTimeSeconds(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    public static long GetServerTimeMS()
    {
        long retval = (long)((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds) - LensPlayer.serverTimeDeltaMS;
        return retval;// - trailerStartOffsetMS;
    }

    public static string GetStoragePath()
    {
        string storagePath;
#if UNITY_EDITOR
        storagePath = "C:";
#else
        storagePath = "";
#endif
        return storagePath;
    }


    public static void AdvanceFrontEnd()
    {
    }



    static public string GetVideoTitle(string filename)
    {
        // Try to find the video title in the video data.
        string vidTitle = "";
        if (LensPlayer.playerJSONData != null)
        {
            Logger.Log("Looking for video title in player JSON data.");
            foreach (PlayerJSONVideoData v in LensPlayer.playerJSONData.videoData)
            {
                if (v.videoFilename.Equals(filename))
                {
                    Logger.Log("\tFound video in player JSON data.");
                    vidTitle = v.title;
                    break;
                }
            }
        }


        // If the video was not found in the player data, find the video in the video data.
        if (vidTitle.Length == 0)
        {
            Logger.Log("\tFailed to find video in player JSON data. Looking for video title in video data.");
            Platform.VideoData[] videoData = AppControllerLensPlayer.GetAvailableVideos();
            Platform.VideoData currentVideoData = null;
            for (int i = 0; i < videoData.Length; ++i)
            {
                string fn = videoData[i].filePath.Substring(videoData[i].filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                if (fn.Equals(filename))
                {
                    currentVideoData = videoData[i];
                    Logger.Log("\tFound video title in video data.");
                    break;
                }
            }
            if (currentVideoData != null)
            {
                vidTitle = currentVideoData.title;
            }
            else
            {
                vidTitle = "";
            }
        }

        return vidTitle;

    }


    #endregion
    //---------------------------------------------------------------------------------------------------
}


[Serializable]
public class PlayerJSONVideoData
{
    public string videoFilename;
    public string thumbnailFilename;
    public string title;
}

[Serializable]
public class PlayerJSONData
{
    public PlayerJSONVideoData[] videoData;
}

[System.Serializable]
public class serialisedPlan
{
    public int status;
    public long expires;
    public long created;
    public DateTime endDate;
}


public class PlanOut
{
    public int status;
    public long expires;
    public long created;
    public string source;
}



//---------------------------------------------------------------------------------------------------
#region GLOBAL DATA STRUCTURES




public class UDPCommandPlay
{
    public long id;
    public int type;
    public string filename;
    public long playtime;
    public string device_id;
}

#endregion
//---------------------------------------------------------------------------------------------------
