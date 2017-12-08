using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AppControllerLensPlayer : MonoBehaviour
{

    public AudioClip buttonRollover;
    public AudioClip toggleClick;
    public AudioClip buttonClick;

    public GameObject watermark;
    public GameObject jackNotify;
	public GameObject pathImage;

    public Video_List video_list = new Video_List();


    static Sprite loadingImage = null;


    static AppControllerLensPlayer instance;

    Platform.VideoData[] videoData;

    static bool isReady = false;

    static public Sprite GetLoadingImage()
    {
        return loadingImage;
    }

    bool useStaging = false;

    /**
     *
     *
     **/
    IEnumerator Start()
    {
        // Store a reference to self.
        instance = this;
		
		// Init the loading image.
		yield return InitLoadingImage();

		

		// Wait for the platform to be ready before doing anything else.
		while (!Platform.IsReady())
        {
            yield return new WaitForEndOfFrame();
        }

		SetStagingFlag();

		// Load the plan data.
		yield return new WaitForEndOfFrame();
        yield return LoadPlanData();


        // Load the configuration.
        LoadConfiguration();


        // Load the player data file.
        LoadPlayerJSONFile();

        if (LensPlayer.demo)
        {
            Logger.Log("DEMO VERSION");
        }
        else if (LensPlayer.plan == null)
        {
            Logger.Log("No plan data.");
        }
        else if (LensPlayer.plan.status < 2)
        {
            Logger.Log("Trial Plan.");
        }

        if ((!LensPlayer.demo) && ((LensPlayer.plan == null) || (LensPlayer.plan.status < 2)))
        {
            LensPlayer.isTrialMode = false; // temporarily hardcoded as false. TODO set back to true after trial mode QA.
        }
        else
        {
            LensPlayer.isTrialMode = false;
        }


        // Get the available videos.
        videoData = Platform.GetAvailableVideos();

        while (!Logger.IsReady())
        {
            yield return new WaitForEndOfFrame();
        }
        if (LensPlayer.developerMode)
        {
            Logger.SetLogViewVisible(true);
            Profiler.SetActive(true);
        }
        else
        {
            Logger.SetLogViewVisible(false);
            Profiler.SetActive(false);
        }


        // Set the ready flag.
        isReady = true;
        Logger.Log("LensPlayer ready");

    }



    void SetStagingFlag()
    {
        string stagingFlagFile = LensPlayer.GetStoragePath() + "/sdcard/welens/data/staging";
        if (File.Exists(stagingFlagFile))
        {
            Logger.Logd("Staging server set.");
            useStaging = true;
        }
    }




    static public bool IsReady()
    {
        return isReady;
    }


    static public Platform.VideoData[] GetAvailableVideos()
    {
        return instance.videoData;
    }



    // Update is called once per frame
    void Update ()
    {

	}


    static bool isLoadingImageChecked = false;
    static public bool IsLoadingImageChecked()
    {
        return isLoadingImageChecked;
    }


    static public bool HasLoadingImage()
    {
        string filepath = LensPlayer.welensPath + "splash-logo.jpg";
        return File.Exists(filepath);
    }





    private IEnumerator InitLoadingImage()
    {
        Logger.Log("Loading splash image.");
        string filepath = "file://" + LensPlayer.welensPath + "splash-logo.jpg";
        WWW fileSplash = new WWW(filepath);
        yield return fileSplash;
        if (string.IsNullOrEmpty(fileSplash.error))
        {
            Logger.Log("\tSplash logo loaded.");
            loadingImage = Sprite.Create(fileSplash.texture, new Rect(0, 0, fileSplash.texture.width, fileSplash.texture.height), Vector2.zero);
        }
        else
        {
            Logger.Log("\tNo splash logo found.");
        }
        isLoadingImageChecked = true;
    }





    [Serializable]
    public class DeviceJSONData
    {
        public int status;
        public long expires;
    }



    IEnumerator LoadPlanData()
    {
        // If the device json file exists on disk, then read it.
        Logger.Log("Loading plan data from device JSON.");
        string deviceID = Platform.GetShortDeviceID();
        if (deviceID.Length > 4)
        {
            deviceID = deviceID.Substring(deviceID.Length - 4);
        }
        string dataFilename = LensPlayer.GetStoragePath() + "/sdcard/welens/data/" + deviceID + ".json";
        string jsonString = null;
        DeviceJSONData deviceData = null;
        if (System.IO.File.Exists(dataFilename))
        {
            Logger.Log("\tLoading device JSON from disk");
            StreamReader streamReader = new StreamReader(dataFilename);
            jsonString = streamReader.ReadToEnd();
            deviceData = JsonUtility.FromJson<DeviceJSONData>(jsonString);
            Logger.Log("\tDevice JSON: " + jsonString);

            if (null != deviceData)
            {
                // If the plan has expired, then delete the file.
                System.DateTime epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
                long timestamp = (long)(System.DateTime.UtcNow - epochStart).TotalSeconds;
                if (deviceData.expires < timestamp)
                {
                    Logger.Log("\tPlan expired. Deleting file.");
                    System.IO.File.Delete(dataFilename);
                    deviceData = null;
                }
                else
                {
                    LensPlayer.plan = JsonUtility.FromJson<serialisedPlan>(jsonString);
                    LensPlayer.plan.endDate = LensPlayer.UnixTimeStampToDateTimeSeconds(LensPlayer.plan.expires);
                    Logger.Log("\tPlan Status: " + LensPlayer.plan.status);
                    Logger.Log("\tPlan Expires: " + LensPlayer.plan.endDate);
                }
            }
            streamReader.Close();
        }
        else
        {
            string dir = dataFilename.Substring(0, dataFilename.LastIndexOf('/'));
            if (Directory.Exists(LensPlayer.welensPath))
            {
                Directory.CreateDirectory(dir);
            }
        }


        // If the device data was not obtained, then attempt to get it from the server.
        if (null == deviceData)
                {
            //                    Logger.Log("Loading device JSON from server with device ID: " + Platform.GetShortDeviceID());
            string url;
            if (useStaging)
            {
                url = "http://staging.welens.com/api/lenspass/device/" + Platform.GetShortDeviceID();
            }
            else
            {
                url = "http://www.welens.com/api/lenspass/device/" + Platform.GetShortDeviceID();
            }
            Logger.Log("Accessing URL: '" + url + "'");
                    WWW www = new WWW(url);
                    yield return www;
                    if ((www.error != null) && (www.error.Length > 0))
                    {
                        Logger.Log("Error requesting device data from server");
                        LensPlayer.plan = new serialisedPlan();
                        LensPlayer.plan.status = 0;
                        LensPlayer.plan.endDate = DateTime.Now;
                        yield break;
                    }
                    jsonString = www.text;

                    // Process the device data.
                    Logger.Log("Device data from server: " + jsonString);
                    string accountJson = JsonHelper.GetJsonObject(jsonString, "account");
                    if (null != accountJson)
            {
                LensPlayer.plan = JsonUtility.FromJson<serialisedPlan>(accountJson);
                LensPlayer.plan.endDate = LensPlayer.UnixTimeStampToDateTimeSeconds(LensPlayer.plan.expires);
                Logger.Log("\tPlan Status: " + LensPlayer.plan.status);
                Logger.Log("\tPlan Expires: " + LensPlayer.plan.endDate);

                // Save the file.
                PlanOut planout = new PlanOut();
                planout.status = LensPlayer.plan.status;
                planout.expires = LensPlayer.plan.expires;
                System.DateTime epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
                long timestamp = (long)(System.DateTime.UtcNow - epochStart).TotalSeconds;
                planout.created = timestamp;
                planout.source = "player";
                string outstr = JsonUtility.ToJson(planout);
                File.WriteAllText(dataFilename, outstr);
            }
            else
            {
                Logger.Log("Device data does not contain plan data.");
                LensPlayer.plan = new serialisedPlan();
                LensPlayer.plan.status = 0;
                LensPlayer.plan.endDate = DateTime.Now;
            }
        }
        yield return null;
    }










    private void LoadPlayerJSONFile()
    {
        Logger.Log("Loading Player JSON file.");
        // Read the player.json if it exists.
        string dataFilename = LensPlayer.GetStoragePath() + "/sdcard/welens/data/player.json";
        try
        {
			if (Directory.Exists(LensPlayer.welensPath))
			{
				StreamReader streamReader = new StreamReader(dataFilename);
				string jsonString = streamReader.ReadToEnd();
				LensPlayer.playerJSONData = JsonUtility.FromJson<PlayerJSONData>(jsonString);
				//            Logger.Log("\tplayer JSON: " + jsonString);
			}
		}
        catch (Exception e)
        {
            Logger.Log("\tFailed to load player.json. Reason: " + e.Message);
        }
    }


    static public void PlayButtonClickSound()
    {
        SoundEffects.PlaySoundEffect(instance.buttonClick);
    }


    static public void PlayToggleClickSound()
    {
        SoundEffects.PlaySoundEffect(instance.toggleClick);
    }



    static public void PlayRolloverSound()
    {
        SoundEffects.PlaySoundEffect(instance.buttonRollover);
    }


    void LoadConfiguration()
    {
        Logger.Log("Loading config.yaml");
        // Read the config from file or read the default config.
        TextReader input = null;
        string configFilename = LensPlayer.welensPath + "config.yaml";
        try
        {
            input = new StringReader(File.ReadAllText(configFilename));
        }
        catch (System.Exception e)
        {
            Logger.Log("\tconfig.yaml not read: " + e.Message);
            return;
        }

        // Load and parse the config.yaml.
        var yaml = new YamlStream();
        try
        {
            yaml.Load(input);
        }
        catch (System.Exception e)
        {
            Logger.Log(Logger.LogLevel.Error, "Error reading config.yaml: " + e.Message);
            return;
        }
        if (yaml.Documents.Count < 1)
        {
            return;
        }
        YamlMappingNode mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        ReadMapping(mapping);
    }

    void ReadMapping(YamlMappingNode mapping)
    {
        try { LensPlayer.configString = mapping.ToString(); Logger.Log("\tConfig: " + LensPlayer.configString); }
        catch (Exception) { LensPlayer.configString = "Configuration is empty"; }

        try { LensPlayer.demo = mapping.Children[(YamlScalarNode)"lpd"].ToString() == "true" ? true : false; Logger.Log("\tlpd set to " + (LensPlayer.demo ? "true" : "false")); }
        catch (Exception) { LensPlayer.demo = false; }

        try { LensPlayer.performanceWarning = mapping.Children[(YamlScalarNode)"performance_warning"].ToString() == "true" ? true : false; }
        catch (Exception) { LensPlayer.performanceWarning = false; }

        try { LensPlayer.theaterMode = mapping.Children[(YamlScalarNode)"theater_mode"].ToString() == "true" ? true : false; }
        catch (Exception) { LensPlayer.theaterMode = false; }

		    try { LensPlayer.basicFont = mapping.Children[(YamlScalarNode)"basic_font"].ToString() == "true" ? true : false; }
        catch (Exception) { LensPlayer.basicFont = false; }

        try { LensPlayer.developerMode = mapping.Children[(YamlScalarNode)"developer_mode"].ToString() == "true" ? true : false;
        Logger.Log("\tdeveloper mode set to " + (LensPlayer.developerMode ? "true" : "false")); }
        catch (Exception) { LensPlayer.developerMode = false; }

        try { LensPlayer.videoAutoStart = mapping.Children[(YamlScalarNode)"video_auto_start"].ToString() == "true" ? true : false; }
        catch (Exception) { LensPlayer.videoAutoStart = true; }

        try { LensPlayer.custom_message = mapping.Children[(YamlScalarNode)"custom_message"].ToString(); }
        catch (Exception) { LensPlayer.custom_message = ""; }

        try { LensPlayer.autoReset = mapping.Children[(YamlScalarNode)"auto_reset"].ToString() == "true" ? 20 : -1; }
        catch (Exception) { LensPlayer.autoReset = -1; }
        try { LensPlayer.autoReset = int.Parse(mapping.Children[(YamlScalarNode)"auto_reset"].ToString()); }
        catch (Exception) {  }

        try { LensPlayer.screeningScheduleMinutes = int.Parse(mapping.Children[(YamlScalarNode)"screening_schedule"].ToString()); }
        catch (Exception) { LensPlayer.screeningScheduleMinutes = -1; }

        try { LensPlayer.disableControls = mapping.Children[(YamlScalarNode)"disable_controls"].ToString() == "true" ? true : false; }
        catch (Exception) { LensPlayer.disableControls = false; }

        try { LensPlayer.screeningCountdownSeconds = int.Parse(mapping.Children[(YamlScalarNode)"screening_countdown"].ToString()); }
        catch (Exception) { LensPlayer.screeningCountdownSeconds = -1; }

        try { LensPlayer.trailerFilename = mapping.Children[(YamlScalarNode)"trailer_filename"].ToString(); }
        catch (Exception) { }

        try { LensPlayer.isStrictModeEnabled = mapping.Children[(YamlScalarNode)"sync_mode"].ToString() == "strict" ? true : false; }
        catch (Exception) { LensPlayer.isStrictModeEnabled = false; }

        try { LensPlayer.spatVolume = Convert.ToSingle(mapping.Children[(YamlScalarNode)"spatial_volume"].ToString()) / 100; Logger.Log("Spatial Volume set to " + LensPlayer.spatVolume.ToString("#.00")); }
        catch (Exception) { LensPlayer.spatVolume = 1.0f; }
        
        try { LensPlayer.headsUp = int.Parse(mapping.Children[(YamlScalarNode)"heads_up"].ToString()); }
        catch (Exception) { LensPlayer.headsUp = LensPlayer.headsUpDefault; }

        try { LensPlayer.theaterTitle = mapping.Children[(YamlScalarNode)"theater_title"].ToString(); }
		catch (Exception) { LensPlayer.theaterTitle = null; }

		try { LensPlayer.theaterTheme = mapping.Children[(YamlScalarNode)"theater_theme"].ToString(); }
		catch (Exception) { LensPlayer.theaterTheme = null; }

		try { LensPlayer.headsUpCountdown = mapping.Children[(YamlScalarNode)"heads_up_countdown"].ToString() == "true" ? true : false;  }
		catch (Exception) { LensPlayer.headsUpCountdown = false; }

		/*
                try {
                    LensPlayer.videoStartDelaySeconds = int.Parse(mapping.Children[(YamlScalarNode)"video_start_delay"].ToString());
                    if (LensPlayer.videoStartDelaySeconds < 0)
                    {
                        LensPlayer.videoStartDelaySeconds = 0;
                    }
                }
                catch (Exception) { LensPlayer.videoStartDelaySeconds = -1; }
        */

		// Check video list here.
		var deserializer = new DeserializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();

        YamlScalarNode n = new YamlScalarNode("video_list");

        // Do the check here now for empty or malformed configs
        if (mapping.Children.ContainsKey(n) && mapping.Children[(YamlScalarNode)"video_list"] != null)
        {
            try { YamlSequenceNode videoListTest = (YamlSequenceNode)mapping.Children[(YamlScalarNode)"video_list"]; }
            catch { GetEmpty(); }

            YamlSequenceNode videoList = (YamlSequenceNode)mapping.Children[(YamlScalarNode)"video_list"];

            // Differentiate between new and old YAML configs.
            if (!videoList.Children[0].ToString().Contains("video_filename"))
            {
                // New way to parse video files
                LensPlayer.videoList = new string[videoList.Children.Count];
                LensPlayer.stereoList = new string[videoList.Children.Count];
				LensPlayer.formatList = new string[videoList.Children.Count];
                for (int i = 0; i < videoList.Children.Count; ++i)
                {
                    var stream = (YamlMappingNode)videoList.Children[i];
                    Video video = new Video();
                    video = deserializer.Deserialize<Video>(new EventStreamParserAdapter(YamlNodeToEventStreamConverter.ConvertToEventStream(stream)));
                    if (video.stereo != null)
                    {
                        try { LensPlayer.stereoList[i] = video.stereo; }
                        catch (Exception) { LensPlayer.stereoList[i] = ""; }
                    }
                    else
                    {
                        try { LensPlayer.stereoList[i] = ""; }
                        catch (Exception) { LensPlayer.stereoList[i] = ""; }
                    }
					// Format stuff here
					if (video.format != null)
					{
						try { LensPlayer.formatList[i] = video.format; }
						catch (Exception) { LensPlayer.formatList[i] = ""; }
					}
					else
					{
						try { LensPlayer.formatList[i] = ""; }
						catch (Exception) { LensPlayer.formatList[i] = ""; }
					}
                    try { LensPlayer.videoList[i] = video.filename; }
                    catch (Exception) { LensPlayer.videoList[i] = "";  }
                }
             }

            else
            { 
                // Old way to parse video files
                LensPlayer.videoList = new string[videoList.Children.Count];
                for (int i = 0; i < videoList.Children.Count; ++i)
                {
                    YamlMappingNode videoData = (YamlMappingNode)videoList.Children[i];
                    try { LensPlayer.videoList[i] = videoData.Children[(YamlScalarNode)"video_filename"].ToString(); }
                    catch (Exception) { LensPlayer.videoList[i] = ""; }
				}
            }
        }
        else
        {
            GetEmpty();
        }

        /*
                try { LensPlayer.videoAutoStart = mapping.Children[(YamlScalarNode)"video_auto_start"].ToString() == "true" ? true : false; }
                catch (Exception) { LensPlayer.videoAutoStart = true; }


                try { LensPlayer.video_filename = mapping.Children[(YamlScalarNode)"video_filename"].ToString(); }
                catch (Exception) { LensPlayer.video_filename = ""; }



                try { LensPlayer.videoStartTime = DateTime.Parse(mapping.Children[(YamlScalarNode)"video_start_time"].ToString()); LensPlayer.videoStartTimeEnabled = true; }
                catch (Exception) { LensPlayer.videoStartTime = DateTime.Now; LensPlayer.videoStartTimeEnabled=LensPlayer.videoSchedule!=0; }

                //Debug.LogError(LensPlayer.videoStartTime);
                LensPlayer.configuration = new Configuration[1];
                LensPlayer.configuration[0] = new Configuration()
                {
                    videoFilename = LensPlayer.video_filename,
                    videoStartTime = LensPlayer.videoStartTime
                };
                if (System.String.IsNullOrEmpty(LensPlayer.video_filename))
                {
                    try
                    {
                        YamlScalarNode n = new YamlScalarNode("video_list");
                        if (mapping.Children.ContainsKey(n))
                        {
                            YamlSequenceNode videoList = (YamlSequenceNode)mapping.Children[(YamlScalarNode)"video_list"];
                            LensPlayer.configuration = new Configuration[videoList.Children.Count];
                            //LensPlayer.configuration = new System.Collections.Generic.List<Configuration>();
                            for (int i = 0; i < videoList.Children.Count; ++i)
                            {
                                YamlMappingNode videoData = (YamlMappingNode)videoList.Children[i];
                                LensPlayer.configuration[i] = new Configuration();

                                try { LensPlayer.configuration[i].videoFilename = videoData.Children[(YamlScalarNode)"video_filename"].ToString(); }
                                catch (Exception) { LensPlayer.configuration[i].videoFilename = ""; }

                                try { LensPlayer.configuration[i].videoStartTime = System.DateTime.Parse(videoData.Children[(YamlScalarNode)"video_start_time"].ToString()); }
                                catch (Exception) { LensPlayer.configuration[i].videoStartTime = DateTime.Now; }
                            }

                            try
                            { LensPlayer.video_filename = LensPlayer.configuration[0].videoFilename; }
                            catch (Exception)
                            { }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }


        */
        Logger.Log("\tConfig parsed.");
    }

    void GetEmpty()
    {
        DirectoryInfo dir = new DirectoryInfo(LensPlayer.welensPath);
        FileInfo[] videos = dir.GetFiles("*.mp4");

        LensPlayer.videoList = new string[videos.Length];
        for (int i = 0; i < videos.Length; i++)
        {
            try { LensPlayer.videoList[i] = videos[i].Name.ToString(); }
            catch (Exception) { LensPlayer.videoList[i] = ""; }
        }
    }
    


    static public void SetWatermarkVisible(bool isVisible)
    {
        instance.watermark.SetActive(isVisible);
    }


    static public void CheckHeadphoneJack()
    {
        instance.StartCoroutine(instance.CheckHeadphoneJackCoRoroutine());
    }

    IEnumerator CheckHeadphoneJackCoRoroutine()
    {
        Logger.Log("Checking headphones.");
        bool headJack = DetectHeadset.Detect();
        if (!headJack)
        {
            Logger.Log("\tHeadphones not found.");
            jackNotify.SetActive(true);
            yield return new WaitForSeconds(3f);
            jackNotify.SetActive(false);
        }
        else
        {
            Logger.Log("\tHeadphones found.");
            jackNotify.SetActive(false);
        }
    }

	static public void pathStatus()
	{
		instance.pathImage.SetActive(true);
	}


}
public class Video_List
{
    public List<Video> Videos;
}
[Serializable]
public class Video
{
    public string video { get; set; }
    public string filename { get; set; }
    public string format { get; set; }
    public string stereo { get; set; }
}
