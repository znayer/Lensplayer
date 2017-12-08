using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoRenderer : MonoBehaviour
{
    public GameObject mono;
    public GameObject stereoVertical;
    public GameObject stereoHorizontal;
    public GameObject stereoHorizontal180;
    public GameObject stereoVertical180;
	public GameObject widescreen;

    public Material black;

    static VideoRenderer instance;

    public enum RendererType
    {
        mono,
        stereoHorizontal,
        stereoVerical,
        stereoHorizontal180,
        stereoVertical180,
		widescreen,
        none
    };


	// Use this for initialization
	void Start ()
    {
        instance = this;	
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}


    static public void SetStereoVerticalMaterial(Material mat)
    {
        for (int i=0; i<instance.stereoVertical.transform.childCount; ++i)
        {
            if (null == mat)
            {
                instance.stereoVertical.transform.GetChild(i).GetComponent<Renderer>().material = instance.black;
            }
            else
            {
                instance.stereoVertical.transform.GetChild(i).GetComponent<Renderer>().material = mat;
            }
        }
    }


    static public void SetStereoHorizontalMaterial(Material mat)
    {
        for (int i = 0; i < instance.stereoVertical.transform.childCount; ++i)
        {
            if (null == mat)
            {
                instance.stereoHorizontal.transform.GetChild(i).GetComponent<Renderer>().material = instance.black;
            }
            else
            {
                instance.stereoHorizontal.transform.GetChild(i).GetComponent<Renderer>().material = mat;
            }
        }
    }




    static public void SetMonoMaterial(Material mat)
    {
        if (null==mat)
        {
            instance.mono.GetComponent<Renderer>().material = instance.black;
        }
        else
        {
            instance.mono.GetComponent<Renderer>().material = mat;
        }
    }

	static public void SetWideScreenMaterial(Material mat)
	{
		if (null == mat)
		{
			instance.widescreen.GetComponent<Renderer>().material = instance.black;
		}
		else
		{
			instance.widescreen.GetComponent<Renderer>().material = mat;
		}
	}

/*
    static public void SetMonoTexture(Texture tex)
    {
        instance.mono.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
    }
*/


    static public void SetVideoRendererActive(RendererType type)
    {
        switch (type)
        {
            case RendererType.none:
                instance.mono.SetActive(false);
                instance.stereoHorizontal.SetActive(false);
                instance.stereoVertical.SetActive(false);
                instance.stereoHorizontal180.SetActive(false);
                instance.stereoVertical180.SetActive(false);
				instance.widescreen.SetActive(false);
				break;
            case RendererType.mono:
                instance.mono.SetActive(true);
                instance.stereoHorizontal.SetActive(false);
                instance.stereoVertical.SetActive(false);
                instance.stereoHorizontal180.SetActive(false);
                instance.stereoVertical180.SetActive(false);
				instance.widescreen.SetActive(false);
				break;
            case RendererType.stereoHorizontal:
                instance.mono.SetActive(false);
                instance.stereoHorizontal.SetActive(true);
                instance.stereoVertical.SetActive(false);
                instance.stereoHorizontal180.SetActive(false);
                instance.stereoVertical180.SetActive(false);
				instance.widescreen.SetActive(false);
				break;
            case RendererType.stereoVerical:
                instance.mono.SetActive(false);
                instance.stereoHorizontal.SetActive(false);
                instance.stereoVertical.SetActive(true);
                instance.stereoHorizontal180.SetActive(false);
                instance.stereoVertical180.SetActive(false);
				instance.widescreen.SetActive(false);
				break;
            case RendererType.stereoHorizontal180:
                instance.mono.SetActive(false);
                instance.stereoHorizontal.SetActive(false);
                instance.stereoVertical.SetActive(false);
                instance.stereoHorizontal180.SetActive(true);
                instance.stereoVertical180.SetActive(false);
				instance.widescreen.SetActive(false);
				break;
            case RendererType.stereoVertical180:
                instance.mono.SetActive(false);
                instance.stereoHorizontal.SetActive(false);
                instance.stereoVertical.SetActive(false);
                instance.stereoHorizontal180.SetActive(false);
                instance.stereoVertical180.SetActive(true);
				instance.widescreen.SetActive(false);
				break;
			case RendererType.widescreen:
				instance.mono.SetActive(false);
				instance.stereoHorizontal.SetActive(false);
				instance.stereoVertical.SetActive(false);
				instance.stereoHorizontal180.SetActive(false);
				instance.stereoVertical180.SetActive(false);
				instance.widescreen.SetActive(true);
				break;
		}
    }
}
