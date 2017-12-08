using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{

    static AsyncOperation asyncOperation = null;


    static public bool IsLoading()
    {
        if (null == asyncOperation)
        {
            return false;
        }
        return !asyncOperation.isDone || !asyncOperation.allowSceneActivation;
    }

    static public void Load(string sceneName, bool activateOnLoad=true)
    {
        SceneFader.FadeIn();
        asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = activateOnLoad;
    }






    static public bool IsSceneLoaded()
    {
        if (null == asyncOperation)
        {
            return false;
        }

        if (asyncOperation.allowSceneActivation == false)
        {
            if (asyncOperation.progress == 0.9f)
            {
                return true;
            }
            return false;
        }
        else
        {
            return asyncOperation.isDone;
        }
    }







    static public void ActivateLoadedScene()
    {
        asyncOperation.allowSceneActivation = true;
        asyncOperation = null;
    }







}
