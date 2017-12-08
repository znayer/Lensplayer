using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class GearVRControllerPointer : MonoBehaviour {
    
    public GameObject pointer;                  // Visual marker for what is being pointed at with the controller

    private LineRenderer lineRenderer;
    private RaycastHit hitInfo;
    private float maxRaycastDistance = 5;
    private float lineRendererLength = .25f;


    OVRInput.Controller controllerType = OVRInput.Controller.LTrackedRemote;


    public void SetControllerType(OVRInput.Controller controllerType)
    {
        this.controllerType = controllerType;
    }


    void Start ()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;                  // Unity 5.5.1 : lineRenderer.numPositions = 2;
                                                        // Unity 5.6   : lineRenderer.positionCount = 2;
    }

    void Update ()
    {
        //If this is not being tracked, hide and disable
        bool isTracking = gameObject.activeSelf;
        lineRenderer.enabled = isTracking;
        if (null != pointer)
        {
            pointer.SetActive(isTracking);
        }
        if (!isTracking)
        {
            return;
        }

        //Show and point with the controller
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + transform.forward * lineRendererLength);

        if (null != pointer)
        {
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, maxRaycastDistance))
            {
                pointer.transform.position = hitInfo.point;
            }
            else
            {
                pointer.transform.position = transform.position + transform.forward * maxRaycastDistance;
            }
        }

        Quaternion rotation = OVRInput.GetLocalControllerRotation(controllerType);
        transform.parent.rotation = rotation;
    }
}
