using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentApp : MonoBehaviour
{

    static PersistentApp instance;

    void Awake()
    {
        DontDestroyOnLoad(this);
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Logger.Log("TOO MANY PERSISTENT APP GAME OBJECTS!");
            DestroyObject(gameObject);
        }

    }


    static public PersistentApp Instance()
    {
        return instance;
    }
}
