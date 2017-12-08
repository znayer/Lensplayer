using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearVRControllerManager : MonoBehaviour
{
    [SerializeField]
    GameObject controller;

    [SerializeField]
    Transform leftControllerAnchor;

    [SerializeField]
    Transform rightControllerAnchor;


    OVRInput.Controller currentController = OVRInput.Controller.None;

    static GearVRControllerManager instance = null;


    // Use this for initialization
    void Start ()
    {
        instance = this;
	}



    static public OVRInput.Controller GetCurrentController()
    {
        if (null == instance)
        {
            return OVRInput.Controller.None;
        }
        return instance.currentController;
    }



    void Update ()
    {
        // If the right controller is conntected, then handle.
        if (OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote))
        {
            // If the right controller was not previously connected, then
            // handle.
            if (currentController != OVRInput.Controller.RTrackedRemote)
            {
                // Set the controller parent.
                controller.GetComponent<GearVRControllerPointer>().SetControllerType(OVRInput.Controller.RTrackedRemote);
                Vector3 pos = controller.transform.localPosition;
                controller.transform.parent = leftControllerAnchor;
                controller.transform.localPosition = pos;
                GazePointer.SetRootTransform(controller.transform.parent.transform);

                // Update the current controller value.
                currentController = OVRInput.Controller.RTrackedRemote;
            }
        }
        else if (OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote))
        {
            // If the left controller was not previously connected, then
            // handle.
            if (currentController != OVRInput.Controller.LTrackedRemote)
            {
                // Set the controller parent.
                controller.GetComponent<GearVRControllerPointer>().SetControllerType(OVRInput.Controller.LTrackedRemote);
                Vector3 pos = controller.transform.localPosition;
                controller.transform.parent = leftControllerAnchor;
                controller.transform.localPosition = pos;
                GazePointer.SetRootTransform(controller.transform.parent.transform);

                // Update the current controller value.
                currentController = OVRInput.Controller.LTrackedRemote;
            }
        }
        else
        {
            controller.SetActive(false);
            currentController = OVRInput.Controller.None;
            GazePointer.SetRootTransform(Head.Trans());
        }


        if (currentController != OVRInput.Controller.None)
        {
            if (GazeInput.IsPointerActive() && (controller.activeSelf == false))
            {
                controller.SetActive(true);
            }
            else if (!GazeInput.IsPointerActive() && (controller.activeSelf == true))
            {
                controller.SetActive(false);
            }
        }
    }
}
