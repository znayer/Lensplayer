using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

namespace TBE {

	/// <summary>
	/// Interface for native functions of TBSpatDecoder
	/// </summary>
	public class PInvSpatDecoder
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
		const string DLL_NAME = "TBAudioEngineUnity";
		#elif UNITY_IPHONE && !UNITY_EDITOR
		const string DLL_NAME = "__Internal";
		#else
		const string DLL_NAME = "TBAudioEngine";
		#endif
		
		[DllImport(DLL_NAME)]
		static extern IntPtr TBSpatDecoder_new();
		
		[DllImport(DLL_NAME)]
		static extern TBError TBSpatDecoder_init(IntPtr pSpatDecoder);
		
		[DllImport(DLL_NAME)]
		static extern void TBSpatDecoder_destroy(IntPtr pSpatDecoder);
		
		[DllImport(DLL_NAME)]
		static extern TBError TBSpatDecoder_play(IntPtr pSpatDecoder);
		
		[DllImport(DLL_NAME)]
		static extern TBError TBSpatDecoder_pause(IntPtr pSpatDecoder);
		
		[DllImport(DLL_NAME)]
		static extern TBError TBSpatDecoder_stop(IntPtr pSpatDecoder);
		
		[DllImport(DLL_NAME)]
		static extern TBError TBSpatDecoder_loadAsset(IntPtr pSpatDecoder, string in_cAssetName, TBAssetLoadType in_LoadType, TBAssetLocation in_eLocation);

		[DllImport(DLL_NAME)]
		static extern TBError TBSpatDecoder_loadChunkAsAsset(IntPtr pSpatDecoder, string in_cAssetName, TBAssetLoadType in_eLoadType, TBAssetLocation in_eLocation, long in_lOffsetInBytes, long in_lLengthInBytes);

		[DllImport(DLL_NAME)]
		static extern void TBSpatDecoder_setVolumeInDecibels(IntPtr pSpatDecoder, float in_fVolumeInDecibels);

		[DllImport(DLL_NAME)]
		static extern float TBSpatDecoder_getVolumeInDecibels(IntPtr pSpatDecoder);

		[DllImport(DLL_NAME)]
		static extern void TBSpatDecoder_setVolume(IntPtr pSpatDecoder, float in_fLinearAmplitude);
		
		[DllImport(DLL_NAME)]
		static extern float TBSpatDecoder_getVolume(IntPtr pSpatDecoder);

		[DllImport(DLL_NAME)]
		static extern long TBSpatDecoder_getAssetDurationInSamples(IntPtr pSpatDecoder);

		[DllImport(DLL_NAME)]
		static extern float TBSpatDecoder_getAssetDurationInMs(IntPtr pSpatDecoder);
		
		[DllImport(DLL_NAME)]
		static extern long TBSpatDecoder_getTimeInSamples(IntPtr pSpatDecoder);
		
		[DllImport(DLL_NAME)]
		static extern float TBSpatDecoder_getTimeInMs(IntPtr pSpatDecoder);

		[DllImport(DLL_NAME)]
		static extern TBError TBSpatDecoder_setTimeInSamples(IntPtr pSpatDecoder, int in_iTimeInSamples);
		
		[DllImport(DLL_NAME)]
		static extern TBError TBSpatDecoder_setTimeInMs(IntPtr pSpatDecoder, float in_fTimeInMs);

		[DllImport(DLL_NAME)]
		static extern void TBSpatDecoder_setSyncMode(IntPtr pSpatDecoder, TBSyncMode in_eSyncMode);

		[DllImport(DLL_NAME)]
		static extern TBSyncMode TBSpatDecoder_getSyncMode(IntPtr pSpatDecoder);

		[DllImport(DLL_NAME)]
		static extern void TBSpatDecoder_setExternalClockInMs(IntPtr pSpatDecoder, float in_fExternalClockInMs);

		[DllImport(DLL_NAME)]
		static extern void TBSpatDecoder_setFreewheelTimeInMs(IntPtr pSpatDecoder, float in_fFreewheelInMs);

		[DllImport(DLL_NAME)]
		static extern float TBSpatDecoder_getFreewheelTimeInMs(IntPtr pSpatDecoder);

		[DllImport(DLL_NAME)]
		static extern TBPlayState TBSpatDecoder_getPlayState(IntPtr pSpatDecoder);

		[DllImport(DLL_NAME)]
		static extern void TBSpatDecoder_enableFocus(IntPtr pSpatDecoder, bool in_bEnableFocus, bool in_bFollowListener);

		[DllImport(DLL_NAME)]
		static extern void TBSpatDecoder_setFocusProperties(IntPtr pSpatDecoder, float in_fOffFocusLevel, float in_fFocusWidth);

		[DllImport(DLL_NAME)]
		static extern void TBSpatDecoder_setFocusOrientationQuat(IntPtr pSpatDecoder, TBQuat4 in_FocusQuat);

		private IntPtr rawPtr;
		
		public PInvSpatDecoder()
		{
			rawPtr = TBSpatDecoder_new();
		}

		public TBError init()
		{
			return TBSpatDecoder_init(rawPtr);
		}
		
		public void destroy()
		{
			TBSpatDecoder_destroy(rawPtr);
		}
		
		public TBError play()
		{
			return TBSpatDecoder_play(rawPtr);
		}
		
		public TBError stop()
		{
			return TBSpatDecoder_stop(rawPtr);
		}

		public TBError pause()
		{
			return TBSpatDecoder_pause(rawPtr);
		}
		
		public TBError loadAsset(string in_cAssetName, TBAssetLoadType in_LoadType, TBAssetLocation in_eLocation)
		{
			return TBSpatDecoder_loadAsset(rawPtr, in_cAssetName, in_LoadType, in_eLocation);
		}

		public TBError loadChunkAsAsset(string in_cAssetName, TBAssetLoadType in_eLoadType, TBAssetLocation in_eLocation, long in_lOffsetInBytes, long in_lLengthInBytes)
		{
			return TBSpatDecoder_loadChunkAsAsset (rawPtr, in_cAssetName, in_eLoadType, in_eLocation, in_lOffsetInBytes, in_lLengthInBytes);
		}

		public void setVolumeInDecibels(float in_fVolumeInDecibels)
		{
			TBSpatDecoder_setVolumeInDecibels (rawPtr, in_fVolumeInDecibels);
		}

		public float getVolumeInDecibels()
		{
			return TBSpatDecoder_getVolumeInDecibels (rawPtr);
		}

		public void setVolume(float in_fLinearGain)
		{
			TBSpatDecoder_setVolume (rawPtr, in_fLinearGain);
		}
		
		public float getVolume()
		{
			return TBSpatDecoder_getVolume (rawPtr);
		}

		public void enableFocus(bool in_bEnableFocus, bool in_bFollowListener)
		{
			TBSpatDecoder_enableFocus(rawPtr, in_bEnableFocus, in_bFollowListener);
		}

		public void setFocusProperties(float in_fOffFocusLevel, float in_fFocusWidth)
		{
			TBSpatDecoder_setFocusProperties(rawPtr, in_fOffFocusLevel, in_fFocusWidth);
		}

		public void setFocusOrientationQuat(TBQuat4 in_FocusQuat)
		{
			TBSpatDecoder_setFocusOrientationQuat (rawPtr, in_FocusQuat);
		}

		public long getAssetDurationInSamples()
		{
			return TBSpatDecoder_getAssetDurationInSamples (rawPtr);
		}

		public float getAssetDurationInMs()
		{
			return TBSpatDecoder_getAssetDurationInMs (rawPtr);
		}

		public long getTimeInSamples()
		{
			return TBSpatDecoder_getTimeInSamples(rawPtr);
		}

		public float getTimeInMs()
		{
			return TBSpatDecoder_getTimeInMs(rawPtr);
		}

		public TBError setTimeInSamples(int in_fTimeInSamples)
		{
			return TBSpatDecoder_setTimeInSamples(rawPtr, in_fTimeInSamples);
		}
		
		public TBError setTimeInMs(float in_fTimeInMs)
		{
			return TBSpatDecoder_setTimeInMs (rawPtr, in_fTimeInMs);
		}

		public void setSyncMode(TBSyncMode in_eSyncMode)
		{
			TBSpatDecoder_setSyncMode(rawPtr, in_eSyncMode);
		}

		public TBSyncMode getSyncMode()
		{
			return TBSpatDecoder_getSyncMode(rawPtr);
		}

		public void setExternalClockInMs(float in_fExternalClockInMs)
		{
			TBSpatDecoder_setExternalClockInMs(rawPtr, in_fExternalClockInMs);
		}

		public void setFreewheelTimeInMs(float in_fFreewheelInMs)
		{
			TBSpatDecoder_setFreewheelTimeInMs(rawPtr, in_fFreewheelInMs);
		}

		public float getFreewheelTimeInMs()
		{
			return TBSpatDecoder_getFreewheelTimeInMs(rawPtr);
		}

		public TBPlayState getPlayState()
		{
			return TBSpatDecoder_getPlayState (rawPtr);
		}
	}
	
}
