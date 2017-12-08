using UnityEngine;
using System.Collections;

namespace TBE {

	/// <summary>
	/// Updates the listener's orientation in the engine. Must be placed on the main camera.
	/// </summary>
	public class TBEngineListener : MonoBehaviour 
	{
		TBVector3 ForwardVec;
		TBVector3 UpVec;

#if TBE_USE_UNITY_AUDIO_DEVICE
		float[] AudioBuffer;
#endif

		void Awake()
		{
#if TBE_USE_UNITY_AUDIO_DEVICE
			var Config = AudioSettings.GetConfiguration ();
			var buffer_size = Config.dspBufferSize;
			AudioBuffer = new float[buffer_size * 2];
#endif
		}

		void Update () 
		{	
			ForwardVec.set(transform.forward.x, transform.forward.y, transform.forward.z);
			UpVec.set (transform.up.x, transform.up.y, transform.up.z);
			PInvAudioEngine.setListenerOrientation(ForwardVec, UpVec);
		}

#if TBE_USE_UNITY_AUDIO_DEVICE
		void OnAudioFilterRead(float[] data, int channels) 
		{
			if (PInvAudioEngine.isInitialised ())
			{
				PInvAudioEngine.getAudioMix (AudioBuffer, data.Length);

				for (int i = 0; i < data.Length; ++i)
				{
					data[i] += AudioBuffer[i];
				}
			}
		}
#endif
	}

}


