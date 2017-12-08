using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RealtimeFeedback : MonoBehaviour
{
    static RealtimeFeedback instance;
    public Text feedbackText;


	// Use this for initialization
	void Start ()
    {
        instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public static void SetFeedback(string msg)
    {
        if (!instance)
        {
            return;
        }
        instance.feedbackText.text = msg;
    }
}
