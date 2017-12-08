using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;



/**
 * Overview:
 * This class sends all the playback data and log data on application quit, and has methods to hold the information. 
 * 
 * Description:
 * Logging during app execution is done via the static Logger.Log method. If the log level of the log message is a warning or an error,
 * then RecordLog is called. This method records the log to a list of logs.
 * 
 * In addition to logs, playback events are recorded. When a playback event occurs, such as playing a video or a video ending,
 * the occurrence of the event is recorded via a call to the static method PlaybackEventManager.RecordEvent. This method records the event to a list of playback events.
 * 
 * Upon application quit, Unity calls the OnApplicationQuit method for existing MonoBehaviour objects. The PlaybackEventManager
 * is attached to a persistent game object and is guaranteed to exist at all times, and as a result, has its OnApplicationQuit method called upon application quit.
 * This method calls the SendData method, which creates an object that contains the log list and the playback event list, along with other information, and sends it to the "log" API endpoint.
 * If the send fails, the log data is saved to a file.
 * 
 * Upon startup, this class checks for an existence of a log file and sends it to the log API and deletes if successful.
 * 
 **/
public class PlaybackEventManager : MonoBehaviour
{
    #region PRIVATE VARIABLES

    /// <summary>
    /// The list of playback event data.
    /// </summary>
    static PlaybackEventData playbackList = new PlaybackEventData();

    // XXX BAL: This variable doesn't seem necessary.
    /// <summary>
    /// Unix start timestamp.
    /// </summary>
    static System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

    /// <summary>
    /// The filename for the video for the last playback event. This is used solely for sending a filename to RecordEvent in OnApplicationQuit.
    /// </summary>
    static string videoFilename;

    /// <summary>
    /// The WWW object used for the final sending of data. It is marked static to keep it in scope while the send is being done.
    /// </summary>
    static WWW www;

	bool allowQuit;


    #endregion




    #region MONOBEHAVIOUR OVERRIDE METHODS


    /**
     * Called at the start of this object.
     * 
     * Stores an instance of the object and sends the data file generated from the previous execution, if one exists.
     * 
     **/
    void Start()
    {
		allowQuit = false;
        if (File.Exists(LensPlayer.welensPath + "data.json"))
        {
            SendSave();
    	}

		//        StartCoroutine(testfunc());
	}



    IEnumerator testfunc()
    {
        yield return new WaitForSeconds(5);

        SendData();
    }



    string CreateTestJSONMessage()
    {
        Logger.LogData logData = new Logger.LogData();
        for (int i=0; i<5; ++i)
        {
            Logger.LogEvent eventData = new Logger.LogEvent(i, "test", "test log " + i);
            logData.Logs.Add(eventData);
        }
        PlaybackEventData playbackEvents = new PlaybackEventData();
        for (int i=0; i<2; ++i)
        {
            PlaybackEvent eventData = new PlaybackEvent(i, "test", "test fn " + i);
            playbackEvents.playbackData.Add(eventData);
        }

        int battery = GetBatteryLevel();
        string version = Application.version;
        string device = Platform.GetShortDeviceID();
        string eventID = LensPlayer.eventID;
        Info infoJson = new Info(version, device, eventID, battery, logData, playbackEvents);
        string finalJson = JsonUtility.ToJson(infoJson);


        // Format the JSON string for the logs API.
        string playbackRemove = "},\"playbackData\":{";
        string logRemove = "\"Logs\":{";
        finalJson = finalJson.Replace(playbackRemove, "");
        finalJson = finalJson.Replace(logRemove, "");
        finalJson = finalJson.Remove(finalJson.Length - 2);
        finalJson = finalJson.Insert(finalJson.IndexOf("\"playbackData\""), ",");
        finalJson += "}";

        return finalJson;
    }







    /**
     * Called by Unity upon application quit.
     * 
     * If Sends the data to the log API.
     * 
     **/
    void OnApplicationQuit()
    {
        SendData();
		if (!allowQuit)
		{
			Application.CancelQuit();
		}
	}


    #endregion




    #region PUBLIC METHODS


    /**
     * Called to record a playback event.
     * 
     * A playback event object is created, initialized, and stored in the playback event list.
     * 
     **/
    public static void RecordEvent (string type, string videoFilename)
    {
        PlaybackEventManager.videoFilename = videoFilename;
        int timestamp = ((int)(System.DateTime.UtcNow - epochStart).TotalSeconds);
        string video = videoFilename;
        int battery = GetBatteryLevel();
        string version = Application.version;
        PlaybackEvent jsonString = new PlaybackEvent(timestamp, type, video);
        playbackList.playbackData.Add(jsonString);
    }


    #endregion




    #region PRIVATE METHODS


    /**
     * Sends the total log and event list to the logs API. If the data cannot be sent,
     * it is stored in a file in order to be sent on next execution.
     * 
     **/
    void SendData()
    {
        // Create an Info object, fill it with log and event data, then convert it to a JSON string.
        int battery = GetBatteryLevel();
        string version = Application.version;
        string device = Platform.GetShortDeviceID();
        string eventID = LensPlayer.eventID;
        Info infoJson = new Info(version, device, eventID, battery, Logger.GetLogData(), playbackList);
//      Info infoJson = new Info(version, device, eventID, battery, Logger.GetLogData(), playbackEvents);
        string finalJson = JsonUtility.ToJson(infoJson);


        // Format the JSON string for the logs API.
        string playbackRemove = "},\"playbackData\":{";
        string logRemove = "\"Logs\":{";
        finalJson = finalJson.Replace(playbackRemove, "");
        finalJson = finalJson.Replace(logRemove, "");
        finalJson = finalJson.Remove(finalJson.Length - 2);
        finalJson = finalJson.Insert(finalJson.IndexOf("\"playbackData\""), ",");
        finalJson += "}";


		// Send the formatted JSON string to the log API.
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("Content-Type", "application/json");
		byte[] body = Encoding.UTF8.GetBytes(finalJson);
		www = new WWW("http://www.welens.com/api/lenspass/log", body, headers);
        while (!www.isDone)
        {
        }
        if (www.error != null)
        {
            SaveData(finalJson);
        }
    }










    /**
     * Saves the event data to a file.
     * 
     **/
    void SaveData(string jsonData)
    {
        string path = LensPlayer.welensPath + "data.json";
		if (File.Exists(path))
        {
			StreamWriter writer = new StreamWriter(path, true);
			writer.WriteLine(jsonData);
			writer.Close();
		}
        else
        {
			File.Create(path).Close();
			File.WriteAllText(path, jsonData);
        }
    }









    /**
     * Sends the event data saved to file.
     * 
     **/
    void SendSave()
    {
        // Load the data file into a string.
        string path = LensPlayer.welensPath + "data.json";
		StreamReader reader = new StreamReader(path);
		
		while (!reader.EndOfStream)
		{
			string jsonString = reader.ReadLine();

			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("Content-Type", "application/json");
			byte[] body = Encoding.UTF8.GetBytes(jsonString);
			www = new WWW("http://www.welens.com/api/lenspass/log", body, headers);
			StartCoroutine(WaitForSave(jsonString));
		}
		

		reader.Close();
    }








    /**
     * Sends a string to the log API. If the send fails, the string is saved to file.
     * 
     * returns true if the send was successful, false if it failed.
     * 
     **/
    bool SendStringToLogAPI(string finalJson)
    {
        Logger.Logd("Sending log to server.");
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            byte[] body = Encoding.UTF8.GetBytes(finalJson);
            www = new WWW("http://www.welens.com/api/lenspass/log", body, headers);
			StartCoroutine(WaitForSend(finalJson));
            return true;
        }


        // If the data cannot be sent, record it to file.
        catch (UnityException ex)
        {
			// It never gets here.
            Debug.LogException(ex);
            Logger.Logd("Sending log FAILED.");
            return false;
        }
    }










    /**
     * Coroutine used to keep WWW object in scope while sending during quit.
     * 
     **/
    IEnumerator WaitForSend(string Json)
    {
		yield return www;
		if (www.error != null)
		{
			Logger.Logd("Sending log ERROR: " + www.error);
			Logger.Logd("Saving data to disk");
			SaveData(Json);
		}
		else
		{
			Logger.Logd("Deleting saved data");
			File.Delete(LensPlayer.welensPath + "data.json");

			Logger.Logd("Sending log DONE.");
			allowQuit = true;
		}
	}

	IEnumerator WaitForSave(string Json)
	{
		yield return www;
		if (www.error != null)
		{
			Logger.Logd("Sending log ERROR: " + www.error);
		}
		else
		{
			Logger.Logd("Deleting saved data");
			File.Delete(LensPlayer.welensPath + "data.json");

			Logger.Logd("Sending log DONE.");
			allowQuit = true;
		}
	}










	/**
     * Returns the battery level.
     * 
     **/
	static int GetBatteryLevel()
    {
        try
        {
            string CapacityString = System.IO.File.ReadAllText("/sys/class/power_supply/battery/capacity");
            return int.Parse(CapacityString);
        }
        catch
        {
            Debug.LogWarning("Failed to read battery power; Setting to 100.");
            return 100;
        }
    }



    #endregion




    #region DATA STRUCTURES


    /**
     * Data structure class that encapsulates the data sent to the log API.
     * 
     **/
    [System.Serializable]
    class Info
    {
        public string version;
        public string device;
        public string eventID;
        public int battery;
        public Logger.LogData Logs;
        public PlaybackEventData playbackData;


        /**
         * Constructor. Initializes data.
         * 
         **/
        public Info(string version, string device, string eventID, int battery, Logger.LogData logs, PlaybackEventData playbackData)
        {
            this.version = version;
            this.device = device;
            this.eventID = eventID;
            this.battery = battery;
            this.Logs = logs;
            this.playbackData = playbackData;
        }
    }









    /**
     * Data structure class for holding event information.
     * 
     **/
    [System.Serializable]
    class PlaybackEvent
    {
        public int timestamp;
        public string type;
        public string filename;


        /**
         * Constructor. Initializes values.
         * 
         **/
        public PlaybackEvent(int timestamp, string type, string filename)
        {
            this.timestamp = timestamp;
            this.type = type;
            this.filename = filename;
        }
    }









    /**
     * Data structure class that encapsulates the list of playback event data objects.
     * 
     * This is used in the Info class to make the Info class ready for JSON conversion.
     *       
     **/
    [System.Serializable]
    class PlaybackEventData
    {
        public List<PlaybackEvent> playbackData = new List<PlaybackEvent>();
    }
    

    #endregion


}

