
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorGazeController : MonoBehaviour
{

    [Tooltip("Camera.")]
    [SerializeField]
    GameObject cam = null;


    #region PRIVATE VARIABLES

    // Mouse sensitivity.  Increase the number to get more movement per mouse move.
    float mouseSensitvity = 0.5f;

    #endregion

    void Awake()
    {
#if !UNITY_EDITOR
        DestroyObject(gameObject);
#endif
    }



    IEnumerator Start()
    {
/*
        yield return new WaitForEndOfFrame();
        StandaloneInputModule standaloneInputModule = GetComponent<StandaloneInputModule>();
        if (standaloneInputModule.ShouldActivateModule())
        {
            Logger.Log("Editor Gaze Controller disabled.");
            enabled = false;
        }
        else
        {
            Logger.Log("Editor Gaze Controller enabled.");
        }
*/
        yield break;
    }



    void Update()
    {
    }


    void OnGUI()
    {
        // If the mouse was right click dragged, then update the camera rotation.
        if ((Event.current.type == EventType.MouseDrag) && (Event.current.button == 1) && (null != cam))
        {
            Vector2 d = Event.current.delta;
            Vector3 rot = cam.transform.rotation.eulerAngles;
            rot.y += d.x * mouseSensitvity;
            rot.x += d.y * mouseSensitvity;
            cam.transform.rotation = Quaternion.Euler(rot);
        }


        /*
                // Store the change in mouse position.
                Vector2 d = Event.current.delta;
                SetAxisDelta(AxisHorizontal, d.y);
                SetAxisDelta(AxisVertical, d.x);


                // If the event was a drag, then set the drag flag.
                if (Event.current.type == EventType.MouseDrag)
                {
                    mouseDragFlag = true;
                }
                else
                {
                    mouseDragFlag = false;
                }
        */
    }




    /*
            [SerializeField]
            private bool m_ForceModuleActive;

            public bool forceModuleActive
            {
                get { return m_ForceModuleActive; }
                set { m_ForceModuleActive = value; }
            }

            public override bool IsModuleSupported()
            {
                return forceModuleActive;
            }

            public override bool ShouldActivateModule()
            {
                return PointerInputModule.ShouldActivateModule();
            }
    */
}

