#pragma warning disable 0649
using UnityEngine;



/**
 * This class is used with the Singleton Persistent GameObject to persist the GameObject throughtout application execution.
 * 
 **/
public class Persistent : MonoBehaviour
{


    #region PRIVATE VARIABLES

    static Persistent instance;


    #endregion






    #region PUBLIC METHODS


    static public bool Exists()
    {
        return (null != instance);
    }


    static public GameObject GetCameraContainer()
    {
        return instance.gameObject.transform.Find("Camera Container").gameObject;
    }


    #endregion





    #region MONOBEHAVIOR OVERRIDE METHODS

    void Awake()
    {
        DontDestroyOnLoad(this);
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Logger.Log("TOO MANY PERSISTENT GAME OBJECTS!");
            DestroyObject(gameObject);
        }

    }



    #endregion


}
