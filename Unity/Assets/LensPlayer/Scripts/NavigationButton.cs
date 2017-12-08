using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/**
 * This class handles a navigation button in the video select scene.
 * 
 **/
public class NavigationButton : MonoBehaviour
{

    public GameObject text;


    Image image;



    #region MONOBEHAVIOUR OVERRIDE METHODS

    /**
     * Performs startup tasks.
     * 
     * Stores a reference to the button image.
     * 
     **/
    void Start ()
    {
        image = GetComponent<Image>();	
	}

    #endregion




    #region PUBLIC METHODS

    /**
     * Callback when the pointer enters the navigation button.
     * 
     **/
    public void OnButtonEnter()
    {
        text.SetActive(true);
        image.color = Color.white;
    }










    /**
     * Callback when the pointer enters the navigation button.
     * 
     **/
    public void OnButtonExit()
    {
        text.SetActive(false);
        image.color = new Color(1, 1, 1, 0);
    }

#endregion

}
