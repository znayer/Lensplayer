using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VR;

public class GazeInput : PointerInputModule
{
    string leftClickName = "Fire1";
    string rightClickName = "Fire2";


    public GazePointer gazePointer;

    static GazeInput instance;


    private readonly MouseState m_MouseState = new MouseState();

    protected GazeInput()
    {
        instance = this;
    }

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
        if (!base.ShouldActivateModule())
            return false;

        if (m_ForceModuleActive)
            return true;

        return false;
    }


    public override void Process()
    {
        GazeControl();
    }


    protected static PointerEventData.FramePressState StateForButton(string buttonCode)
    {
        var pressed = Input.GetButtonDown(buttonCode);
        var released = Input.GetButtonUp(buttonCode);
        if (pressed && released)
            return PointerEventData.FramePressState.PressedAndReleased;
        if (pressed)
            return PointerEventData.FramePressState.Pressed;
        if (released)
            return PointerEventData.FramePressState.Released;
        return PointerEventData.FramePressState.NotChanged;
    }





    protected MouseState CreateGazePointerEvent(int id)
    {
        PointerEventData leftData;
        var created = GetPointerData(kMouseLeftId, out leftData, true);

        Vector2 pos = new Vector2(0, 0);
#if UNITY_EDITOR
        pos = new Vector2(Screen.width / 2f, Screen.height / 2f);
#else
        pos = new Vector2(VRSettings.eyeTextureWidth / 2, VRSettings.eyeTextureHeight / 2);
#endif
        Vector3 pt = Camera.main.WorldToScreenPoint(gazePointer.transform.position);
        //        pos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        pos.x = pt.x;
        pos.y = pt.y;


        leftData.Reset();
        leftData.delta = Vector2.zero;
        leftData.position = pos;
        leftData.scrollDelta = Vector2.zero;
        leftData.button = PointerEventData.InputButton.Left;

        eventSystem.RaycastAll(leftData, m_RaycastResultCache);
        var raycast = FindFirstRaycast(m_RaycastResultCache);


        // If a GameObject was found, then position the cursor at the raycast target.
        if (null != raycast.gameObject)
        {
            gazePointer.SetDistance(raycast.distance);
        }
        else
        {
            gazePointer.SetDistance(0);
        }



        leftData.pointerCurrentRaycast = raycast;
        m_RaycastResultCache.Clear();

        PointerEventData rightData;
        GetPointerData(kMouseRightId, out rightData, true);
        CopyFromTo(leftData, rightData);
        rightData.button = PointerEventData.InputButton.Right;

        m_MouseState.SetButtonState(
            PointerEventData.InputButton.Left,
            StateForButton(leftClickName),
            leftData);
        m_MouseState.SetButtonState(
            PointerEventData.InputButton.Right,
            StateForButton(rightClickName),
            rightData);

        return m_MouseState;
    }


    private void GazeControl()
    {
        var pointerData = CreateGazePointerEvent(0);

        var leftPressData = pointerData.GetButtonState(PointerEventData.InputButton.Left).eventData;

        ProcessPress(leftPressData.buttonData, leftPressData.PressedThisFrame(), leftPressData.ReleasedThisFrame());
        ProcessMove(leftPressData.buttonData);

        if (Input.GetButton(leftClickName))
        {
            ProcessDrag(leftPressData.buttonData);
        }
    }


    private void ProcessPress(PointerEventData pointerEvent, bool pressed, bool released)
    {
        var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

        if (pressed)
        {
            pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.dragging = false;
            pointerEvent.useDragThreshold = true;
            pointerEvent.pressPosition = pointerEvent.position;
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

            DeselectIfSelectionChanged(currentOverGo, pointerEvent);

            if (pointerEvent.pointerEnter != currentOverGo)
            {
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                pointerEvent.pointerEnter = currentOverGo;
            }

            var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

            if (newPressed == null)
            {
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
            }

            float time = Time.unscaledTime;

            if (newPressed == pointerEvent.lastPress)
            {
                var diffTime = time - pointerEvent.clickTime;
                if (diffTime < 0.3f)
                    ++pointerEvent.clickCount;
                else
                    pointerEvent.clickCount = 1;

                pointerEvent.clickTime = time;
            }
            else
            {
                pointerEvent.clickCount = 1;
            }

            pointerEvent.pointerPress = newPressed;
            pointerEvent.rawPointerPress = currentOverGo;

            pointerEvent.clickTime = time;

            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
        }


        if (released)
        {

            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
            {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
            else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }

            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

            pointerEvent.pointerDrag = null;

            ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
            pointerEvent.pointerEnter = null;
        }
    }

    public override void DeactivateModule()
    {
        base.DeactivateModule();
        ClearSelection();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Input: GazeInput");
        var pointerData = GetLastPointerEventData(kMouseLeftId);
        if (pointerData != null)
            sb.AppendLine(pointerData.ToString());

        return sb.ToString();
    }


    static public bool IsPointerActive()
    {
        if (!instance)
        {
            return false;
        }

        return instance.gazePointer.gameObject.activeSelf;
    }


    static public void SetPointerActive(bool isActive)
    {
        if (!instance)
        {
            Logger.Log(Logger.LogLevel.Error, "Attempting to set the pointer state before it is ready");
            return;
        }

        instance.gazePointer.gameObject.SetActive(isActive);
    }


    static public void SetGazePointer(GazePointer gazePointer)
    {
        instance.gazePointer = gazePointer;
    }

}
