using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



/**
 * This class handles the HeadTracker prefab.
 * 
 * This class will update the transform of the associated GameObject to match the current head transform.
 * 
 **/
public class HeadTracker : MonoBehaviour
{


    /**
     * Updates the GameObject's transform to the head's transform.
     * 
     **/
	void LateUpdate ()
    {
        Transform headTransform = Head.Trans();
        if (null != headTransform)
        {
            transform.rotation = headTransform.rotation;
            transform.position = headTransform.position;
        }

    }
}
