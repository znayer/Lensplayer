using System;
using System.IO;
using UnityEngine;


/**
 * This class provides common functionality via static members.
 * 
 **/
public class Utils
{

    /**
     * Returns the root directory, depending on the platform.
     * 
     **/
    static public string GetRootDirectory()
    {
#if UNITY_EDITOR
        return "C:" + Path.DirectorySeparatorChar + "sdcard" + Path.DirectorySeparatorChar;
#else
        return Path.DirectorySeparatorChar + "sdcard" + Path.DirectorySeparatorChar;
#endif
    }



    static public string FilePathToFilename(string filePath)
    {
        string fn = filePath.Replace('\\', '/');
        fn = fn.Substring(fn.LastIndexOf('/') + 1);
        fn = fn.Substring(0, fn.IndexOf('.'));

        return fn;
    }




    public static T[] getJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        JSONArrayWrapper<T> wrapper = JsonUtility.FromJson<JSONArrayWrapper<T>>(newJson);
        return wrapper.array;
    }


    [Serializable]
    private class JSONArrayWrapper<T>
    {
        public T[] array = null;
    }



}
