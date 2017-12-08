
using System;
using UnityEngine;

public interface PlatformSupport
{
    string GetShortDeviceID();
    string GetFullDeviceID();

    Platform.MusicData[] GetAvailableMusic();

    Platform.ImageData[] GetAvailableImages();



    Platform.VideoData[] GetAvailableVideos();


    void CreateVideoThumbnail(string videoFilePath);


    void BroadcastDeviceState(Platform.DeviceState state, string videoTitle, int videoPositionSec);


    /*
        public abstract string GetPreferenceValue(string key);
        public abstract void SetPreferenceValue(string key, string value);

        public abstract Platform.MusicData[] GetAvailableMusic();

        public abstract bool IsInitialized();
    */

}
