using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GazeButtonEvents : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerDown(PointerEventData data)
    {
        GazeBasedInteractionControl.HoveredButton = null;
    }

    public void OnPointerUp(PointerEventData data)
    {

    }

    public void OnPointerEnter(PointerEventData data)
    {
        GazeBasedInteractionControl.HoveredButton = null;
        GazeBasedInteractionControl.HoveredButton = GetComponent<Button>();
    }

    public void OnPointerExit(PointerEventData data)
    {
        GazeBasedInteractionControl.HoveredButton = null;
    }
}
