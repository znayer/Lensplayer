using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrientationResetter : MonoBehaviour
{
    public GameObject resetOrientationText;
    public GameObject resetOrientationTextBackground;
    public GameObject resetOrientationArrows;

    public Animation fadeArrowsInAnimation;

    public Material arrowsMaterial;

    float arrowsAlpha = 0;


    bool isFadingOut = false;

    float fadeFactor = 8.0f;


    static OrientationResetter instance;

    bool isDeactivating = false;


    bool isHorizontal = false;
    bool isHorizontalAllowed = false;


    static public void SetHorizontalAllowed(bool isAllowed)
    {
        instance.isHorizontalAllowed = isAllowed;
    }


    static public void SetActive(bool isActive)
    {
        // If setting to active, then set the ame object as active.
        if (isActive)
        {
            instance.gameObject.SetActive(true);
        }

        // Otherwise, setting to inactive.
        // If the alpha has reached zero, then set the game object
        // as inactive.
        // Otherwise, set the deactivating flag, and the fading out flag.
        else
        {

            if (instance.arrowsAlpha == 0)
            {
                instance.gameObject.SetActive(false);
            }
            else
            {
                instance.isDeactivating = true;
                instance.isFadingOut = true;
            }
        }
    }








    static public bool IsActive()
    {
        return instance.gameObject.activeSelf;
    }

    void Start()
    {
        instance = this;
    }

    void Update()
    {

        Transform headTransform = Head.Trans();
        if (resetOrientationText.activeSelf && (arrowsAlpha > 0.0f))
        {
			// Sets page back to video info
			var pagedRect = GameObject.FindObjectOfType<UI.Pagination.PagedRect>();
			pagedRect.SetCurrentPage(1);

			if (Input.GetMouseButtonDown(0))
            {
                UnityEngine.VR.InputTracking.Recenter();
                GameObject go = Persistent.GetCameraContainer();
                if (isHorizontalAllowed)
                {
                    if (!isHorizontal)
                    {
                        if ((headTransform.eulerAngles.x > 270) && (headTransform.eulerAngles.x < 315))
                        {
                            isHorizontal = true;
                            go.transform.localRotation = Quaternion.Euler(90, 0, 0);
                        }
                    }
                    else
                    {
                        if ((headTransform.eulerAngles.x > 45) && (headTransform.eulerAngles.x < 90))
                        {
                            isHorizontal = false;
                            go.transform.localRotation = Quaternion.identity;
                        }
                    }
                }
                Logger.Log("OrientationResetter recentering");
            }
        }

        if (null != headTransform)
        {
            float angleY = headTransform.eulerAngles.y;
            if (angleY > 180)
            {
                angleY -= 360;
            }
            angleY = Mathf.Abs(angleY);

            // If the angle is greater than 90 and either the text is not displayed or it is fading out, then fade in the display.
            bool show = false;
            if (!isHorizontal)
            {
                if (angleY > 90)
                {
                    show = true;
                }
                else if ((headTransform.eulerAngles.x > 270) && (headTransform.eulerAngles.x < 315) && isHorizontalAllowed)
                {
                    show = true;
                }
            }
            else
            {

                if ((headTransform.eulerAngles.x > 45) && (headTransform.eulerAngles.x < 90))
                {
                    show = true;
                }
                else if ((headTransform.eulerAngles.z > 15f) && (headTransform.eulerAngles.z < 345f))
                {
                    show = true;
                }
/*
                if (angleY > 90)
                {
                    show = true;
                }

                else if ((headTransform.eulerAngles.x > 270) && (headTransform.eulerAngles.x < 315))
                {
                    show = true;
                }
*/
            }


            if ((!resetOrientationText.activeSelf) && show)
            {
                //  Set the components as active.
                    resetOrientationText.SetActive(true);
                    resetOrientationTextBackground.SetActive(true);
                if (!isHorizontal)
                {
                    resetOrientationArrows.SetActive(true);
                }


                // Set the start color as transparent for the components.
                arrowsAlpha = 0;
                Color c = arrowsMaterial.color;
                c.a = arrowsAlpha * 0.5f;
                arrowsMaterial.color = c;
                Color c2 = new Color(0, 0, 0, arrowsAlpha * 0.5f);
                resetOrientationText.GetComponent<Text>().color = new Color(1, 1, 1, arrowsAlpha);
                resetOrientationTextBackground.GetComponent<Image>().color = c2;


/*
                    resetOrientationText.GetComponent<Animation>().Play("FadeIn");
                    resetOrientationTextBackground.GetComponent<Animation>().Play("FadeIn");
                    resetOrientationArrows.GetComponent<Animation>().Play("FadeIn");
*/
            }


            // If the display is active, then either fade it in or out.
            else if (resetOrientationText.activeSelf)
            {
                if (!isHorizontal)
                {
                    if ((headTransform.eulerAngles.x > 270) && (headTransform.eulerAngles.x < 315))
                    {
                        show = true;
                    }
                    else if (angleY < 45)
                    {
                        show = false;
                    }
                }
                else
                {
/*
                    if ((headTransform.eulerAngles.x > 270) && (headTransform.eulerAngles.x < 315))
                    {
                        show = true;
                    }
                    else if (angleY < 45)
                    {
                        show = false;
                    }
*/
                }


                if (isFadingOut)
                {
                    arrowsAlpha -= Time.deltaTime * fadeFactor;
                    if (arrowsAlpha < 0.0f)
                    {
                        arrowsAlpha = 0.0f;
                        isFadingOut = false;
                        resetOrientationText.SetActive(false);
                        resetOrientationTextBackground.SetActive(false);
                        resetOrientationArrows.SetActive(false);
                        if (isDeactivating)
                        {
                            isDeactivating = false;
                            gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        Color c2 = new Color(0, 0, 0, arrowsAlpha*0.5f);
                        Color c = arrowsMaterial.color;
                        c.a = arrowsAlpha * 0.5f;
                        arrowsMaterial.color = c;
                        resetOrientationText.GetComponent<Text>().color = new Color(1, 1, 1, arrowsAlpha);
                        resetOrientationTextBackground.GetComponent<Image>().color = c2;

                    }
                }
                else if (!show)
                {
                    isFadingOut = true;
                }
                else if (arrowsAlpha != 1.0f)
                {
                    arrowsAlpha += Time.deltaTime * fadeFactor;
                    if (arrowsAlpha > 1.0f)
                    {
                        arrowsAlpha = 1.0f;
                    }
                    Color c2 = new Color(0, 0, 0, arrowsAlpha * 0.5f);
                    Color c = arrowsMaterial.color;
                    c.a = arrowsAlpha * 0.5f;
                    arrowsMaterial.color = c;
                    resetOrientationText.GetComponent<Text>().color = new Color(1, 1, 1, arrowsAlpha);
                    resetOrientationTextBackground.GetComponent<Image>().color = c2;
                }
            }



        }

    }
}
