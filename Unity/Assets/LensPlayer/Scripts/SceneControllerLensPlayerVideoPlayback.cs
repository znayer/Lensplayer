using monoflow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;



/**
 * This class controls the video playback scene of the LensPlayer app.
 * 
 **/
public class SceneControllerLensPlayerVideoPlayback : MonoBehaviour
{
    #region EDITOR INTERFACE VARIABLES

    [Tooltip("Video player")]
    [SerializeField]
    MPMP mpmp = null;

    [Tooltip("Material used by the video player for output")]
    [SerializeField]
    Material playbackMaterial = null;


    [Header("Playback Controls")]
    [Tooltip("Container for the playback controls")]
    [SerializeField]
    CanvasGroup playbackControlsGroup = null;

    [Tooltip("Play/Pause image")]
    [SerializeField]
    Image playPauseImage = null;

    [Tooltip("Image to use for pause")]
    [SerializeField]
    Sprite pauseSprite = null;

    [Tooltip("Image to use for play")]
    [SerializeField]
    Sprite playSprite = null;

    [Tooltip("Playback timeline")]
    [SerializeField]
    Image timeline = null;

    [SerializeField]
    Slider slider = null;


    [Tooltip("Current time text")]
    [SerializeField]
    Text timmerStart = null;

    [Tooltip("Video length text")]
    [SerializeField]
    Text timmerEnd = null;

    [SerializeField]
    Text errorText;


    public GameObject orientationResetIndicator;
    public GameObject orientationResetButton;


    #endregion




    #region PRIVATE VARIABLES

    bool updateVideoPlayer = false;


    string editorTestVideo = "C:\\sdcard\\welens\\PacMan.mp4";

    // Flag indicating if the orientation resetter should be used.
    bool useOrientationResetter = true;

    // If this value is set, then this video should be played then the app should return to the video select.
    public static string singlePlayFilePath = "";


    // Tracks the current playlist index.
    static int playlistIndex = 0;

    // Spat decoder.
    TBE.TBSpatDecoder Decoder = null;

    // The amount of time left to display the playback controls.
    float playbackControlsDisplayTimeSec = 0;

    string currentVideoFilePath = null;

    string currentVideoTitle = null;
    string currentVideoStereo = null;



    #endregion


    public static int startPositionSeconds = 0;

    #region MONOBEHAVIOUR OVERRIDE METHODS



    /**
     * This method performs scene initialization and starts video playback.
     * 
     **/
    IEnumerator Start()
    {
        Logger.Log("Playback Mode waiting for app controller...");

        // Make sure the playback controls are not visible.
        playbackControlsGroup.gameObject.SetActive(false);


        // Wait for the platform to be ready before doing anything else.
        while (!Platform.IsReady())
        {
            yield return new WaitForEndOfFrame();
        }
        while (!AppControllerLensPlayer.IsReady())
        {
            yield return new WaitForEndOfFrame();
        }


        // Allow scene components to start up.
        yield return new WaitForEndOfFrame();


        Logger.Log("****************************************");
        Logger.Log("Starting Video Playback.");



        // Set the watermark state.
        AppControllerLensPlayer.SetWatermarkVisible(LensPlayer.isTrialMode);
        

        // Start fading out the scene fader.
        SceneFader.FadeOut();


        // Set timed gaze input as active.
        GazeBasedInteractionControl.SetTimedInputActive(true);

        
        // Set the orientation resetter as not active.  It will be set as active when the controls appear.
        OrientationResetter.SetActive(false);


        // Check the headphone jack.
        AppControllerLensPlayer.CheckHeadphoneJack();





        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        HandleAdvanceVideo(true);


        Platform.BroadcastDeviceState(Platform.DeviceState.Playing, currentVideoTitle, 888);
    }








    float lastForcePlaybackInputTime = 0;

    DateTime userLeftTime;
    double videoPositionWhenUserLeft = 0;

    /*
        IEnumerator DelayedRestart()
        {
            if (OVRPlugin.userPresent && !mpmp.IsPlaying())
            {
                mpmp.Play();
                yield return new WaitForSeconds(5);
                mpmp.SeekTo((float)videoPositionWhenUserLeft);
            }
        }
    */

    IEnumerator DelayedRestart(float userLeftAtSeconds)
    {
        if (OVRPlugin.userPresent && !mpmp.IsPlaying())
        {
            float goneTime = (float)((DateTime.Now - userLeftTime).TotalSeconds);
            if ((LensPlayer.autoReset != -1) && (goneTime > LensPlayer.autoReset))
            {
                HandleAdvanceScene();
            }
            else
            {
                mpmp.Play();
                //            yield return new WaitForSeconds(5);
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                float seekTime;
                if (LensPlayer.autoReset != -1)
                {
                    seekTime = (float)userLeftAtSeconds + goneTime;
                }
                else
                {
                    seekTime = (float)userLeftAtSeconds;
                }
                mpmp.SeekTo(seekTime);
                Logger.Logd("user left at " + userLeftAtSeconds);
                Logger.Logd("gone for " + goneTime);
                Logger.Logd("seeking to " + ((float)userLeftAtSeconds + goneTime));
            }
        }
    }


    /**
     * This method performs tasks each frame necessary for video playback.
     * 
     **/
    void Update()
    {
        // If in the editor, mute the video volume.
#if UNITY_EDITOR
        if (mpmp.IsPlaying())
        {
            mpmp.volume = 0.0f;
        }
#endif


#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            LensPlayer.isResetRequested = true;
        }
#endif


        if ((!OVRPlugin.userPresent) && (videoPositionWhenUserLeft == 0))
        {
            videoPositionWhenUserLeft = mpmp.GetCurrentPosition();
            mpmp.Pause();
            userLeftTime = DateTime.Now;

        }
        else if ((OVRPlugin.userPresent) && (videoPositionWhenUserLeft != 0))
        {
            float f = (float)videoPositionWhenUserLeft;
            StartCoroutine(DelayedRestart(f));
            videoPositionWhenUserLeft = 0;
            return;
        }
        


        // If the next video isn't loading, or if the scene isn't fading to the next scene, then check for
        // input requesting the video is advanced.
        if ((!mpmp.IsLoading()) && (!SceneLoader.IsLoading()))
        {
            if (Input.GetButtonDown("Cancel"))
//            if (Input.GetKeyDown(KeyCode.Escape))
            {


                if ((Time.time - lastForcePlaybackInputTime < 2.0f) || (mpmp.IsPaused()))
                {
                    // If this is a single play, then handle.
                    if (singlePlayFilePath.Length != 0)
                    {
                        // If the video is already paused, then advance the video.
                        if (mpmp.IsPaused() || LensPlayer.disableControls)
                        {
                            HandleAdvanceVideo();
                        }
                    }

                    // Otherwise, advance the video.
                    else
                    {
                        HandleAdvanceVideo();
                    }
                }
                lastForcePlaybackInputTime = Time.time;
            }
        }


        // If this is a single play, then update the controls state.
        if (singlePlayFilePath.Length != 0)
        {
            UpdateVideoControls();
        }


        // If spatial audio is playing, then handle.
        if ((null != Decoder) && (Decoder.playState == TBE.TBPlayState.TB_STATE_PLAYING))
        {
            double videoTimeMS = mpmp.GetCurrentPosition() * 1000.0;
            Decoder.setExternalClockInMs((float)videoTimeMS);
            if (mpmp.IsPlaying())
            {
                mpmp.volume = 0.0f;
            }
        }
        else
        {
            if (mpmp.IsPlaying())
            {
                mpmp.volume = 1.0f;
            }
        }



        // If loading the next scene and the scene fader has faded out, then activate the next scene.
        if (SceneLoader.IsSceneLoaded() && !SceneFader.IsFading())
        {
            SceneLoader.ActivateLoadedScene();
        }



        if (LensPlayer.isResetRequested)
        {
            LensPlayer.isResetRequested = false;
            HandleAdvanceScene();
        }

        Platform.BroadcastDeviceState(Platform.DeviceState.Playing, currentVideoTitle, (int)(mpmp.GetCurrentPosition()));
    }



    public void HandleBackInput()
    {
        if (playlistIndex == 0)
        {
            HandleAdvanceScene();
            return;
        }


        --playlistIndex;


        currentVideoFilePath = LensPlayer.welensPath + LensPlayer.videoList[playlistIndex];
        currentVideoTitle = LensPlayer.GetVideoTitle(LensPlayer.videoList[playlistIndex]);
        if ((null == currentVideoTitle) || (currentVideoTitle.Length == 0))
        {
            currentVideoTitle = currentVideoFilePath.Substring(currentVideoFilePath.LastIndexOf('/') + 1);
        }
    }

    public void HandleForwardInput()
    {
        HandleAdvanceVideo();
    }


#endregion




#region PUBLIC METHODS



    /**
     * Used to set the single play video.
     * 
     **/
    static public void SetVideoFilePath(string videoFilePath)
    {
        singlePlayFilePath = videoFilePath;
    }










    /**
     * Used to reset the playlist playback index.
     * 
     * This is done by the theater view before starting playback of the first video.
     * 
     **/
    static public void SetPlaylistPlayback(int idx)
    {
        playlistIndex = idx;
    }









    /**
     * OnClick callback for the play/pause button.
     * 
     * The video player is set to play or pause accordingly, and the button image is updated.
     * 
     **/
    public void PlayPause()
    {
        if (mpmp.IsPlaying())
        {
            mpmp.Pause();
            playPauseImage.sprite = pauseSprite;
        }
        else
        {
            mpmp.Play();
            playPauseImage.sprite = playSprite;
        }
    }










    /**
     * OnClick callback for the fast forward and rewind buttons.
     * 
     **/
    public void Seek(float ammount)
    {
        // Reset the playback controls display time.
        playbackControlsDisplayTimeSec = 2.5f;


        // Seek.
        float pos = ((float)mpmp.GetCurrentPosition()) + ammount;
        if (pos < 0)
        {
            pos = 0;
        }
        mpmp.SeekTo(pos);
    }



    public void OnSeekbarClicked()
    {
        updateVideoPlayer = true;
    }





    /**
     * OnClick callback for seek via the seek bar.
     * 
     **/
    public void SeekTime(float ammount)
    {
        // Reset the playback controls display time.
        playbackControlsDisplayTimeSec = 2.5f;


        // Seek
        float pos = ((float)mpmp.GetDuration() * ammount);
        Logger.Log("Seeking to " + pos);
        mpmp.SeekTo(pos);
    }

#endregion




#region PRIVATE METHODS


    // XXX BAL: commented out for now. if you start a video and quickly advance to the next video this will fire off wrong.
    IEnumerator CheckPlayback()
    {
        yield return new WaitForSeconds(1);
        if (!mpmp.IsPlaying() && (!mpmp.IsPaused()))
        {
//            StartCoroutine(PlayFailed());
        }
    }


    /**
     * Starts the playback of the specified video.
     * 
     **/
    bool playCond;
    bool stopCond = true;
    void MPMPPlayVideo(string fn)
    {
        StartCoroutine(CheckPlayback());
        Logger.Log(Logger.LogLevel.Debug, "Loading video " + fn);
        mpmp.videoPath = fn;
        mpmp.autoPlay = false;
        mpmp.looping = false;
        mpmp.Load();



        mpmp.OnLoaded = (a) =>
        {
            mpmp.Play();
            Logger.Logd("Playing video " + fn);
            PlaybackEventManager.RecordEvent("video-start", fn);
            playCond = false;
            mpmp.volume = 1.0f;
        };

        mpmp.OnError = (a) =>
        {
            StartCoroutine(PlayFailed());
        };

        mpmp.OnPlaybackCompleted = (a) =>
        {
            stopCond = false;
            PlaybackEventManager.RecordEvent("video-complete", fn);
            HandleAdvanceVideo();
        };

        mpmp.OnPlay = (a) =>
        {
            if (playCond)
            {
                PlaybackEventManager.RecordEvent("video-playing", fn);
            }
            float s = startPositionSeconds;
            mpmp.SeekTo(s);
            string msg = string.Format("Seeking to {0:F2} sec", s);
            Logger.Log(msg);
            startPositionSeconds = 0;
        };

        mpmp.OnStop = (a) =>
        {
            if (stopCond)
            {
                playCond = true;
            }
        };
    }


    IEnumerator PlayFailed()
    {
        Logger.Log(Logger.LogLevel.Error, "Failed to load video.");

        string fn = Utils.FilePathToFilename(currentVideoFilePath);
        errorText.text = "Failed to load video " + fn + ".";
        errorText.gameObject.SetActive(true);
        yield return new WaitForSeconds(4);
        HandleAdvanceScene();
    }




    void HandleAdvanceScene()
    {
        if (SceneLoader.IsLoading())
        {
            return;
        }
        LensPlayer.udpCommandPlay = null;
		if ((LensPlayer.theaterMode == false) || (null == singlePlayFilePath))
		{
			SceneLoader.Load("1a. Video Selection", false);
		}
		else if (LensPlayer.theaterTheme == "rainforest")
		{
			SceneLoader.Load("1c. Waiting Room Rainforest", false);
		}
		else
		{
			SceneLoader.Load("1b. Waiting Room", false);
		}
    }





    /**
     * Called when the current video needs to be advanced.
     * 
     * If the playback is a single play, then the video selection scene is loaded. if the last video in the video list has played, then
     * the waiting room is displayed. Otherwise, the next video is played.
     * 
     **/
    void HandleAdvanceVideo(bool isIntialRun = false)
    {
        if (mpmp.IsPlaying() || mpmp.IsPaused())
        {
            PlaybackEventManager.RecordEvent("video-stop", mpmp.videoPath);
        }
        lastForcePlaybackInputTime = 0;
        Decoder = GetComponent<TBE.TBSpatDecoder>();
        if (Decoder.playState == TBE.TBPlayState.TB_STATE_PLAYING)
        {
            Decoder.stop();
        }


        playbackControlsGroup.gameObject.SetActive(false);
        GazeInput.SetPointerActive(false);

        /*
                if ((LensPlayer.udpCommandPlay != null) && (!isIntialRun))
                {
                    HandleAdvanceScene();
                    return;
                }
        */


        // If testing in the editor, make sure a video is set.
#if UNITY_EDITOR
        if ((null == singlePlayFilePath) && (null == LensPlayer.videoList))
        {
            singlePlayFilePath = editorTestVideo;
        }
#endif

        // If the single play file path is set then either play the video or advance the scene.
        if (singlePlayFilePath.Length != 0)
        {
            if (!isIntialRun)
            {
                HandleAdvanceScene();
                return;
            }
            else
            {
                currentVideoFilePath = singlePlayFilePath;
                currentVideoTitle = singlePlayFilePath.Substring(singlePlayFilePath.LastIndexOf('/') + 1);
            }
        }
        else
        {
            if (!isIntialRun)
            {
                ++playlistIndex;
            }

            if (null == LensPlayer.videoList)
            {
                Logger.Log(Logger.LogLevel.Error, "Playback with no videos.");
                return;
            }
            else if (playlistIndex >= LensPlayer.videoList.Length)
            {
                HandleAdvanceScene();
                return;
            }
            else
            {
                currentVideoFilePath = LensPlayer.welensPath + LensPlayer.videoList[playlistIndex];

                if ((LensPlayer.stereoList != null) && (LensPlayer.stereoList.Length > playlistIndex))
                {
                    currentVideoStereo = LensPlayer.stereoList[playlistIndex];
                }

                currentVideoTitle = LensPlayer.GetVideoTitle(LensPlayer.videoList[playlistIndex]);
                if ((null == currentVideoTitle) || (currentVideoTitle.Length == 0))
                {
                    currentVideoTitle = currentVideoFilePath.Substring(currentVideoFilePath.LastIndexOf('/') + 1);
                }
            }
        }


        // If the video does not exist, then display the error message.
        if (!File.Exists(currentVideoFilePath))
        {
            StartCoroutine(PlayFailed());
            return;
        }


        // Play the video.
        MPMPPlayVideo(currentVideoFilePath);


        // If a spatial audio file is found, play spatial audio.
        string tbeFilename = currentVideoFilePath.Substring(0, currentVideoFilePath.LastIndexOf('.')) + ".tbe";
        if (System.IO.File.Exists(tbeFilename))
        {
            Decoder.loadAssetFromPath(tbeFilename);
            Decoder.volume = .75f;
            Decoder.syncMode = TBE.TBSyncMode.TB_SYNC_EXTERNAL;
            Decoder.play();
            Logger.Log("Playing spatial audio.");
        }


        if ((LensPlayer.stereoList != null) && (currentVideoStereo != null))
        {
            // We will read stereo in here instead
            // TODO: Add a way to read formats and add that here
            string cvsfn = currentVideoStereo.ToLower();
            if (cvsfn == "tb" || cvsfn == "3dv")
            {
                VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.stereoVerical);
                VideoRenderer.SetStereoVerticalMaterial(playbackMaterial);
            }
            else if (cvsfn == "lr" || cvsfn == "3dh")
            {
                VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.stereoHorizontal);
                VideoRenderer.SetStereoHorizontalMaterial(playbackMaterial);
            }
            else if (cvsfn == "_180x180_3dv.mp4" || cvsfn == "_180x160_3dv.mp4")
            {
                VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.stereoVertical180);
                VideoRenderer.SetStereoVerticalMaterial(playbackMaterial);
            }
            else if (cvsfn == "_180x180_3dh.mp4" || cvsfn == "_180x160_3dh.mp4" || cvsfn == "widescreen")
            {
                VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.stereoHorizontal180);
                VideoRenderer.SetStereoHorizontalMaterial(playbackMaterial);
            }
            else
            {
                VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.mono);
                VideoRenderer.SetMonoMaterial(playbackMaterial);
            }
        }
        else
        {
            // Set the appropriate renderer as active.
            string cmpfn = currentVideoFilePath.ToLower();
            if (cmpfn.EndsWith("_tb.mp4") || cmpfn.EndsWith("_3dv.mp4"))
            {
                VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.stereoVerical);
                VideoRenderer.SetStereoVerticalMaterial(playbackMaterial);
            }
            else if (cmpfn.EndsWith("_lr.mp4") || cmpfn.EndsWith("_3dh.mp4"))
            {
                VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.stereoHorizontal);
                VideoRenderer.SetStereoHorizontalMaterial(playbackMaterial);
            }
            else if (cmpfn.EndsWith("_180x180_3dv.mp4") || cmpfn.EndsWith("_180x160_3dv.mp4"))
            {
                VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.stereoVertical180);
                VideoRenderer.SetStereoVerticalMaterial(playbackMaterial);
            }
            else if (cmpfn.EndsWith("_180x180_3dh.mp4") || cmpfn.EndsWith("_180x160_3dh.mp4"))
            {
                VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.stereoHorizontal180);
                VideoRenderer.SetStereoHorizontalMaterial(playbackMaterial);
            }
            else
            {
                VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.mono);
                VideoRenderer.SetMonoMaterial(playbackMaterial);
            }
        }
    }








    /**
     * Updates the video control display.
     * 
     **/
    void UpdateVideoControls()
    {
        RealtimeFeedback.SetFeedback("time: " + playbackControlsDisplayTimeSec);

        if ((!mpmp.IsLoading()) && (!SceneLoader.IsLoading()))
        {
            if ((!LensPlayer.disableControls) && (Input.GetButtonDown("Cancel")))
//                if (Input.GetKeyDown(KeyCode.Escape))
            {
                //If video is paused, exit to previous scene / reload current depending on config file
                mpmp.Pause();
                playPauseImage.sprite = pauseSprite;
            }

            if (isResettingOrientation)
            {
                playbackControlsDisplayTimeSec = 2.5f;
            }

            // If any key is pressed, then display the playback controls.
            if ((!LensPlayer.disableControls) && (Input.anyKey || GazeBasedInteractionControl.HoveredButton))
            {
                playbackControlsDisplayTimeSec = 2.5f;
                playbackControlsGroup.gameObject.SetActive(true);
                GazeInput.SetPointerActive(true);

                if (useOrientationResetter)
                {
                    if (!OrientationResetter.IsActive())
                    {
                        OrientationResetter.SetActive(true);
                    }
                }
            }
        }


        // Otherwise, if the controls are displayed, then decrease the life of the controls.
        if (playbackControlsDisplayTimeSec > 0)
        {
            playbackControlsDisplayTimeSec -= Time.deltaTime;
        }


        // Fade the playback controls group.
        playbackControlsGroup.alpha = Mathf.Lerp(playbackControlsGroup.alpha, playbackControlsDisplayTimeSec > 0 ? 1 : 0, Time.deltaTime * 10);


        // If the playback controls group is not very visible (either fading in or out), then set the controls group as inactive, and the gaze input as inactive.
        if (playbackControlsGroup.alpha < 0.05f)
        {
            playbackControlsGroup.gameObject.SetActive(false);
            GazeInput.SetPointerActive(false);
        }


        // If fading out and the head is more than 90 degrees from center, don't fade out
        float angleY = Head.Trans().eulerAngles.y;
        if (angleY > 180)
        {
            angleY -= 360;
        }
        if ((playbackControlsGroup.alpha < 1.0f) && (playbackControlsDisplayTimeSec < 1.0f) && (Mathf.Abs(angleY) > 90.0f) )
        {
            playbackControlsGroup.alpha = 1.0f;
        }


        // If the playback controls group is fading out, then set the orientation resetter as inactive.
        if (useOrientationResetter)
        {
            if (OrientationResetter.IsActive() && (playbackControlsGroup.alpha < 0.5f) && (playbackControlsDisplayTimeSec < 1.0f))
            {
                OrientationResetter.SetActive(false);
            }
        }



        if (updateVideoPlayer)
        {
            float pos = ((float)mpmp.GetDuration()) * slider.value;
            mpmp.SeekTo(pos);
            updateVideoPlayer = false;
        }
        else
        {
            slider.value = (float)( mpmp.GetCurrentPosition() / mpmp.GetDuration());
        }



        // Update the timer end text.
        TimeSpan ts;
        if (timmerEnd.text == "00:00" || timmerEnd.text == "0:0")
        {
            ts = System.TimeSpan.FromSeconds(mpmp.GetDuration());
            timmerEnd.text = (ts.Minutes < 10 ? "0" : "") + ts.Minutes.ToString() + ":" + (ts.Seconds < 10 ? "0" : "") + ts.Seconds.ToString();
        }


        // Update the timer start text.
        ts = System.TimeSpan.FromSeconds(mpmp.GetCurrentPosition());
        timmerStart.text = (ts.Minutes < 10 ? "0" : "") + ts.Minutes.ToString() + ":" + (ts.Seconds < 10 ? "0" : "") + ts.Seconds.ToString();
    }



    bool isResettingOrientation = false;

    GameObject resetButtonParent;

    public void OnRecenterButtonDown()
    {
        resetButtonParent = orientationResetButton.transform.parent.gameObject;
        orientationResetButton.transform.SetParent(orientationResetIndicator.transform);
        isResettingOrientation = true;
    }


    public void OnRecenterButtonUp()
    {
        orientationResetButton.transform.SetParent(resetButtonParent.transform);
        orientationResetButton.transform.localPosition = Vector3.zero;
        orientationResetButton.transform.localRotation = Quaternion.identity;
        isResettingOrientation = false;
        UnityEngine.VR.InputTracking.Recenter();
    }

#endregion


}