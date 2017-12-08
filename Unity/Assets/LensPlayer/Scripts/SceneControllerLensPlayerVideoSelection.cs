using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


/**
 * Handled the video selection scene.
 * 
 **/
public class SceneControllerLensPlayerVideoSelection : MonoBehaviour, ThumbnailCreatedMessageTarget
{

    #region EDITOR INTERFACE VARIABLES

    [Tooltip("Video item")]
    [SerializeField]
    public GameObject videoItem;

    [Tooltip("Video item content")]
    [SerializeField]
    public GameObject content;

    [Tooltip("Title text")]
    [SerializeField]
    public Text titleText;

    [Tooltip("Title text background")]
    [SerializeField]
    public GameObject titleBackground;

    [Tooltip("Environment")]
    [SerializeField]
    public Material environment;


    [Tooltip("Environment")]
    [SerializeField]
    public Texture defaultBackground;

    [Tooltip("Number of pages text in the navigator")]
    [SerializeField]
    public Text pageText;

    [Tooltip("Stare button image")]
    [SerializeField]
    public Image stareInputButtonImage;

    [Tooltip("Text on stare toggle button")]
    [SerializeField]
    public Text stareInputButtonText;

    [Tooltip("Black button image")]
    [SerializeField]
    public Sprite blackButton;

    [Tooltip("White button")]
    [SerializeField]
    public Sprite whiteButton;

    [Tooltip("Navigator")]
    [SerializeField]
    public GameObject navigator;

    [SerializeField]
    GameObject leftNavigation;

    [SerializeField]
    GameObject rightNavigation;

    [SerializeField]
    ScrollRect scrollRect;

    [SerializeField]
    GameObject gazeNavigation;


    #endregion




    #region PRIVATE VARIABLES

    // The video data.
    Platform.VideoData[] videoData;

    // Flag indicating stare input has been activiated. Stare input is activated on a delay.
    bool hasActivatedStareInput = false;

    // The time the scene has started.
    float startTime = 0;

    // The delay before the scene fades in.
    float startDelay = 0.0f;

    // Flag indicating the scene fader has faded out.
    bool hasFadedOut = false;


    static bool debugSimulateUDP = false;


    bool hasHandledUDPCommand = false;

    int currentPage = 0;
    int numPages = 0;


    float targetScrollPos = 0;


    #endregion




    #region MONOBEHAVIOUR OVERRIDE METHODS


    /**
     * Performs startup tasks.
     * 
     **/
    IEnumerator Start()
    {
        if (LensPlayer.disableControls)
        {
            gazeNavigation.SetActive(false);
        }
        if (true == debugSimulateUDP)
        {
            if (null == LensPlayer.udpCommandPlay)
            {
                LensPlayer.udpCommandPlay = new UDPCommandPlay();
                LensPlayer.udpCommandPlay.filename = "360sample.mp4";
                LensPlayer.udpCommandPlay.playtime = LensPlayer.GetServerTimeMS() + (10 * 1000);
            }
            debugSimulateUDP = false;
        }

        Logger.Log("App Mode waiting for app controller...");

        
        // Wait for the app controller to be ready.
        while (!AppControllerLensPlayer.IsReady())
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();


        Logger.Log("****************************************");
        Logger.Log("Starting App Mode.");

        WWW lobby = new WWW("file:///sdcard/welens/lobby.jpg");
        yield return lobby;
        if (!string.IsNullOrEmpty(lobby.error))
        {
            environment.mainTexture = defaultBackground;
        }
        else
            environment.mainTexture = lobby.texture;



        Platform.BroadcastDeviceState(Platform.DeviceState.Idle, "", 0);


        // Set the mono video renderer active, and set the mono material to the video selection
        // environment.
        VideoRenderer.SetVideoRendererActive(VideoRenderer.RendererType.mono);
        VideoRenderer.SetMonoMaterial(environment);


        // Set the watermark as visible accordingly.
        Logger.Log("Setting watermark as " + (LensPlayer.isTrialMode ? "" : "not ") + "visible");
        AppControllerLensPlayer.SetWatermarkVisible(LensPlayer.isTrialMode);


        // Set the gaze pointer and gaze based interaction control as not active.
        GazeInput.SetPointerActive(false);
        GazeBasedInteractionControl.SetTimedInputActive(false);


        // Set the title.
        Logger.Log("Setting title to: " + LensPlayer.custom_message);
        titleText.text = LensPlayer.custom_message;
        if (LensPlayer.custom_message.Length == 0)
        {
            titleBackground.SetActive(false);
        }

        // Get the video data.
        videoData = AppControllerLensPlayer.GetAvailableVideos();
        Logger.Log(Logger.LogLevel.Debug, videoData.Length + " videos on device.");
        ArrayList welensVideos = new ArrayList();
        for (int i=0; i<videoData.Length; ++i)
        {
            if (videoData[i].filePath.Contains("welens"))
            {
                welensVideos.Add(videoData[i]);
            }
        }
        videoData = new Platform.VideoData[welensVideos.Count];
        for (int i=0; i<welensVideos.Count; ++i)
        {
            videoData[i] = (Platform.VideoData)welensVideos[i];
        }
        Logger.Log(Logger.LogLevel.Debug, videoData.Length + " videos in WeLens directory.");


        // Fill the content with video items.
        RectTransform contentRect = content.GetComponent<RectTransform>();
        RectTransform itemRect = videoItem.GetComponent<RectTransform>();
        int numItemsWide = 3;
        int numItemsTall = 2;
        float spacingX = (contentRect.rect.width - (itemRect.rect.width * numItemsWide)) / numItemsWide;
        float spacingY = (contentRect.rect.height - (itemRect.rect.height * numItemsTall)) / numItemsTall;
        for (int i=0; i< videoData.Length; ++i)
        {
            // Create the item.
            Logger.Log(Logger.LogLevel.Verbose, "Creating video item " + i);
            GameObject newVideoItem = GameObject.Instantiate(videoItem, content.transform);
            newVideoItem.GetComponent<VideoItem>().SetVideoData(videoData[i]);
            newVideoItem.SetActive(true);

            // Position the item.
            RectTransform rt = newVideoItem.GetComponent<RectTransform>();
            int page = i / (numItemsTall*numItemsWide);
            rt.localPosition = new Vector3(
                (page*contentRect.rect.width) + (i%numItemsWide) * (itemRect.rect.width + spacingX) + (spacingX*0.5f),
                (-itemRect.rect.height*page) - ((i/numItemsWide) - (page * numItemsWide)) * (spacingY + itemRect.rect.height) - (spacingY * 0.5f),
                0);

            // Set the title.
            Logger.Log(Logger.LogLevel.Verbose, "\tSetting title to " + videoData[i].title);
            newVideoItem.transform.Find("Title").GetComponent<Text>().text = videoData[i].title;


            // If the thumbnail is in the data directory, then use that.
            string fn = videoData[i].filePath;
            fn = fn.Substring(fn.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            fn = fn.Substring(0, fn.Length - 4);
            string thumbnailFilename = LensPlayer.welensPath + "data" + Path.DirectorySeparatorChar + fn + ".jpg";
            if (File.Exists(thumbnailFilename))
            {
                Logger.Log(Logger.LogLevel.Verbose, "\tUsing thumbnail in data directory.");
                StartCoroutine(LoadThumbnailFromDisk(thumbnailFilename, newVideoItem.transform.Find("Thumbnail").gameObject));
            }


            // Otherwise, if the ThumbnailManager has the thumbnail, use that.
            else
            {
                thumbnailFilename = Platform.GetVideoThumbnail(videoData[i].filePath);
                if (null != thumbnailFilename)
                {
                    StartCoroutine(LoadThumbnailFromDisk(thumbnailFilename, newVideoItem.transform.Find("Thumbnail").gameObject));
                }
            }
        }


        // If there are more than one page, then display the navigator.
        // Otherwise, hide it.
        numPages = videoData.Length / (numItemsWide * numItemsTall) + 1;
        if (videoData.Length > numItemsWide*numItemsTall)
        {
            pageText.text = "1 of " + numPages;
//            navigator.SetActive(false);
        }
        else
        {
            navigator.SetActive(false);
        }

        Vector2 sd = contentRect.sizeDelta;
        sd.x *= numPages;
        contentRect.sizeDelta = sd;
        leftNavigation.SetActive(false);


        // Check the headphone jack and display a message if not connected.
        AppControllerLensPlayer.CheckHeadphoneJack();

        bool isStareNavigationActive = PlayerPrefs.GetInt("stare_navigation", 1) == 1 ? true : false;
        if (false == isStareNavigationActive)
        {
            OnToggleStareNavigation(false);
        }
        else
        {
            Logger.Log(Logger.LogLevel.Debug, "prefs: stare on");
        }


        // Record the start time.
        startTime = Time.time;
    }



    /**
     * Performs update tasks.
     * 
     **/
    void Update()
    {
        if (SceneLoader.IsSceneLoaded() && !SceneFader.IsFading())
        {
            SceneLoader.ActivateLoadedScene();
        }


        // If a specified time has passed, then activate timed gaze input.
        if (!hasActivatedStareInput && (!GazeBasedInteractionControl.GetTimedInputActive()) && (Time.time - startTime > (startDelay + 2.0f)))
        {
            GazeBasedInteractionControl.SetTimedInputActive(true);
            hasActivatedStareInput = true;
        }

    
        // If the start delay has passed and the screen fader hasn't started fading out,
        // then fade out and set the gaze pointer active.
        if ((!hasFadedOut) && (startTime != 0) && (Time.time - startTime > startDelay) && (hasHandledUDPCommand==false))
        {
            hasFadedOut = true;
            GazeInput.SetPointerActive(true);
            SceneFader.FadeOut();
        }


        if ((LensPlayer.udpCommandPlay != null) && (!hasHandledUDPCommand))
        {
            GazeInput.SetPointerActive(false);
            hasHandledUDPCommand = true;
            Logger.Log("UDP command detected");
            SceneLoader.Load("1b. Waiting Room", false);
        }


        float currentScrollPos = scrollRect.horizontalNormalizedPosition;
        if (currentScrollPos != targetScrollPos)
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(currentScrollPos, targetScrollPos, Time.deltaTime * 5.0f);
        }


    }



    #endregion




    #region PUBLIC METHODS


    /**
     * Called by video items when clicked to play a video.
     * 
     **/
    static public void PlayVideo(string filePath)
    {
        Logger.Log("Video selected: " + filePath);
        SceneControllerLensPlayerVideoPlayback.SetVideoFilePath(filePath);
        GazeInput.SetPointerActive(false);
        SceneLoader.Load("2. Video Playback", false);
    }










    /**
     * Toggles the visibility of the navigator.
     * 
     **/
    public void OnToggleStareNavigation(bool playSound = true)
    {
        if (playSound)
        {
            AppControllerLensPlayer.PlayToggleClickSound();
        }
        if (GazeBasedInteractionControl.GetTimedInputActive())
        {
            Logger.Log(Logger.LogLevel.Debug, "stare toggled off");
            GazeBasedInteractionControl.SetTimedInputActive(false);
            stareInputButtonImage.sprite = blackButton;
            stareInputButtonText.text = "Stare Navigation: Off";
            stareInputButtonText.color = Color.white;
            PlayerPrefs.SetInt("stare_navigation", 0);
        }
        else
        {
            Logger.Log(Logger.LogLevel.Debug, "stare toggled on");
            GazeBasedInteractionControl.SetTimedInputActive(true);
            stareInputButtonImage.sprite = whiteButton;
            stareInputButtonText.text = "Stare Navigation: On";
            stareInputButtonText.color = Color.black;
            PlayerPrefs.SetInt("stare_navigation", 1);
        }
    }


    #endregion




    #region PRIVATE METHODS



    /**
     * Loads a thumbnail from disk.
     * 
     * XXX BAL: add error handling here.
     * 
     **/
    IEnumerator LoadThumbnailFromDisk(string filename, GameObject thumbnail)
    {
        WWW www = new WWW("file://" + filename);
        yield return www;
        thumbnail.GetComponent<Image>().sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
    }




    Texture2D CreateThumbnailFromPixels(int width, int height, Color[] pixels)
    {
        // Create the largest 16:9 texture that can be created from the pixels.
        int newWidth;
        int newheight;
        int xOffset = 0;
        int yOffset = 0;
        if (width >= height)
        {
            // If the height is not large enough then adjust.
            if (height < ((width * 9.0f) / 16.0f))
            {
                newheight = height;
                newWidth = (int)(height * 16.0f / 9.0f);
                xOffset = (width - newWidth) / 2;
            }
            else
            {
                newWidth = width;
                newheight = (int)(width * 9.0f / 16.0f);
                yOffset = (height - newheight) / 2;
            }
        }

        // Otherwise the image is a portrait.
        else
        {
            newWidth = width;
            newheight = (int)(width * 9.0f / 16.0f);
            yOffset = (height - newheight) / 2;
        }


        // Create an array of colors with the new dimensions.
        Color[] newColors = new Color[newWidth * newheight];
        int pixelIndex = 0;
        for (int y=0; y<height; ++y)
        {
            if ((y < yOffset) || (y >= newheight + yOffset))
            {
                continue;
            }
            for (int x = 0; x < width; ++x)
            {
                if ((x < xOffset) || (x >= newWidth + xOffset))
                {
                    continue;
                }
                newColors[pixelIndex++] = pixels[y * width + x];
            }
        }
        Texture2D sizedImage = new Texture2D(newWidth, newheight);
        sizedImage.SetPixels(newColors);


        // Resize the texture to have a maximum dimension of 512.
        int computeWidth = newWidth;
        int computeHeight = newheight;
        if (newWidth > 512)
        {
            computeWidth = 512;
            computeHeight = (512 * 9) / 16;
        }
//        sizedImage.Resize(computeWidth, computeHeight);
        sizedImage.Apply();

        return sizedImage;
    }







    #endregion





    void OnNavigationButtonEnter(GameObject obj)
    {
        Image image = obj.GetComponent<Image>();
        image.color = Color.white;

        GameObject text = obj.transform.GetChild(1).gameObject;
        text.SetActive(true);
    }


    public void OnLeftNavigationButtonEnter()
    {
        if (currentPage != 0)
        {
            OnNavigationButtonEnter(leftNavigation);
        }
    }


    public void OnRightNavigationButtonEnter()
    {
        if (currentPage != numPages-1)
        {
            OnNavigationButtonEnter(rightNavigation);
        }
    }



    void OnNavigationButtonExit(GameObject obj)
    {
        Image image = obj.GetComponent<Image>();
        image.color = new Color(1, 1, 1, 0);

        GameObject text = obj.transform.GetChild(1).gameObject;
        text.SetActive(false);
    }


    public void OnLeftNavigationButtonExit()
    {
        OnNavigationButtonExit(leftNavigation);
    }


    public void OnRightNavigationButtonExit()
    {
        OnNavigationButtonExit(rightNavigation);
    }






    public void OnLeftNavigationButtonClick()
    {
        if (currentPage == 0)
        {
            return;
        }

        AppControllerLensPlayer.PlayButtonClickSound();
        rightNavigation.SetActive(true);
        OnNavigationButtonExit(rightNavigation);

        if (--currentPage == 0)
        {
            leftNavigation.SetActive(false);
        }
        targetScrollPos = ((float)currentPage) / ((float)(numPages));
//        scrollRect.horizontalNormalizedPosition = pos;
        pageText.text = (currentPage + 1) + " of " + numPages;
    }





    public void OnRightNavigationButtonClick()
    {
        if (currentPage == (numPages - 1))
        {
            return;
        }

        AppControllerLensPlayer.PlayButtonClickSound();
        leftNavigation.SetActive(true);
        OnNavigationButtonExit(leftNavigation);


        if (++currentPage == (numPages - 1))
        {
            rightNavigation.SetActive(false);
        }
        targetScrollPos = ((float)currentPage+1) / ((float)(numPages));
//        scrollRect.horizontalNormalizedPosition = pos;
        pageText.text = (currentPage+1) + " of " + numPages;
    }

    public void ThumbnailCreated(string videoFilename, string thumbnailFilename)
    {
        Logger.Log(Logger.LogLevel.Debug, "vid selection received message");


        VideoItem currentItem = null;
        for (int i=0; i< content.transform.childCount; ++i)
        {
            VideoItem item = content.transform.GetChild(i).gameObject.GetComponent<VideoItem>();
            if (item.GetVideoData().filePath.Equals(videoFilename))
            {
                currentItem = item;
                break;
            }
        }
        if (currentItem != null)
        {
            StartCoroutine(LoadThumbnailFromDisk(thumbnailFilename, currentItem.transform.Find("Thumbnail").gameObject));

        }
    }
}
