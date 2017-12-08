#pragma warning disable 0649
using StatsMonitor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.UI;




/**
 * This class provides logging functionality. Functionality is exposed via the static public method "Log".
 * 
 * Log messages are written to a log file and displayed in a log view. The class gets the log view display text and container from the editor.
 * 
 **/
public class Logger : MonoBehaviour
{


    #region EDITOR INTERFACE VARIABLES

    [Tooltip("Log display canvas")]
    [SerializeField]
    Canvas canvas;


    [Tooltip("Scroll view content container")]
    [SerializeField]
    GameObject content;


    [Tooltip("Display text")]
    [SerializeField]
    Text displayText;

    [Tooltip("Logger active")]
    [SerializeField]
    bool loggerActive = false;


    [Tooltip("Logger scroll rect")]
    [SerializeField]
    ScrollRect scrollRect;


    [SerializeField]
    LogLevel currentLogLevel = LogLevel.Debug;
    #endregion





    #region PRIVATE VARIABLES

    // The Singleton instance of this class.
    static Logger instance;


    // Holds all log messages issued.
    static string totalLog = "";

    // Line height.
    float offsetY;

    // Number of lines.
    static int numLines = 0;

    private LogData logList = new LogData();

    static System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);


    #endregion





    #region PUBLIC METHODS

    static public void Log(string msg)
    {
        Log(LogLevel.Verbose, msg);
    }


    static bool hasLoggedDeviceID = false;



    static public bool IsReady()
    {
        return instance != null;
    }


    static public void Logd(string msg)
    {
        Log(LogLevel.Debug, msg);
    }



        /**
         * Logs a message.
         * 
         **/
        static public void Log(LogLevel level, string msg)
    {
        if (!hasLoggedDeviceID && Platform.IsReady())
        {
            hasLoggedDeviceID = true;
            string startLogMessage = "DEVICE ID: " + Platform.GetShortDeviceID();
            Log(startLogMessage);
//            string fn = GetDebugLogPath() + GetDebugLogFilename();
//            File.AppendAllText(fn, startLogMessage);
        }
        // If the message log level is lower than the class log level, then do nothing.
        if ((instance != null) && (level < instance.currentLogLevel))
        {
            return;
        }


        // Add the datetime along with the type the log message to be written.
        string logMsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssz") + " ";
        switch (level)
        {
            case LogLevel.Verbose:
                logMsg += "(Verbose)";
                break;
            case LogLevel.Debug:
                logMsg += "(Debug)";
                break;
            case LogLevel.Warning:
                logMsg += "(Warning)";
                RecordLog("warning", msg);
                break;
            case LogLevel.Error:
                logMsg += "(ERROR)";
                RecordLog("error", msg);
                break;
            default:
                logMsg += "(?)";
                RecordLog("?", msg);
                break;
        }
        logMsg += "\t" + msg;


        // Ensure it ends with a newline.
        if (!logMsg.EndsWith("\n"))
        {
            logMsg += "\n";
        }


        // Add the line to the total log and to the log file.
        totalLog += logMsg;
        if (Directory.Exists(GetDebugLogPath()))
        {
            File.AppendAllText(GetDebugLogPath() + GetDebugLogFilename(), logMsg);
        }


        // Write to Unity Editor
        Debug.Log(logMsg);

        
        // Increment the number of lines.
        ++numLines;


        // Update the log view text and container size.
        if (null != instance)
        {
            string logstr;
            if (totalLog.Length > 2400)
            {
                logstr = totalLog.Substring(totalLog.Length - 2400);
            }
            else
            {
                logstr = totalLog;
            }
            instance.displayText.text = logstr;
            RectTransform rt = instance.content.GetComponent<RectTransform>();
            Vector2 sizeDelta = rt.sizeDelta;
            sizeDelta.y = numLines * instance.offsetY;
            rt.sizeDelta = sizeDelta;
//            instance.scrollRect.verticalNormalizedPosition = 1.0f;
        }
    }




    static public void SetLogViewVisible(bool visible)
    {
        if (null == instance)
        {
            return;
        }

        instance.loggerActive = visible;
        instance.canvas.gameObject.SetActive(visible);
    }



    #endregion





    #region MONOBEHAVIOR OVERRIDE METHODS


    void Update()
    {
        transform.position = Head.Trans().position;        
    }

    /**
     * Initializes variables and sets the uncaught exception handler.
     * 
     **/
    void Start()
    {
		// Initialize variables.
		RectTransform rt = displayText.GetComponent<RectTransform>();
        offsetY = rt.sizeDelta.y;


        // Verify directory.
        string debugLogPath = GetDebugLogPath();
        if (!Directory.Exists(debugLogPath))
        {
			if (!Directory.Exists(LensPlayer.welensPath))
			{
				Debug.Log("The real test has happened");
				LensPlayer.isInstalled = false;
			}
            else
            {
                Directory.CreateDirectory(debugLogPath);
            }
        }

		// Log the start message.
		instance = this;
        LogStartMessage();


        // Add the uncaught exception handler.
        Application.logMessageReceived += LogUnhandledException;

        SetLogViewVisible(loggerActive);

        Log(LogLevel.Debug, "-------------------------------------------");
        Log(LogLevel.Debug, "Logger Started");
        Log(LogLevel.Debug, "Version Number: " + Application.version);

    }






    void OnApplicationQuit()
    {
//        Logger.Log("FPS History: " + StatsMonitorWidget.GetFPSHistory());

#if !UNITY_EDITOR
        SendText("Log", totalLog);
#endif
    }



    static WWW www;

    static public bool noSend = false;

    static public void SendText(string type, string msg, string eventType = null)
    {
        if (noSend)
        {
            return;
        }

        // Grab some current data first
        // TODO: On 5.6 change, battery to use SystemInfo
        string video = LensPlayer.currentVideoFilename;
        string device = Platform.GetShortDeviceID();
        float battery = 0;
        string version = Application.version;
        JSON jsonString = new JSON(type, eventType, msg, video, device, battery, version);

        // If the number of requests is less than the throttle max, proceed
        try
        {
            // Write the JSON
            string sendJson = JsonUtility.ToJson(jsonString);

            // Send the data
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            byte[] body = Encoding.UTF8.GetBytes(sendJson);
            www = new WWW("http://www.welens.com/api/lenspass/log", body, headers);

            // Wait for this to go through
            instance.StartCoroutine(WaitForSend(www));
        }
        catch (UnityException ex)
        {
            Debug.Log(ex.Message);
        }
    }

    // Record a log event that's not a debug
    public static void RecordLog(string type, string message)
    {
        int timestamp = ((int)(System.DateTime.UtcNow - epochStart).TotalSeconds);


        LogEvent jsonString = new LogEvent(timestamp, type, message);


        instance.logList.Logs.Add(jsonString);
    }



    // Timer for sending
    static IEnumerator WaitForSend(WWW www)
    {
        yield return www;
    }

    static public LogData GetLogData()
    {
        return instance.logList;
    }





    #endregion





    #region PRIVATE METHODS



    /**
     * Logs a message indicating application start.
     * 
     **/
    void LogStartMessage()
    {
        string startLogMessage = "\n\n**************************************************\n\tAPPLICATION START " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssz") + "\n**************************************************\n";
        string fn = GetDebugLogPath() + GetDebugLogFilename();
        if (Directory.Exists(GetDebugLogPath()))
        {
            File.AppendAllText(fn, startLogMessage);
        }

        instance.displayText.text = totalLog;
        RectTransform rt = instance.content.GetComponent<RectTransform>();
        Vector2 sizeDelta = rt.sizeDelta;
        sizeDelta.y = numLines * instance.offsetY;
        rt.sizeDelta = sizeDelta;
    }





    static string GetDebugLogPath()
    {
        //        string outfilename = Utils.GetRootDirectory() + "welens" + Path.DirectorySeparatorChar + "player-log" + Path.DirectorySeparatorChar;
        string outfilename = Utils.GetRootDirectory() + "welens" + Path.DirectorySeparatorChar + "player-log" + Path.DirectorySeparatorChar;

        return outfilename;
    }





    /**
     * Returns the name of the debug log filename to be written to.
     * 
     **/
    static string GetDebugLogFilename()
    {
        string outfilename = "debug" + DateTime.Now.ToString("yy.MM.dd") + ".txt";

        return outfilename;
    }




    /**
     * Uncaught exception message handler.
     * 
     **/
    void LogUnhandledException(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Log)
        {
            // Nothing
        }
        else if (type == LogType.Exception)
        {
            Log(LogLevel.Error, "Unhandled Exception: " + logString + "\n" + stackTrace);
        }
        else if (type == LogType.Error)
        {
            Log(LogLevel.Error, "Unhandled Exception: " + logString + "\n" + stackTrace);
        }
        else if (type == LogType.Warning)
        {
            Log(LogLevel.Warning, "Unhandled Exception: " + logString + "\n" + stackTrace);
        }
    }



    static void SendMail(string aFrom, string aTo, string aSubject, string aBody, string aPassword)
    {
        if (!aTo.Contains("@") && !aTo.ToLower().Contains(".com"))
            return;

        MailMessage mail = new MailMessage();

        mail.From = new MailAddress(aFrom);
        mail.To.Add(aTo);
        mail.Subject = aSubject;
        mail.Body = aBody;

        SmtpClient smtpServer = new SmtpClient();
        smtpServer.Host = "smtp.gmail.com";
        smtpServer.Port = 587;
        smtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpServer.Credentials = new System.Net.NetworkCredential(aFrom, aPassword) as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        smtpServer.Send(mail);
    }




#endregion





#region DATA STRUCTURES


    // Log levels, in increasing importance.
    public enum LogLevel
    {
        Verbose,
        Debug,
        Warning,
        Error
    };


    #endregion


    [System.Serializable]
    public class JSON
    {
        public string type;
        public string eventType;
        public string message;
        public string video;
        public string device;
        public float battery;
        public string version;

        public JSON(string type, string eventType, string message, string video, string device, float battery, string version)
        {
            this.type = type;
            this.eventType = eventType;
            this.message = message;
            this.video = video;
            this.device = device;
            this.battery = battery;
            this.version = version;
        }
    }

    [System.Serializable]
    public class LogEvent
    {
        public int timestamp;
        public string type;
        public string message;

        public LogEvent(int timestamp, string type, string message)
        {
            this.timestamp = timestamp;
            this.type = type;
            this.message = message;
        }
    }
    [System.Serializable]
    public class LogData
    {
        public List<LogEvent> Logs = new List<LogEvent>();
    }


}
