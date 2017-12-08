using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/**
 * This class handles items in the video select scene.
 * 
 **/
public class VideoItem : MonoBehaviour
{

    #region PRIVATE VARIABLE

    // The video data for this item.
    Platform.VideoData videoData;


    #endregion




    #region PUBLIC METHODS

    /**
     * Sets the video data for this item.
     * 
     **/
    public void SetVideoData(Platform.VideoData videoData)
    {
        this.videoData = videoData;
    }


    public Platform.VideoData GetVideoData()
    {
        return videoData;
    }











    /**
     * Callback used when the pointer enters the item.
     * 
     * The display of the item is updated and the rollover sound is played.
     * 
     **/
    public void OnPointerEnter()
    {
        transform.Find("Title").GetComponent<Text>().color = Color.black;
    }










    /**
     * Callback used when the pointer exits the item.
     * 
     * The display of the item is updated.
     * 
     **/
    public void OnPointerExit()
    {
        transform.Find("Title").GetComponent<Text>().color = Color.white;
    }










    /**
     * Callback used when the user clicks this item.
     * 
     * The selected video is played and the button click sound is played.
     * 
     **/
    public void OnClick()
    {
//        if (!LensPlayer.disableControls)
        {
            SceneControllerLensPlayerVideoSelection.PlayVideo(videoData.filePath);
        }
        AppControllerLensPlayer.PlayButtonClickSound();
    }

    #endregion


}
