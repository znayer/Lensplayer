using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

namespace TBE {

	/// <summary>
	/// Interface for the native functions of the audio engine.
	/// TBAudioEngine controls all global initialsation/destruction,
	/// routines and listener parameters.
	/// All methods are static.
	/// The AudioEngine must be initialised first and destroyed last.
	/// </summary>
	public class PInvAudioEngine
	{	

		#if UNITY_ANDROID && !UNITY_EDITOR
		const string DLL_NAME = "TBAudioEngineUnity";
		#elif UNITY_IPHONE && !UNITY_EDITOR
		const string DLL_NAME = "__Internal";
		#else
		const string DLL_NAME = "TBAudioEngine";
		#endif

		[DllImport(DLL_NAME)]
		static extern TBError TBAudioEngine_init(float in_fSampleRate, uint in_uBufferSize, TBEngineFlags in_iFlags);
		
		[DllImport(DLL_NAME)]
		static extern void TBAudioEngine_destroy();
		
		[DllImport(DLL_NAME)]
		static extern void TBAudioEngine_start();
		
		[DllImport(DLL_NAME)]
		static extern void TBAudioEngine_pause();
		
		[DllImport(DLL_NAME)]
		static extern void TBAudioEngine_setListenerOrientationVectors(TBVector3 ForwardVector, TBVector3 UpVector);
		
		[DllImport(DLL_NAME)]
		static extern void TBAudioEngine_setListenerOrientationQuat(TBQuat4 ListenerQuat);
		
		[DllImport(DLL_NAME)]
		static extern float TBAudioEngine_getSampleRate();
		
		[DllImport(DLL_NAME)]
		static extern uint TBAudioEngine_getBufferSize();
		
		[DllImport(DLL_NAME)]
		static extern bool TBAudioEngine_isInitialised();
		
		[DllImport(DLL_NAME)]
		static extern int TBAudioEngine_getVersionMajor();
		
		[DllImport(DLL_NAME)]
		static extern int TBAudioEngine_getVersionMinor();
		
		[DllImport(DLL_NAME)]
		static extern int TBAudioEngine_getVersionPatch();

#if TBE_USE_UNITY_AUDIO_DEVICE
		[DllImport(DLL_NAME)]
		static extern void TBAudioEngine_getAudioMix (float[] in_Buffer, int in_iNumOfSamples);
#endif

		/// <summary>
		/// Init the engine by specifying target sample rate and buffer size.
		/// This method must be called first before any other engine component is initialised.
		/// It is not guaranteed that the target device/OS will accept these settings.
		/// The final values can be queried from getSampleRate and getBufferSize
		/// </summary>
		/// <param name="in_fSampleRate">Target sample rate in Hz</param>
		/// <param name="in_uBufferSize">Target buffer size in samples</param>
		/// <returns>TB_SUCCESS if initialisation is successful, or corresponding error message</returns>
		public static TBError init(float in_fSampleRate, uint in_uBufferSize, TBEngineFlags in_eInitFlags)
		{
			return TBAudioEngine_init(in_fSampleRate, in_uBufferSize, in_eInitFlags);
		}

		/// <summary>
		/// Returns true if the engine is initialised, else false
		/// </summary>
		/// <returns><c>true</c>If the engine is initialised, else false <c>false</c> otherwise.</returns>
		public static bool isInitialised()
		{
			return TBAudioEngine_isInitialised();
		}

		/// <summary>
		/// Destroy all resources. Must be called last, after all 
		/// other engine components are destroyed
		/// </summary>
		public static void destroy()
		{
			TBAudioEngine_destroy();
		}

		/// <summary>
		/// Start audio processing (controls global playback of all objects)
		/// This can be useful for starting and pausing all activity on the audio thread
		/// (such as when the app is in the background)
		/// </summary>
		public static void start()
		{
			TBAudioEngine_start();
		}

		/// <summary>
		/// Pause audio processing (controls global playback of all objects)
		/// This can be useful for starting and pausing all activity on the audio thread
		/// such as when the app is in the background)
		/// </summary>
		public static void pause()
		{
			TBAudioEngine_pause();
		}

		/// <summary>
		/// Set the orientation of the listener through direction vectors
		/// </summary>
		/// <param name="ForwardVector">Forward vector of the listener</param>
		/// <param name="UpVector">Up vector of the listener</param>
		public static void setListenerOrientation(TBVector3 ForwardVector, TBVector3 UpVector)
		{
			TBAudioEngine_setListenerOrientationVectors(ForwardVector, UpVector);
		}

		/// <summary>
		/// Returns the sample rate of the engine in Hz.
		/// </summary>
		/// <returns>The sample rate in Hz.</returns>
		public static float getSampleRate()
		{
			return TBAudioEngine_getSampleRate ();
		}

		/// <summary>
		/// Retuns the buffer size of the engine in samples.
		/// </summary>
		/// <returns>The buffer size in samples.</returns>
		public static uint getBufferSize()
		{
			return TBAudioEngine_getBufferSize ();
		}

		public static string getVersion()
		{
			return TBAudioEngine_getVersionMajor () + "." + TBAudioEngine_getVersionMinor () + "." + TBAudioEngine_getVersionPatch ();
		}

#if TBE_USE_UNITY_AUDIO_DEVICE
		/// <summary>
		/// Returns the audio mix -- CAUTION use this only if TBEngineFlags in the init() method is set to INIT_NO_SINK
		/// </summary>
		/// <param name="in_Buffer">In_ buffer.</param>
		/// <param name="in_iNumOfSamples">In_i number of samples.</param>
		public static void getAudioMix(float[] in_Buffer, int in_iNumOfSamples)
		{
			TBAudioEngine_getAudioMix (in_Buffer, in_iNumOfSamples);
		}
#endif
	}


}
