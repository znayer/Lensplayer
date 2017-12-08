using UnityEngine;
using System.Collections;

namespace TBE {

	/// <summary>
	/// Initialises the audio engine on Awake
	/// </summary>
	public class TBEngineInitialise : MonoBehaviour 
	{
		void Awake () 
		{

			const float sample_rate = 44100;

#if TBE_USE_UNITY_AUDIO_DEVICE
			var Config = AudioSettings.GetConfiguration ();
			uint buffer_size = (uint) Config.dspBufferSize;

			if (sample_rate != AudioSettings.outputSampleRate)
			{
				Debug.LogError ("TBAudioEngine: Unity's sample rate does not match TBAudioEngine. Set the system sample rate in Edit > Project Settings > Audio to 44100");
				return;
			}

			TBError err = PInvAudioEngine.init(sample_rate, buffer_size, TBEngineFlags.INIT_NO_SINK);	
#else
			#if UNITY_IOS && !UNITY_EDITOR
			const uint buffer_size = 1024;
			#else
			const uint buffer_size = 512;
			#endif

			TBError err = PInvAudioEngine.init(sample_rate, buffer_size, TBEngineFlags.INIT_DEFAULT);	
#endif

			if (err < TBError.TB_SUCCESS)
			{
				Debug.LogError("Error initialising TBAudioEngine: " + err);
				return;
			}

			PInvAudioEngine.start();
		}

		void OnApplicationPause(bool pauseStatus) 
		{	
			if (!PInvAudioEngine.isInitialised ()) 
			{
				return;
			}

			if (pauseStatus) 
			{
				PInvAudioEngine.pause();
			} 
			else 
			{
				PInvAudioEngine.start();
			}
		}

	}

}

