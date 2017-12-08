using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GazeBasedInteractionControl : MonoBehaviour {

    public float gazeTime = 3;
    public float gazeTimmer = 3;
    public Image progressFill;
    private Button hoveredButton = null;
    private Button prevHoveredButton = null;

    bool isTimedInputActive = true;

    static GazeBasedInteractionControl instance;

    public static Button HoveredButton { get
        {
            if (null == instance)
            {
                return null;
            }
            return instance.hoveredButton;
        }
        set
        {
            if (null == instance)
            {
                return;
            }
            instance.hoveredButton = value;
        } }

    public static Button PreviousHoveredButton { get { return instance.prevHoveredButton; } set { instance.prevHoveredButton = value; } }

    public static void SetTimedInputActive(bool isActive) { instance.isTimedInputActive = isActive; }
    public static bool GetTimedInputActive() { return instance.isTimedInputActive; }

    void Awake()
    {
        instance = this;
    }

    float startTime;


    float startInactivePeriod = 0.0f;



    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update ()
    {
        if (HoveredButton)
        {
            if (PreviousHoveredButton != HoveredButton)
            {
                PreviousHoveredButton = HoveredButton;
                gazeTimmer = 0;
            }
            if ((isTimedInputActive == false) || (Time.time - startTime < startInactivePeriod) || (GearVRControllerManager.GetCurrentController() != OVRInput.Controller.None))
            {
                return;
            }
            if (gazeTimmer > gazeTime)
            {
                HoveredButton.onClick.Invoke();
                hoveredButton = null;
            }
            else gazeTimmer += Time.deltaTime;
        }
        else
        {
            gazeTimmer = 0;
        }
        progressFill.fillAmount = gazeTimmer/gazeTime;
	}



    static public void SetEnabled(bool enabled)
    {
        instance.isTimedInputActive = enabled;
    }


}
