using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedUVTexture : MonoBehaviour
{
	private float iX=0;
	private float iY=1;
	public int _uvTieX = 1;
	public int _uvTieY = 1;
	public int _fps = 10;
	private Vector2 _size;
	private Renderer _myRenderer;
	private int _lastIndex = -1;
	private bool hasStarted = false;

	/**
	 * Function called by the SceneControllerLensPlayerWaitingRoom class in order to
	 * initiate the animation sequence
	 *
	 **/
	public void commenceAnimation ()
	{
		StartCoroutine(waitingForOne());
	}

	/**
	 * Waits for a second (for timing reasons) and then initiatives the animated texture code
	 *
	 **/
	IEnumerator waitingForOne()
	{
		yield return new WaitForSeconds (1);
		hasStarted = true;
	}


	/**
	 * Sets up the animated texture to be animated and puts the animation on the first frame
	 * which is just a black screen - prepares for play, to be initiated by commenceAnimation()
	 *
	 **/
	void Start ()
	{
		_size = new Vector2 (1.0f / _uvTieX ,
			1.0f / _uvTieY);

		_myRenderer = GetComponent<Renderer>();

		if(_myRenderer == null) enabled = false;

		_myRenderer.material.SetTextureScale ("_MainTex", _size);

		int index = (int)(Time.timeSinceLevelLoad * _fps) % (_uvTieX * _uvTieY);
		if (index != _lastIndex) {
			Vector2 offset = new Vector2 (iX * _size.x,
				1 - (_size.y * iY));
			iX++;
			if (iX / _uvTieX == 1) {
				if (_uvTieY != 1)
					iY++;
				iX = 0;
				if (iY / _uvTieY == 1) {
					iY = 1;
				}
			}
			_myRenderer.material.SetTextureOffset ("_MainTex", offset);
			_lastIndex = index;
		}
	}


	/**
	 * If the animation has been started (as indicated by the hasStarted bool), updates the
	 * next frame of the animation.
	 *
	 **/
	void Update()
	{
		if (hasStarted) {
			int index = (int)(Time.timeSinceLevelLoad * _fps) % (_uvTieX * _uvTieY);

			if (index != _lastIndex) {
				Vector2 offset = new Vector2 (iX * _size.x,
					1 - (_size.y * iY));
				iX++;
				if (iX / _uvTieX == 1) {
					if (_uvTieY != 1)
						iY++;
					iX = 0;
					if (iY / _uvTieY == 1) {
						iY = 1;
					}
				}

				_myRenderer.material.SetTextureOffset ("_MainTex", offset);
				_lastIndex = index;
			}
		}
	}
}
