using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartVideoReceiver : MonoBehaviour
{
    static string startVideoFilePath = null;


    static public string GetStartVideoPath()
    {
        return startVideoFilePath;
    }


    public void SetStartVideo(string vidPath)
    {
        startVideoFilePath = vidPath;
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


}
