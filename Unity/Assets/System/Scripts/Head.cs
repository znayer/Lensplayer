using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour
{

    static Head instance;



    // Use this for initialization
    void Start ()
    {
        instance = this;
	}
	
	// Update is called once per frame
	void Update ()
    {
        Camera mainCamera = Camera.main;
        transform.rotation = mainCamera.transform.rotation;
        transform.position = mainCamera.transform.position;

    }


    static public Transform Trans()
    {
        if (!instance)
        {
            return null;
        }
        return instance.transform;
    }

}
