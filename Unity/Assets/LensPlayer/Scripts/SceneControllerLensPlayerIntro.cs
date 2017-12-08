using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

/**
 * This class corresponds to the intro scene of the LensPlayer app. When the app is deployed, this is the first scene executed.
 *
 * The intro scene is a cover for the app while it initializes. The Start method of this class waits for the app controller to load the loading screen image.
 * Once the loading screen image is loaded, it is passed to the Scene Fader. The Scene Loader is then instructed to load the next scene.
 *
 * The Update method waits for further initialization to complete. After a certian minimum number of seconds and all initiallization has completed, the app
 * advances to the next scene.
 *
 **/
public class SceneControllerLensPlayerIntro : MonoBehaviour
{
    #region PRIVATE VARIABLES

    [Tooltip("Splash image")]
    [SerializeField]
    Image splashImage = null;


    [Tooltip("Default splash image sprite")]
    [SerializeField]
    Sprite defaultSplashImage = null;

    #endregion




    #region PRIVATE VARIABLES

    // Minimum amount of time to display the intro.
    int minimumDisplayTimeSeconds = 2;

    // Time the scene started.
    float startTime;

    static bool debugSimulateUDP = false;


    #endregion




    #region MONOBEHAVIOUR OVERRIDE METHODS


    /**
     * Performs scene startup tasks.
     *
     * The loading image is set, the scene fader is faded in, and the next scene is loaded.
     *
     **/
    IEnumerator Start ()
    {
		// If there is a loading image, then load it and set the splash logo to it.
		while (!AppControllerLensPlayer.IsLoadingImageChecked())
        {
            yield return new WaitForEndOfFrame();
        }
        if (AppControllerLensPlayer.HasLoadingImage())
        {
            Sprite loadingImage = AppControllerLensPlayer.GetLoadingImage();
            splashImage.color = Color.white;
            splashImage.sprite = loadingImage;
		}
        else
        {
            splashImage.sprite = defaultSplashImage;
            splashImage.color = Color.white;
        }


        // Wait for the app controller to be ready.
        while (!AppControllerLensPlayer.IsReady())
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();

		// Check if LensPass App is installed
		// by checking if welens folder exists
		if (!LensPlayer.isInstalled)
		{
			Logger.Logd("Welens path does not exist");
			splashImage.gameObject.SetActive(false);
			AppControllerLensPlayer.pathStatus();
			yield break;
		}

		// If a start video is specified, then start the start video.
		string startVideo = StartVideoReceiver.GetStartVideoPath();
        if (null != startVideo)
        {
            SceneControllerLensPlayerVideoPlayback.SetVideoFilePath(startVideo);
            SceneLoader.Load("2. Video Playback", false);
        }


        // Otherwise, if theater mode is specified, then handle.
        else if (LensPlayer.theaterMode)
        {
            // If the video is to autostart, then start it.
            if (LensPlayer.videoAutoStart)
            {
                // If strict mode is enabled and a screening schedule is specified, then
                // load theater mode.
                if ((LensPlayer.isStrictModeEnabled) && (LensPlayer.screeningScheduleMinutes != -1))
                {
                    Logger.Log("Loading Theater Mode");
                    if (LensPlayer.theaterTheme == "rainforest")
                    {
                        SceneLoader.Load("1c. Waiting Room Rainforest", false);
                    }
                    else
                    {
                        SceneLoader.Load("1b. Waiting Room", false);
                    }
                }
                else
                {
                    Logger.Log("Autoplay video");
                    SceneLoader.Load("2. Video Playback", false);
                }
            }
            else
            {
                Logger.Log("Loading Theater Mode");
                if (LensPlayer.theaterTheme == "rainforest")
                {
                    SceneLoader.Load("1c. Waiting Room Rainforest", false);
                }
                else
                {
                    SceneLoader.Load("1b. Waiting Room", false);
                }
            }
        }

        
        // Otherwise, load the video selection view.
        else
        {
            Logger.Log("Loading App Mode");
            SceneLoader.Load("1a. Video Selection", false);
        }


        // Record the start time in order to do display the loading image for a minimum amount of time.
        startTime = Time.time;
	}







    /**
     * Performs scene update tasks.
     *
     * Activates the scene when conditions are met.
     *
     **/
    void Update()
    {
        // If the scene is still loading, the fader is still fading, the minimum time hasn't passed, or app initialization hasn't completed, then do nothing.
        if (!SceneLoader.IsSceneLoaded() || SceneFader.IsFading() || (Time.time - startTime < minimumDisplayTimeSeconds))
        {
            return;
        }


        // Otherwise, activate the new scene.
        LensPlayer.udpCommandPlay = null;
        if (true == debugSimulateUDP)
        {
            if (null == LensPlayer.udpCommandPlay)
            {
                LensPlayer.udpCommandPlay = new UDPCommandPlay();
                LensPlayer.udpCommandPlay.filename = "";
                LensPlayer.udpCommandPlay.playtime = LensPlayer.GetServerTimeMS() + (10 * 1000);
            }
        }

        LensPlayer.isResetRequested = false;
        SceneLoader.ActivateLoadedScene();
	}


    #endregion


}
