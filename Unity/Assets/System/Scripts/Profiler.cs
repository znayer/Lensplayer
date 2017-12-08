using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Profiler : MonoBehaviour
{
    #region EDITOR INTERFACE VARIABLES



    [Tooltip("Stats Monitor Prefab")]
    [SerializeField]
    GameObject statsMonitorPrefab = null;

    [Tooltip("Stats Monitor active")]
    [SerializeField]
    bool statsMonitorActive = true;


    #endregion


    #region PRIVATE VARIABLES

    // The Singleton instance of this class.
    static Profiler instance;

    GameObject statsMonitor;

    static bool useOverrideStartValue = false;
    static bool overrideStartValue = false;

    #endregion



    #region PUBLIC METHODS


    public static void SetActive(bool isActive)
    {
        if (null == instance)
        {
            useOverrideStartValue = true;
            useOverrideStartValue = isActive; 
        }
        else
        {
            if (null == instance.statsMonitor)
            {
                return;
            }
            instance.statsMonitorActive = isActive;
            instance.statsMonitor.SetActive(isActive);
            if (isActive)
            {
                Vector3 pos = new Vector3(2, 0.5f, 2);
                Quaternion rot = Quaternion.Euler(0, 45, 0);
                instance.statsMonitor.transform.position = pos;
                instance.statsMonitor.transform.rotation = rot;
            }
        }
    }


    #endregion





    #region MONOBEHAVIOR OVERRIDE METHODS

    void Awake()
    {
        if (null != instance)
        {
            return;
        }


        instance = this;

        if (null == statsMonitorPrefab)
        {
            return;
        }
        statsMonitor = GameObject.Find("Stats Monitor");
        if (null == statsMonitor)
        {
            statsMonitor = GameObject.Instantiate(statsMonitorPrefab);
        }
        if (useOverrideStartValue)
        {
            SetActive(overrideStartValue);
        }
        else
        {
            SetActive(statsMonitorActive);
        }
    }



    void LateUpdate()
    {
        if (null == statsMonitor)
        {
            return;
        }
        Vector3 offset = new Vector3(3, 0, 3);
        statsMonitor.transform.position = Head.Trans().position + offset;
    }

    #endregion




}

