
using System;
using UnityEngine;

public class PlatformSupportAndroid : PlatformSupport
{

    #region PRIVATE VARIABLES

    // The Singleton instance of this class.
    public static PlatformSupportAndroid instance = null;


    // The Unity Player Android Java class.
    AndroidJavaClass unityPlayer;


    // The Android Activity for this application.
    AndroidJavaObject androidActivity;

    #endregion

    /*
        public override string GetPreferenceValue(string key)
        {
            //        return "";
            return instance.androidActivity.Call<string>("GetPreferenceValue", key);
        }


        public override void SetPreferenceValue(string key, string value)
        {
            instance.androidActivity.Call("SetPreferenceValue", key, value);
        }
    */




    public PlatformSupportAndroid()
    {
        instance = this;
        unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        androidActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }






    #region PUBLIC FUNCTIONS



    public string GetShortDeviceID()
    {
        if (null == instance)
        {
            return "unknown";
        }
        string retval = SystemInfo.deviceUniqueIdentifier;
        retval = instance.androidActivity.Call<string>("GetDeviceID");
        if (retval.Length > 4)
        {
            retval = retval.Substring(retval.Length - 4);
        }

        return retval;
    }


    public string GetFullDeviceID()
    {
        if (null == instance)
        {
            return "unknown";
        }
        string retval = SystemInfo.deviceUniqueIdentifier;
        retval = instance.androidActivity.Call<string>("GetDeviceID");

        return retval;
    }



    public Platform.MusicData[] GetAvailableMusic()
    {
        string availableMusicJSON = instance.androidActivity.Call<string>("GetMusicDataJSON");
        return Utils.getJsonArray<Platform.MusicData>(availableMusicJSON);
    }

    public Platform.VideoData[] GetAvailableVideos()
    {
        string availableMusicJSON = instance.androidActivity.Call<string>("GetVideoDataJSON");
        if ((null == availableMusicJSON) || (availableMusicJSON.Length == 0))
        {
            return new Platform.VideoData[0];
        }
        return Utils.getJsonArray<Platform.VideoData>(availableMusicJSON);
    }



    public Platform.ImageData[] GetAvailableImages()
    {
        string availableImageJSON = instance.androidActivity.Call<string>("GetImageDataJSON");
        if ((null == availableImageJSON) || (availableImageJSON.Length == 0))
        {
            return new Platform.ImageData[0];
        }
        return Utils.getJsonArray<Platform.ImageData>(availableImageJSON);
    }



    public void BroadcastDeviceState(Platform.DeviceState state, string videoTitle, int videoPositionSec)
    {
        instance.androidActivity.Call("SetDeviceState", (int)state, videoTitle, videoPositionSec);
    }


    void PlatformSupport.CreateVideoThumbnail(string videoFilePath)
    {
        Logger.Log("Getting thumbnail for " + videoFilePath);
        instance.androidActivity.Call("CreateVideoThumbnail", videoFilePath);
    }


    /*
        public Color[] GetVideoThumbnail(string filePath, out int width, out int height)
        {
            int[] pixels = instance.androidActivity.Call<int[]>("GetVideoThumbnailPixels", filePath);

            int w = pixels[0];
            int h = pixels[1];

            Color[] colors = new Color[w * h];

            int i = 0;
            for (int y=h-1; y>=0; --y)
            {
                for (int x=0; x<w; ++x)
                {
                    int pixelIdx = y * w + x + 2;
                    float r = ((float)((pixels[pixelIdx] & (0xff000000)) >> 24)) / 255.0f;
                    float g = ((float)((pixels[pixelIdx] & (0xff0000)) >> 16)) / 255.0f;
                    float b = ((float)((pixels[pixelIdx] & (0xff00)) >> 8)) / 255.0f;
                    float a = ((float)((pixels[pixelIdx] & (0xff)))) / 255.0f;
                    colors[i] = new Color(r, g, b, a);
                    ++i;
                }
            }

            width = w;
            height = h;
            return colors;
        }
    */



    /*
        public bool IsInitialized()
        {
            bool retval = instance.androidActivity.Call<bool>("IsInitialized");
            return retval;
        }
    */


    #endregion
    //---------------------------------------------------------------------------------------------------



}
