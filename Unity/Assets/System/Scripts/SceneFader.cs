using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{

    #region EDITOR INTERFACE VARIABLES

    [Tooltip("Render texture")]
    [SerializeField]
    RawImage renderTexture = null;



    [Tooltip("Blackout image")]
    [SerializeField]
    Image blackout = null;


    [Tooltip("display image")]
    [SerializeField]
    Image displayImage = null;


    [SerializeField]
    GameObject content = null;

    [SerializeField]
    float fadeDurationSec = 0.25f;



    #endregion


    State currentState = State.Hidden;
    static SceneFader instance;


    float startFadeTime = -1.0f;


    float displayImageMaxDimension;




    // Use this for initialization
    void Start ()
    {
        instance = this;
        gameObject.SetActive(false);
        gameObject.transform.position = Camera.main.transform.position;
        displayImageMaxDimension = displayImage.rectTransform.sizeDelta.x;
        renderTexture.color = new Color(1, 1, 1, 0.0f);

    }





    static public void SetObject(GameObject obj)
    {
        GameObject.Instantiate(obj, instance.content.transform);
        instance.renderTexture.gameObject.SetActive(true);
    }




    static public void SetImage(Sprite image)
    {
        float imageSizeX = image.bounds.size.x;
        float imageSizeY = image.bounds.size.y;

        if (imageSizeX > imageSizeY)
        {
            float ratio = instance.displayImageMaxDimension / imageSizeX;
            imageSizeY *= ratio;
            imageSizeX = instance.displayImageMaxDimension;
        }
        else
        {
            float ratio = instance.displayImageMaxDimension / imageSizeY;
            imageSizeX *= ratio;
            imageSizeY = instance.displayImageMaxDimension;
        }
        instance.displayImage.rectTransform.sizeDelta = new Vector2(imageSizeX, imageSizeY);
        instance.displayImage.sprite = image;
        instance.displayImage.color = Color.white;
    }







    static public bool IsFading()
    {
        if (!instance)
        {
            return false;
        }

        if ((instance.currentState == State.FadingIn) || (instance.currentState == State.FadingOut))
        {
            return true;
        }

        return false;
    }







    // Update is called once per frame
    void Update ()
    {
        Camera cam = Camera.main;
        if (null != cam)
        {
            transform.rotation = cam.transform.rotation;
        }




        if (startFadeTime != -1)
        {
            float dif = Time.time - startFadeTime;
            float ratio = dif / fadeDurationSec;
            if (ratio > 1.0f)
            {
                startFadeTime = -1;
                if (currentState == State.FadingIn)
                {
                    currentState = State.Visible;
                    instance.renderTexture.color = new Color(1, 1, 1, 1.0f);
                    instance.blackout.color = new Color(0, 0, 0, 1.0f);
                }
                else
                {
                    currentState = State.Hidden;
                    instance.gameObject.SetActive(false);
                }
            }
            else
            {
                float alpha;
                if (currentState == State.FadingIn)
                {
                    alpha = ratio;
                }
                else
                {
                    alpha = 1.0f - ratio;
                }
                instance.renderTexture.color = new Color(1, 1, 1, alpha);
                instance.blackout.color = new Color(0, 0, 0, alpha);
            }
        }

    }






    static public void FadeOut()
    {
        if ((!instance) || (instance.currentState == State.Hidden) || (instance.currentState == State.FadingOut))
        {
            return;
        }

        instance.gameObject.SetActive(true);
        instance.startFadeTime = Time.time;
        instance.currentState = State.FadingOut;
    }







    static public void FadeIn()
    {
        if ((!instance) || (instance.currentState == State.Visible) || (instance.currentState == State.FadingIn))
        {
            return;
        }

        instance.gameObject.SetActive(true);
        instance.renderTexture.color = new Color(0, 0, 0, 0.0f);
        instance.blackout.color = new Color(0, 0, 0, 0.0f);
        instance.startFadeTime = Time.time;
        instance.currentState = State.FadingIn;
    }


    public enum State
    {
        Visible,
        Hidden,
        FadingOut,
        FadingIn
    }

}
