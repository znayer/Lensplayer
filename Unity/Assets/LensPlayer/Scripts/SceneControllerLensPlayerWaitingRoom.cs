using monoflow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


/**
 * Scene controller for the waitine room scene.
 * 
 **/
public class SceneControllerLensPlayerWaitingRoom : MonoBehaviour, ThumbnailCreatedMessageTarget
{

    #region EDITOR INTERFACE VARIABLES


    [Tooltip("Video player")]
    [SerializeField]
    MPMP trailerVideoPlayer = null;

    [Tooltip("Video player")]
    [SerializeField]
    Text customMessage = null;

    [Tooltip("Video player")]
    [SerializeField]
    Text title = null;

    [Tooltip("Video player")]
    [SerializeField]
    Text nextScreeningTitle = null;

    [Tooltip("Video player")]
    [SerializeField]
    Text nextScreeningValue = null;

    [Tooltip("Video player")]
    [SerializeField]
    Text nextScreeningSchedule = null;

    [Tooltip("Video player")]
    [SerializeField]
    GameObject messageBackground = null;

    [Tooltip("Video player")]
    [SerializeField]
    CanvasGroup theaterModeControlsGroup = null;

	[Tooltip("Video player")]
	[SerializeField]
	CanvasGroup theaterModeTimerGroup = null;

	[Tooltip("Video player")]
    [SerializeField]
    GameObject trailerDisplay = null;

    [Tooltip("Video player")]
    [SerializeField]
    Image videoThumbnail = null;

	[Tooltip("Animated Texture")]
	[SerializeField]
	AnimatedUVTexture opener = null;

	[Tooltip("Animated Texture")]
	[SerializeField]
	AnimatedUVTexture headsUpOpener = null;

	[Tooltip("Video player")]
	[SerializeField]
	Text headsUpTimer = null;

	[Tooltip("Video player")]
	[SerializeField]
	Image headsUpBackground = null;

    [Tooltip("Video player")]
    [SerializeField]
    Text lookUpText = null;

	[Tooltip("Pagination Group")]
	[SerializeField]
	RectTransform paginationGroup = null;

	[SerializeField]
    GameObject infoBackground = null;

    #endregion




    #region PRIVATE VARIABLES

    // Flag indicating video playback has occured.
    static bool hasPlayed = false;

	GameObject pagedRect;

	bool playbackStart = false;

    // Playback countdown value.
    int countdownTimer = 0;

    float lastForcePlaybackInputTime = 0;

    bool hasHandledUDPCommand;

    int vidIndex = 0;

    //font that all text onscreen will be set to if user specifies so
    string basicFontName = "Arial.ttf";
    bool canPlay = true;
	bool hasAnimated = false;

	Transform head;
	bool lookUp = false;
	float lookUpAngle = 35;
	float huCounter;

	#endregion




	#region MONOBEHAVIOUR OVERRIDE METHODS


	/**
     * Performs startup tasks.
     * 
     **/
	IEnumerator Start()
    {

        Logger.Log("Theater Mode waiting for app controller...");

        hasHandledUDPCommand = false;

        // Wait for the app controller to be ready.
        while (!AppControllerLensPlayer.IsReady())
        {
            yield return new WaitForEndOfFrame();
        }

        Logger.Log("****************************************");
        Logger.Log("Starting Theater Mode.");


        Platform.BroadcastDeviceState(Platform.DeviceState.Idle, "", 0);



        // Set the watermark visible accordingly.
        Logger.Log("Setting watermark as " + (LensPlayer.isTrialMode ? "" : "not ") + "visible");
        AppControllerLensPlayer.SetWatermarkVisible(LensPlayer.isTrialMode);

        // Fade out the scene fader.
        SceneFader.FadeOut();

		// Start Heads Up Counter
		headsUpTimer.gameObject.SetActive(false);
		headsUpBackground.gameObject.SetActive(false);
		lookUpText.gameObject.SetActive(false);

#if !UNITY_EDITOR
        if (OVRPlugin.userPresent)
#endif
        {
            // BAL: The following line was added to force headsup to work again upon any restart of the theater view. If the following line is commented out
            // then headsup will only restart after the headset has been removed.
            LensPlayer.headsUpSetting = true;

            // Start the headsup counter.
            StartCoroutine(HeadsUpCounter());
		}
		head = Head.Trans();
		headsUpTimer.text = "00:03";

		// Set the mono renderer visible.
		VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.none);


        // If there are no videos in the video list, then 
        if ((LensPlayer.videoList == null) || (LensPlayer.videoList.Length == 0))
        {
            Logger.Log("No videos in video list.");
            title.text = "No videos in video list";
            adjustTitleFont();
            customMessage.gameObject.SetActive(false);
            nextScreeningTitle.gameObject.SetActive(false);
            nextScreeningValue.gameObject.SetActive(false);
            nextScreeningSchedule.gameObject.SetActive(false);
            messageBackground.SetActive(false);
            canPlay = false;
            yield break;
        }


        // Make sure the first video in the video list exists.
        if (!CheckVideoExists(LensPlayer.welensPath + LensPlayer.videoList[vidIndex]))
        {
            canPlay = false;
            yield break;
        }


        if ((LensPlayer.isStrictModeEnabled) && (LensPlayer.screeningScheduleMinutes != -1))
        {
            // Compute the current playback position.
            DateTime t = DateTime.Now;
            int currentHourSecond = t.Minute * 60 + t.Second;
            int screeningScheduleSeconds = LensPlayer.screeningScheduleMinutes * 60;
            int currentPlaybackPositionSeconds = currentHourSecond % screeningScheduleSeconds;

            // Find the currently playing video.
            Platform.VideoData currentlyPlayingVideo = null;
            Platform.VideoData[] videoData = Platform.GetAvailableVideos();
            for (int i = 0; i < LensPlayer.videoList.Length; ++i)
            {
                // Find the search video in the video data.
                Platform.VideoData currentVideoData = null;
                for (int j = 0; j < videoData.Length; ++j)
                {
                    String fn = videoData[j].filePath.Substring(videoData[j].filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    if (fn.Equals(LensPlayer.videoList[i]))
                    {
                        currentVideoData = videoData[j];
#if UNITY_EDITOR
                        switch (i)
                        {
                            case 0:
                                currentVideoData.durationMS = "" + (4 * 1000);
                                break;
                            case 1:
                                currentVideoData.durationMS = "" + (13 * 1000);
                                break;
                            case 2:
                                currentVideoData.durationMS = "" + (6 * 1000);
                                break;
                        }
#endif
                        break;
                    }
                }
                if (null == currentVideoData)
                {
                    string msg = "Unknown video in video list: " + LensPlayer.videoList[i];
                    Logger.Log("No videos in video list.");
                    title.text = "No videos in video list";
                    customMessage.gameObject.SetActive(false);
                    nextScreeningTitle.gameObject.SetActive(false);
                    nextScreeningValue.gameObject.SetActive(false);
                    nextScreeningSchedule.gameObject.SetActive(false);
                    messageBackground.SetActive(false);
                    canPlay = false;
                    yield break;
                }

                // If the duration is less than the current playback posiition, then this is the current video.
                int durationSec = int.Parse(currentVideoData.durationMS) / 1000;
                if (durationSec > currentPlaybackPositionSeconds)
                {
                    currentlyPlayingVideo = currentVideoData;
                    vidIndex = i;
                    break;
                }

                // Otherwise, subtract the duration from the current playback position.
                else
                {
                    currentPlaybackPositionSeconds -= durationSec;
                }
            }


            // If a video was not found, then no video is currently playing. Display the next playback info.
            if (null == currentlyPlayingVideo)
            {
                Logger.Log("Strict mode, between playbacks.");
            }

            // Otherwise, start the current video at the appropriate position.
            else
            {
                Logger.Log("Strict mode, playback occuring. ");
                SceneControllerLensPlayerVideoPlayback.startPositionSeconds = currentPlaybackPositionSeconds;
                customMessage.gameObject.SetActive(false);
                nextScreeningTitle.gameObject.SetActive(false);
                nextScreeningValue.gameObject.SetActive(false);
                nextScreeningSchedule.gameObject.SetActive(false);
                messageBackground.SetActive(false);
                title.text = "";
                StartVideo();
                yield break;
            }
        }


        // Initialize the trailer video.
        InitTrailer();


        // Initialize the video thumbnail.
        InitThumbnail();


        // Init the custom message.
        customMessage.text = LensPlayer.custom_message;


        string vidTitle = LensPlayer.GetVideoTitle(LensPlayer.videoList[vidIndex]);
		string theaterTitle = LensPlayer.theaterTitle;

		if (theaterTitle != null)
		{
			Logger.Log("\tSetting video title to: " + theaterTitle);
			title.text = theaterTitle;
		}
		else
		{
        Logger.Log("\tSetting video title to: " + vidTitle);
        title.text = vidTitle;
		}


        // Check the headphone jacke.
        AppControllerLensPlayer.CheckHeadphoneJack();

        
        // If there is a screening schedule, then set the display accordingly.
        if (LensPlayer.screeningScheduleMinutes != -1)
        {
            Logger.Log("Screening schedule set.");
            DateTime t = DateTime.Now;
            int currentHourSecond = t.Minute * 60 + t.Second;
            int screeningScheduleSeconds = LensPlayer.screeningScheduleMinutes * 60;
            countdownTimer = screeningScheduleSeconds - (currentHourSecond % screeningScheduleSeconds);
            t = t.AddSeconds(countdownTimer);
            string format = "h:mm";
            nextScreeningSchedule.text = t.ToString(format);
            StartCoroutine(UpdateCountdown());
        }

        else if ((LensPlayer.headsUpCountdown) && (LensPlayer.screeningCountdownSeconds == -1))
        {
            nextScreeningTitle.gameObject.SetActive(true);
            nextScreeningValue.gameObject.SetActive(true);
            nextScreeningSchedule.gameObject.SetActive(false);
            nextScreeningTitle.gameObject.SetActive(false);
        }

        // Otherwise, if it has already played, then display no "next screening" information.
        else if ((hasPlayed) && (LensPlayer.screeningCountdownSeconds == -1))
        {
            Logger.Log("Has played. Not displaying screening info.");
            nextScreeningTitle.gameObject.SetActive(false);
            nextScreeningValue.gameObject.SetActive(false);
            nextScreeningSchedule.gameObject.SetActive(false);
        }

        // Otherwise, if there is a screening countdown defined, then set the display accordingly.
        else if ((LensPlayer.screeningCountdownSeconds != -1) && (!hasPlayed || (LensPlayer.autoReset != -1)))
        {
            Logger.Log("Screening countdown set.");
            nextScreeningSchedule.gameObject.SetActive(false);
            countdownTimer = LensPlayer.screeningCountdownSeconds;
            StartCoroutine(UpdateCountdown());
        }


        // Otherwise, display no "next screening" information.
        else
        {
            Logger.Log("No screening info defined.");
            nextScreeningTitle.gameObject.SetActive(false);
            nextScreeningValue.gameObject.SetActive(false);
            nextScreeningSchedule.gameObject.SetActive(false);
        }
    }



    bool CheckVideoExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            string fn = Utils.FilePathToFilename(filePath);
            title.text = "Could not find video " + fn + ".";
            adjustTitleFont();
            customMessage.gameObject.SetActive(false);
            nextScreeningTitle.gameObject.SetActive(false);
            nextScreeningValue.gameObject.SetActive(false);
            nextScreeningSchedule.gameObject.SetActive(false);
            messageBackground.SetActive(false);
            return false;
        }

        return true;
    }

    IEnumerator StartCountdown()
    {
        bool startCountdown = false;
        if (countdownTimer == 0)
        {
            startCountdown = true;
        }


        // Wait 
        long serverTimeMS = LensPlayer.GetServerTimeMS();
        yield return new WaitForSeconds((1000 - (serverTimeMS % 1000)) / 1000.0f);

        serverTimeMS = LensPlayer.GetServerTimeMS();
        countdownTimer = (int)((LensPlayer.udpCommandPlay.playtime - serverTimeMS) / 1000);
        if (startCountdown)
        {
            StartCoroutine(UpdateCountdown());
        }
    }



    /**
     * Performs update tasks.
     * 
     **/
    void Update()
    {
        if (false == canPlay)
        {
            return;
        }

#if !UNITY_EDITOR
        if (!OVRPlugin.userPresent)
#else
        if (Input.GetKeyDown(KeyCode.F))
#endif
        {
            if (!SceneLoader.IsLoading())
            {
                if (LensPlayer.theaterTheme == "rainforest")
                {
                    SceneLoader.Load("1c. Waiting Room Rainforest");
                }
                else
                {
                    SceneLoader.Load("1b. Waiting Room");
                }
            }
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            LensPlayer.udpCommandPlay = new UDPCommandPlay();
            LensPlayer.udpCommandPlay.filename = "";
            LensPlayer.udpCommandPlay.playtime = LensPlayer.GetServerTimeMS() + (10 * 1000);
        }
#endif



        if ((LensPlayer.udpCommandPlay != null) && (!hasHandledUDPCommand))
        {

            if (lookUp)
            {
                theaterModeControlsGroup.gameObject.SetActive(true);
                lookUpText.gameObject.SetActive(false);
                headsUpTimer.gameObject.SetActive(false);
                headsUpBackground.gameObject.SetActive(false);
                lookUp = false;
            }

            hasHandledUDPCommand = true;
            Logger.Logd("UDP command detected. " + LensPlayer.udpCommandPlay.playtime);

            long serverTimeMS = LensPlayer.GetServerTimeMS();


            if (LensPlayer.udpCommandPlay.playtime <= serverTimeMS)
            {
				StartCoroutine(PlayAnimation());
            }
            else
            {
                nextScreeningSchedule.gameObject.SetActive(false);
                nextScreeningTitle.gameObject.SetActive(true);
                nextScreeningValue.gameObject.SetActive(true);
                bool startCountdown = false;
                if (countdownTimer == 0)
                {
                    startCountdown = true;
                }
                countdownTimer = (int)((LensPlayer.udpCommandPlay.playtime - serverTimeMS) / 1000);
                if (startCountdown)
                {
                    StartCoroutine(UpdateCountdown());
                }
            }

			/*
                        // Find the filename in the video list. If the video list does not exist, then create one and add the video.
                        // If the video is not found, then add it.
                        int idx = 0;
                        bool found = false;
                        if (null != LensPlayer.videoList)
                        {
                            for (int i = 0; i < LensPlayer.videoList.Length; ++i)
                            {
                                if (LensPlayer.udpCommandPlay.filename.Equals(LensPlayer.videoList[i]))
                                {
                                    idx = i;
                                    found = true;
                                    break;
                                }

                            }
                        }
                        if (!found)
                        {
                            int l;
                            string[] newVideoList;
                            if (null == LensPlayer.videoList)
                            {
                                l = 1;
                                newVideoList = new string[l];
                            }
                            else
                            {
                                l = LensPlayer.videoList.Length + 1;
                                newVideoList = new string[l];
                                for (int i = 0; i < LensPlayer.videoList.Length; ++i)
                                {
                                    newVideoList[i] = LensPlayer.videoList[i];
                                }
                            }

                            idx = newVideoList.Length - 1;
                            newVideoList[idx] = LensPlayer.udpCommandPlay.filename;
                            LensPlayer.videoList = newVideoList;
                        }
                        vidIndex = idx;
                        SceneControllerLensPlayerVideoPlayback.SetPlaylistPlayback(idx);
                        long serverTimeMS = LensPlayer.GetServerTimeMS();
                        if (LensPlayer.udpCommandPlay.playtime <= serverTimeMS)
                        {
                            StartVideo();
                        }
                        else
                        {
                            nextScreeningSchedule.gameObject.SetActive(false);
                            nextScreeningTitle.gameObject.SetActive(true);
                            nextScreeningValue.gameObject.SetActive(true);
                            countdownTimer = (int)((LensPlayer.udpCommandPlay.playtime - serverTimeMS) / 1000);
                            StartCoroutine(UpdateCountdown());
                            InitThumbnail();
                        }
            */
		}


        // Make sure the trailer video volume is off.
        if ((null != trailerVideoPlayer) && (trailerVideoPlayer.IsPlaying()))
        {
            trailerVideoPlayer.volume = 0.0f;
        }


        // If the playback scene is not loading and the escape key is pressed,
        // then start the video.
        if (!SceneLoader.IsLoading() && (Input.GetKeyDown(KeyCode.Escape)))
        {
            if (Time.time - lastForcePlaybackInputTime < 2.0f && !playbackStart)
            {
                Logger.Log("Starting playback on user input");
				StartCoroutine(PlayAnimation());
				playbackStart = true;
            }
            lastForcePlaybackInputTime = Time.time;
        }

		// Heads up countdown

		if (LensPlayer.headsUpCountdown)
		{
			huCounter = LensPlayer.headsUp - Time.timeSinceLevelLoad;
			int seconds = Mathf.FloorToInt(huCounter % 60);
			int minutes = Mathf.FloorToInt(huCounter / 60);
			nextScreeningValue.text = String.Format("{0:00}:{1:00}", minutes, seconds);
		}

		// Heads Up Math

		if (head != null)
		{
			float headAngle = head.eulerAngles.x;
			if (headAngle > 180)
			{
				headAngle -= 360;
			}
			headAngle = headAngle * -1;
			if (headAngle >= lookUpAngle && lookUp)
			{
				lookUp = false;
				StartCoroutine(HeadsUpCountdown());
			}
		}

#if !UNITY_EDITOR
		if (!OVRPlugin.userPresent)
		{
			lookUp = false;
			theaterModeControlsGroup.gameObject.SetActive(true);
			lookUpText.gameObject.SetActive(false);

			headsUpTimer.gameObject.SetActive(false);
			headsUpBackground.gameObject.SetActive(false);

			StopCoroutine(HeadsUpCountdown());
			StopCoroutine(HeadsUpCounter());
		}
#endif
		if (Input.GetKeyDown(KeyCode.Q))
        {
            LensPlayer.isResetRequested = true;
        }

        if ((LensPlayer.isResetRequested) && (countdownTimer == 0))
        {
            LensPlayer.isResetRequested = false;
        }



            // If the playback scene is loaded and the fader is done, then activate
            // the playback scene.
            if (SceneLoader.IsSceneLoaded() && !SceneFader.IsFading())
        {
            SceneLoader.ActivateLoadedScene();
        }


        if ((OVRPlugin.userPresent) && (userLeft == true))
        //        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (LensPlayer.videoAutoStart == true)
            {
                StartVideo();
            }
            else if ((LensPlayer.screeningCountdownSeconds != -1) && (countdownTimer == 0))
            {
                Logger.Logd("User back");
                countdownTimer = LensPlayer.screeningCountdownSeconds;
                nextScreeningTitle.gameObject.SetActive(true);
                nextScreeningValue.gameObject.SetActive(true);
                StartCoroutine(UpdateCountdown());
            }
			if (LensPlayer.headsUpSetting)
			{
				// Start Heads Up Counter
				StartCoroutine(HeadsUpCounter());
				LensPlayer.headsUpSetting = false;
			}

			userLeft = false;
        }
        if ((!userLeft) && (!OVRPlugin.userPresent))
        {
            Logger.Logd("User left");
			StopCoroutine(HeadsUpCounter());
			StopCoroutine(HeadsUpCounter());
			LensPlayer.headsUpSetting = true;
            userLeft = true;
        }




    }


    bool userLeft = false;



#endregion




#region PRIVATE METHODS


    /**
    *
    * Checks the size of the title text and updpates the font size accordingly.
    * All measurements are based on the MontereyFLF font
    * This method is called after every time a new title is set.
    *
    **/
    void adjustTitleFont()
    {

      //if user specified a need for all basic fonts, switch all fonts to Arial
      if (LensPlayer.basicFont) allFontsToBasic();

      Logger.Log("This is a printout before.");
      if (LensPlayer.basicFont) {
        Logger.Log("basicFont was on");

      } else {
        Logger.Log("basicFont was off");
      }
      if (title.text.Length <= 12) {
        title.fontSize = 60;
      } else if (title.text.Length > 12 && title.text.Length <= 14) {
        title.fontSize = 50;
      } else if (title.text.Length > 14 && title.text.Length <= 18) {
        title.fontSize = 40;
      } else if (title.text.Length > 18 && title.text.Length <= 23) {
        title.fontSize = 30;
      } else if (title.text.Length > 23 && title.text.Length <= 58) {
        title.fontSize = 25;
      } else if (title.text.Length > 59 && title.text.Length <= 70) {
        title.fontSize = 20;
      } else if (title.text.Length > 70) {
        title.fontSize = 20;
        truncateTitle();
      }
    }


    /**
    *
    * This method is called if the user specified the basicFont flag.
    * It switches the font of all text that the user can see to a basic Arial font
    *
    **/
    void allFontsToBasic()
    {
      customMessage.font = Resources.GetBuiltinResource<Font>(basicFontName);
      title.font = Resources.GetBuiltinResource<Font>(basicFontName);
      nextScreeningTitle.font = Resources.GetBuiltinResource<Font>(basicFontName);
      nextScreeningTitle.fontSize = 38;
      nextScreeningValue.font = Resources.GetBuiltinResource<Font>(basicFontName);
      nextScreeningSchedule.font = Resources.GetBuiltinResource<Font>(basicFontName);
    }

    /**
    *
    * Truncates title if too long
    *
    **/
    void truncateTitle() {
      string newTitle = title.text.Substring(0, 70) + "...";
      title.text = newTitle;
    }



    /**
     * Coroutine that updates the countdown timer and eventualy starts the video.
     * 
     **/
    IEnumerator UpdateCountdown()
    {
        bool launchedAnimations = false;
        System.TimeSpan ts = System.TimeSpan.FromSeconds((double)countdownTimer);
        nextScreeningValue.text = (ts.Minutes < 10 ? "0" : "") + ts.Minutes.ToString() + ":" + (ts.Seconds < 10 ? "0" : "") + ts.Seconds.ToString();

        Platform.BroadcastDeviceState(Platform.DeviceState.Countdown, "", 444);

        float startTime = Time.time;
        while (countdownTimer > 0)
        {
            if (null != LensPlayer.udpCommandPlay)
            {
                long serverTimeMS = LensPlayer.GetServerTimeMS();
                countdownTimer = (int)((LensPlayer.udpCommandPlay.playtime - serverTimeMS) / 1000);
            }
            else
            {
                DateTime t = DateTime.Now;
                int currentHourSecond = t.Minute * 60 + t.Second;

                if (LensPlayer.screeningScheduleMinutes != -1)
                {
                    int screeningScheduleSeconds = LensPlayer.screeningScheduleMinutes * 60;
                    countdownTimer = screeningScheduleSeconds - (currentHourSecond % screeningScheduleSeconds);
                }
                else
                {
                    countdownTimer = (int)(LensPlayer.screeningCountdownSeconds - (Time.time - startTime));
                }
            }
            ts = System.TimeSpan.FromSeconds((double)countdownTimer);
            nextScreeningValue.text = (ts.Minutes < 10 ? "0" : "") + ts.Minutes.ToString() + ":" + (ts.Seconds < 10 ? "0" : "") + ts.Seconds.ToString();
            if (countdownTimer == 6 && !launchedAnimations)
            {
				// Finds and sets page back to video info
				var pagedRect = GameObject.FindObjectOfType<UI.Pagination.PagedRect>();
				if (pagedRect.CurrentPage != 1)
				{
					pagedRect.SetCurrentPage(1);
					pagedRect.NextPageKey = KeyCode.None;
				}
				else
				{
					pagedRect.NextPageKey = KeyCode.None;
				}

				nextScreeningSchedule.gameObject.SetActive(false);
				trailerDisplay.SetActive(false);
                launchedAnimations = true;
                RunPlayAnimations(theaterModeControlsGroup.gameObject);
				RunPlayAnimations(theaterModeTimerGroup.gameObject);
				paginationGroup.GetComponent<Animator>().Play("Play");
            }
            if (countdownTimer < 1)
            {
                break;
            }
            if (LensPlayer.isResetRequested)
            {
                nextScreeningTitle.gameObject.SetActive(false);
                nextScreeningValue.gameObject.SetActive(false);
                hasPlayed = true;
                vidIndex = 0;
                LensPlayer.isResetRequested = false;
                LensPlayer.udpCommandPlay = null;
                SceneLoader.Load("1b. Waiting Room", true);
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
//		if (LensPlayer.screeningCountdownSeconds != -1 || LensPlayer.screeningScheduleMinutes != -1)
		{
            videoThumbnail.gameObject.SetActive(false);
            title.gameObject.SetActive(false);
            nextScreeningTitle.gameObject.SetActive(false);
            nextScreeningValue.gameObject.SetActive(false);
            nextScreeningSchedule.gameObject.SetActive(false);
            messageBackground.SetActive(false);
            //infoBackground.SetActive(false);
            nextScreeningSchedule.gameObject.SetActive(false);
			paginationGroup.gameObject.SetActive(false);
			opener.gameObject.SetActive(true);
            opener.commenceAnimation();
			yield return new WaitForSeconds(7);
            StartVideo();
          }
    }

	IEnumerator PlayAnimation()
	{
		LensPlayer.headsUpSetting = false;
		nextScreeningSchedule.gameObject.SetActive(false);
		trailerDisplay.SetActive(false);
		paginationGroup.GetComponent<Animator>().Play("Play");
		RunPlayAnimations(theaterModeControlsGroup.gameObject);
		paginationGroup.gameObject.SetActive(false);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForEndOfFrame();
        opener.gameObject.SetActive(true);
        opener.commenceAnimation();
		yield return new WaitForSeconds(7);
		StartVideo();
	}

	IEnumerator HeadsUpCounter()
	{
		if (!LensPlayer.headsUpSetting)
		{
			yield break;
		}

        // Wait for a certain amount of time.
		yield return new WaitForSeconds(LensPlayer.headsUp - 6f);

		// Redirect back to first page
		var pagedRect = GameObject.FindObjectOfType<UI.Pagination.PagedRect>();
		if (pagedRect.CurrentPage != 1)
		{
			pagedRect.SetCurrentPage(1);
			pagedRect.NextPageKey = KeyCode.None;
		}
		else
		{
			pagedRect.NextPageKey = KeyCode.None;
		}

		yield return new WaitForSeconds(5f);

        // If UDP command received, then do nothing.
        if (LensPlayer.udpCommandPlay != null)
        {
            yield break;
        }


        // Turn off timer
        nextScreeningTitle.gameObject.SetActive(false);
		nextScreeningValue.gameObject.SetActive(false);

		// If UDP command received, then do nothing.
		if (LensPlayer.udpCommandPlay != null)
        {
            yield break;
        }

		if (theaterModeControlsGroup.gameObject.activeSelf)
		{
			paginationGroup.GetComponent<Animator>().Play("Play");
			RunPlayAnimations(theaterModeControlsGroup.gameObject);
		}
		yield return new WaitForSeconds(1f);
		OrientationResetter.SetHorizontalAllowed(false);
		//theaterModeControlsGroup.gameObject.SetActive(false);
		//paginationGroup.gameObject.SetActive(false);
		lookUpText.gameObject.SetActive(true);

		// Set headsup to true, activate other timer object
		LensPlayer.headsUpSetting = false;
		lookUp = true;
	}

	IEnumerator HeadsUpCountdown()
	{
		if (playbackStart)
		{
			yield break;
		}
		lookUpText.gameObject.SetActive(false);
		headsUpTimer.gameObject.SetActive(true);
		headsUpBackground.gameObject.SetActive(true);
		yield return new WaitForSeconds(1);
        if (LensPlayer.udpCommandPlay != null)
        {

        }
		headsUpTimer.text = "00:02";
		yield return new WaitForSeconds(1);
		headsUpTimer.text = "00:01";
		yield return new WaitForSeconds(1);
		headsUpTimer.text = "00:00";
		yield return new WaitForSeconds(0.5f);
		paginationGroup.gameObject.SetActive(false);
		headsUpTimer.gameObject.SetActive(false);
		headsUpBackground.gameObject.SetActive(false);
		headsUpOpener.gameObject.SetActive(true);
		headsUpOpener.commenceAnimation();
		yield return new WaitForSeconds(7);
		StartVideo();
	}









    /**
     * Runs the "Play" animations for the specified GameObject and child objects.
     * 
     **/
    void RunPlayAnimations(GameObject go)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            Animator animCont = go.transform.GetChild(i).GetComponent<Animator>();
            if (animCont != null)
            {
                animCont.Play("Play");
                RunPlayAnimations(go.transform.GetChild(i).gameObject);
            }
        }
    }










    /**
     * Initializes the video thumbnail.
     * 
     * This method assumes that if the trailerDisplay object is active, then the thumbhail
     * is not to display.
     * 
     **/
    void InitThumbnail()
    {
        // If the thumbnail is in the data directory, then use that.
        string fn = LensPlayer.videoList[vidIndex];
        if (fn.Contains("" + Path.DirectorySeparatorChar))
        {
            fn = fn.Substring(fn.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }
        fn = fn.Substring(0, fn.Length - 4);
        string thumbnailFilename = LensPlayer.welensPath + "data" + Path.DirectorySeparatorChar + fn + ".jpg";
        if (File.Exists(thumbnailFilename))
        {
            Logger.Log(Logger.LogLevel.Verbose, "\tUsing thumbnail in data directory.");
            StartCoroutine(LoadTheaterVideoThumbnail(thumbnailFilename));
        }


        // Otherwise, if the ThumbnailManager has the thumbnail, use that.
        else
        {
            string filepath = LensPlayer.welensPath + LensPlayer.videoList[vidIndex];
            thumbnailFilename = Platform.GetVideoThumbnail(filepath);
            if (null != thumbnailFilename)
            {
                StartCoroutine(LoadTheaterVideoThumbnail(thumbnailFilename));
            }
        }
    }










    /**
     * Loads the thumbanail from disk.
     * 
     * Once the thumbnail is loaded, the thumbnail display is set.
     * 
     * XXX BAL: do error handling.
     **/
    IEnumerator LoadTheaterVideoThumbnail(string filename)
    {
        WWW www = new WWW("file://" + filename);
        yield return www;
        videoThumbnail.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
    }










    /**
     * Initializes the trailer video playback.
     * 
     * If the trailer file exists, it is loaded and started. Otherwise, the trailer object is set as inactive.
     * 
     **/
    void InitTrailer()
    {
        Logger.Log("Initializing the trailer.");
        string videoName = "";
        string videoTrailerPath = "";
        videoName = LensPlayer.videoList[vidIndex];
        videoTrailerPath = LensPlayer.welensPath + videoName.Insert(videoName.IndexOf('.'), "_Trailer");
        if (File.Exists(videoTrailerPath))
        {
            Logger.Log("\tTrailer found. Starting.");
            trailerVideoPlayer.videoPath = videoTrailerPath;
            trailerVideoPlayer.autoPlay = true;
            trailerVideoPlayer.looping = true;
            trailerVideoPlayer.Load();
        }
        else
        {
            Logger.Log("\tTrailer not found.");
            trailerDisplay.SetActive(false);
        }
    }










    /**
     * Starts the current video.
     * 
     **/
    void StartVideo()
    {
        SceneControllerLensPlayerVideoPlayback.SetPlaylistPlayback(vidIndex);
//        VideoRenderer.SetMonoMaterial(null);
        SceneLoader.Load("2. Video Playback", false);
        hasPlayed = true;
		playbackStart = false;
    }


    public void ThumbnailCreated(string videoFilename, string thumbnailFilename)
    {
        StartCoroutine(LoadTheaterVideoThumbnail(thumbnailFilename));
    }


#endregion


}
