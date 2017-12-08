#pragma warning disable 0649
using UnityEngine;



/**
 * Creates the persistent game object if it does not already exist.
 * 
 **/
public class SceneController : MonoBehaviour
{

    #region EDITOR INTERFACE VARIABLES

    [Tooltip("System persistent prefab")]
    [SerializeField]
    GameObject persistentPrefab;

    [Tooltip("App persistent prefab")]
    [SerializeField]
    GameObject appPersistentPrefab;

    #endregion





    #region MONO BEHAVOR OVERRIDE METHODS

    /**
     * Creates the persistent game object if it does not already exist.
     * 
     **/
    void Start ()
    {
        if (!Persistent.Exists())
        {
            GameObject.Instantiate(persistentPrefab);
        }
        if (PersistentApp.Instance() == null)
        {
            GameObject.Instantiate(appPersistentPrefab);
        }
    }

    #endregion

}
