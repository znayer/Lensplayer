using UnityEngine;
using System.Collections;
using System.IO;

namespace TBE {

	/// <summary>
	/// Decodes and plays back assets created
	/// with the desktop authoring tool. The mix
	/// is recreated in full 3D
	/// </summary>
	public class TBSpatDecoder : MonoBehaviour 
	{	
		PInvSpatDecoder Decoder_ = null;
		bool bIsInitialised_ = false;

		[SerializeField] TriggerType ePlayTrigger_ = TriggerType.Start;
		[SerializeField] TriggerType eLoadTrigger_ = TriggerType.Awake;
		[SerializeField] string StreamingAsset_ = string.Empty;
		[SerializeField] TBSyncMode eSyncMode_ = TBE.TBSyncMode.TB_SYNC_INTERNAL;
		[SerializeField] float fFreewheelTimeMs_ = 200.0f;
		[SerializeField] float fVolume_ = 1.0f;
		[SerializeField] bool bEnableFocus_ = false;
		[SerializeField] bool bFollowListener_ = true;
		[SerializeField] float fOffFocusLevel_ = 1.0f;
		[SerializeField] float fFocusWidth_ = 90.0f;

		void Awake () 
		{	
			if (!PInvAudioEngine.isInitialised ()) 
			{	
				Debug.LogError("TBAudioEngine isn't initialised. Have you setup the scene and script execution order?");
				return;
			}

			Decoder_ = new PInvSpatDecoder();

			if (Decoder_ != null)
			{
				TBError err = Decoder_.init();
				if(err != TBError.TB_SUCCESS)
				{
					Debug.LogError("Error initialising Spat Decoder on " + gameObject.name + ": " + err);
					bIsInitialised_ = false;
					return;
				}

				bIsInitialised_ = true;

				freewheelTimeMs = fFreewheelTimeMs_;
				syncMode = eSyncMode_;
				volume = fVolume_;
				focus = bEnableFocus_;
				followListener = bFollowListener_;
				offFocusLevel = fOffFocusLevel_;
				focusWidth = fFocusWidth_;

				if (eLoadTrigger_ == TriggerType.Awake)
				{
					loadAssetFromApp(StreamingAsset_);
				}

				if (ePlayTrigger_ == TriggerType.Awake)
				{
					play();
				}
			}

		}

		void Start ()
		{
			if (eLoadTrigger_ == TriggerType.Start)
			{
				loadAssetFromApp(StreamingAsset_);
			}
			
			if (ePlayTrigger_ == TriggerType.Start)
			{
				play();
			}
		}

		void OnDestroy()
		{	
			if (Decoder_ != null && bIsInitialised_)
			{
				bIsInitialised_ = false;
				Decoder_.destroy();
				Decoder_ = null;
			}
		}

		/// <summary>
		/// Load an asset of type .tba, created by desktop authoring tools
		/// The file must be placed in Assets/StreamingAssets and will automatically
		/// be picked up the engine.
		/// </summary>
		/// <returns>TB_SUCCESS if successful or corresponding error message.</returns>
		/// <param name="AssetName">Name of the asset, including extension, placed in the 'StreamingAssets' folder.</param>
		public TBError loadAssetFromApp(string AssetName)
		{		
			TBError err = TBError.TB_FAIL;

			if (Decoder_ != null && bIsInitialised_ && !string.IsNullOrEmpty(AssetName))
			{	

				if (Application.platform == RuntimePlatform.Android)
				{
					err = Decoder_.loadAsset(AssetName, TBAssetLoadType.TB_TYPE_STREAM, TBAssetLocation.ASSET_APP_BUNDLE);
				}
				else
				{
					string assetNameWithPath = Path.Combine(Application.streamingAssetsPath, AssetName);
					err = Decoder_.loadAsset(assetNameWithPath, TBAssetLoadType.TB_TYPE_STREAM, TBAssetLocation.ASSET_ABSOLUTE_PATH);
				}

				if (err != TBError.TB_SUCCESS)
				{
					Debug.LogError("Error loading " + AssetName + " on " + gameObject.name + ": " + err);
				}
			}

			return err;
		}

		/// <summary>
		/// Load an asset with absolute path, created by desktop authoring tools
		/// The file can be placed anywhere on the system.
		/// </summary>
		/// <returns>TB_SUCCESS if successful or corresponding error message.</returns>
		/// <param name="AssetName">Name of the asset with absolute ath, including extension.</param>
		public TBError loadAssetFromPath(string AssetNameWithPath)
		{		
			TBError err = TBError.TB_FAIL;
			
			if (Decoder_ != null && bIsInitialised_ && !string.IsNullOrEmpty(AssetNameWithPath))
			{	
				err = Decoder_.loadAsset(AssetNameWithPath, TBAssetLoadType.TB_TYPE_STREAM, TBAssetLocation.ASSET_ABSOLUTE_PATH);
				
				if (err != TBError.TB_SUCCESS)
				{
					Debug.LogError("Error loading " + AssetNameWithPath + " on " + gameObject.name + ": " + err);
				}
			}
			
			return err;
		}

		/// <summary>
		/// Load a chunk of data as an asset. This is useful in cases where multiple assets might be combined in a "store" zip file or a binary blob.
		/// </summary>
		/// <returns>TB_SUCCESS if successful or corresponding error message.</returns>
		/// <param name="AbsoluteFileNameAndPath">Absolute file name and path.</param>
		/// <param name="offsetInBytes">Offset in bytes, from start of file.</param>
		/// <param name="lengthInBytes">Length of asset in bytes.</param>
		public TBError loadChunkAsAsset(string AbsoluteFileNameAndPath, long offsetInBytes, long lengthInBytes)
		{
			TBError err = TBError.TB_FAIL;

			if (Decoder_ != null && bIsInitialised_ && !string.IsNullOrEmpty(AbsoluteFileNameAndPath))
			{	
				err = Decoder_.loadChunkAsAsset(AbsoluteFileNameAndPath, TBAssetLoadType.TB_TYPE_STREAM, TBAssetLocation.ASSET_ABSOLUTE_PATH, offsetInBytes, lengthInBytes);
				
				if (err != TBError.TB_SUCCESS)
				{
					Debug.LogError("Error loading " + AbsoluteFileNameAndPath + " on " + gameObject.name + ": " + err);
				}
			}
			
			return err;
		}

		/// <summary>
		/// Playback the asset
		/// Resumes playback if paused. Starts playback
		/// from the beginning if stopped.
		/// </summary>
		/// <returns>TB_SUCCESS if successful or corresponding error message.</returns>
		public TBError play()
		{		
			TBError err = TBError.TB_FAIL;

			if (Decoder_ != null && bIsInitialised_)
			{	
				err = Decoder_.play();
				if (err != TBError.TB_SUCCESS && err != TBError.TB_NO_ASSET)
				{
					Debug.LogError("Error playing Spat Decoder on " + gameObject.name + ": " + err);
				}
			}

			return err;
		}

		/// <summary>
		/// Stop playback
		/// </summary>
		/// <returns>TB_SUCCESS if successful or corresponding error message.</returns>
		public TBError stop()
		{		
			TBError err = TBError.TB_FAIL;

			if (Decoder_ != null && bIsInitialised_)
			{
				err = Decoder_.stop();
				if (err != TBError.TB_SUCCESS && err != TBError.TB_NO_ASSET)
				{
					Debug.LogError("Error stopping Spat Decoder on " + gameObject.name + ": " + err);
				}
			}

			return err;
		}

		/// <summary>
		/// Pause playback
		/// </summary>
		/// <returns>TB_SUCCESS if successful or corresponding error message.</returns>
		public TBError pause()
		{	
			TBError err = TBError.TB_FAIL;

			if (Decoder_ != null && bIsInitialised_)
			{
				err = Decoder_.pause();
				if (err != TBError.TB_SUCCESS)
				{
					Debug.LogError("Error pausing Spat Decoder on " + gameObject.name + ": " + err);
				}
			}

			return err;
		}

		/// <summary>
		/// Enable mix focus. This gets a specfied area of the mix to be more audible than surrounding areas, by reducing the
		/// amplitude of the area that isn't in focus.
		/// The focus area is shaped as a cosine bump.
		/// </summary>
		/// <param name="enableFocus">If set to <c>true</c> enables focus.</param>
		/// <param name="followListener">If set to <c>true</c>, the focus area follows the listener's gaze.</param>
		public void enableFocus(bool enableFocus, bool followListener)
		{
			bEnableFocus_ = enableFocus;
			bFollowListener_ = followListener;

			if (Decoder_ != null && bIsInitialised_) 
			{
				Decoder_.enableFocus(bEnableFocus_, bFollowListener_);
			}
		}

		/// <summary>
		/// Set the properties of the focus effect. This will be audible only if enableFocus in
		/// enableFocus(..) is set to true.
		/// </summary>
		/// <param name="offFocusLevel">The level of the area that isn't in focus. A clamped ranged between 0 and 1. 1 is no focus. 0 is maximum focus (the
		///                         off focus area is reduced by 14dB). Default = 1. </param>
		/// <param name="focusWidth">The focus area specified in degrees. Clamped to a range of 40 to 120 degrees. Default = 90 degrees.</param>
		public void setFocusProperties(float offFocusLevel, float focusWidth)
		{
			fOffFocusLevel_ = Mathf.Clamp01(offFocusLevel);
			fFocusWidth_ = Mathf.Clamp(focusWidth, 40.0f, 120.0f);

			if (Decoder_ != null && bIsInitialised_) 
			{
				Decoder_.setFocusProperties(fOffFocusLevel_, fFocusWidth_);
			}
		}

		/// <summary>
		/// Set the orientation of the focus area as a quaternion. This orientation is from the perspective
		/// of the listener and comes into effect only if followListener in enableFocus(..) is set to false.
		/// </summary>
		/// <param name="focusQuat">Orientation of the focus area as a quaternion.</param>
		public void setFocusOrientationQuat(Quaternion focusQuat)
		{
			if (Decoder_ != null && bIsInitialised_) 
			{	
				TBQuat4 TBQuat = new TBQuat4(focusQuat.x, focusQuat.y, focusQuat.z, focusQuat.w);
				Decoder_.setFocusOrientationQuat(TBQuat);
			}
		}

		/// <summary>
		/// Returns the elapsed time, in samples, of the currently loaded asset
		/// </summary>
		/// <returns>Elapsed time in samples.</returns>
		public long getTimeInSamples()
		{
			if (Decoder_ != null && bIsInitialised_)
			{
				return Decoder_.getTimeInSamples();
			}
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// Returns the duration of the asset (if loaded) in samples.
		/// </summary>
		/// <returns>The duration of the asset in samples.</returns>
		public long getAssetDurationInSamples()
		{
			if (Decoder_ != null && bIsInitialised_)
			{
				return Decoder_.getAssetDurationInSamples();
			}
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// Returns the duration of the asset (if loaded) in milliseconds
		/// </summary>
		/// <returns>The duration of the asset in milliseconds.</returns>
		public float getAssetDurationInMs()
		{
			if (Decoder_ != null && bIsInitialised_)
			{
				return Decoder_.getAssetDurationInMs();
			}
			else
			{
				return 0.0f;
			}
		}

		/// <summary>
		/// Returns the elapsed time, in milliseconds, of the currently loaded asset
		/// </summary>
		/// <returns>Elapsed time in ms.</returns>
		public float getTimeInMs()
		{
			if (Decoder_ != null && bIsInitialised_)
			{
				return Decoder_.getTimeInMs();
			}
			else
			{
				return 0.0f;
			}
		}

		/// <summary>
		/// Set the playback position, in samples, of the currently loaded asset
		/// </summary>
		/// <returns>TB_SUCCESS if successful or corresponding error</returns>
		public TBError setTimeInSamples(int timeInSamples)
		{
			if (Decoder_ != null && bIsInitialised_)
			{
				return Decoder_.setTimeInSamples(timeInSamples);
			}
			else
			{
				return TBError.TB_FAIL;
			}
		}
		
		/// <summary>
		/// Set the playback position, in milliseconds, of the currently loaded asset
		/// </summary>
		/// <returns>TB_SUCCESS if successful or corresponding error</returns>
		public TBError setTimeInMs(float timeInMs)
		{
			if (Decoder_ != null && bIsInitialised_)
			{
				return Decoder_.setTimeInMs(timeInMs);
			}
			else
			{
				return TBError.TB_FAIL;
			}
		}

		/// <summary>
		/// If syncMode is set to TB_SYNC_EXTERNAL, playback can be
		/// synchronised to an external time reference in milliseconds
		/// </summary>
		/// <param name="externalClockInMs">External time reference in milliseconds</param>
		public void setExternalClockInMs(float externalClockInMs)
		{
			if (Decoder_ != null && bIsInitialised_) 
			{
				Decoder_.setExternalClockInMs(externalClockInMs);
			}
		}

		/// <summary>
		/// Gets or sets the streaming asset name with extension, placed in the 'StreamingAssets' folder.
		/// </summary>
		/// <value>The streaming asset name with extension</value>
		public string streamingAsset
		{
			get
			{
				return StreamingAsset_;
			}

			set
			{
				StreamingAsset_ = value;
			}
		}

		/// <summary>
		/// Gets or sets the playback trigger
		/// </summary>
		/// <value>The type of trigger.</value>
		public TriggerType playTriggerType
		{
			get
			{
				return ePlayTrigger_;
			}
			set
			{
				ePlayTrigger_ = value;
			}
		}

		/// <summary>
		/// Gets or sets the load trigger
		/// </summary>
		/// <value>The type of the load trigger.</value>
		public TriggerType loadTriggerType
		{
			get
			{
				return eLoadTrigger_;
			}
			set
			{
				eLoadTrigger_ = value;
			}
		}

		/// <summary>
		/// Gets the playback state.
		/// </summary>
		/// <value>TB_STATE_PLAYING, TB_STATE_PAUSED, TB_STATE_STOPPED or TB_STATE_INVALID</value>
		public TBPlayState playState
		{
			get
			{
				if (Decoder_ != null)
				{
					return Decoder_.getPlayState();
				}
				else
				{
					return TBPlayState.TB_STATE_INVALID;
				}
			}
		}

		/// <summary>
		/// Gets or sets the sync mode.
		/// If set to external (TB_SYNC_EXTERNAL), playback is controlled by an external clock
		/// using the setExternalClockInMs method. If set to internal (default), playback is synchronised
		/// to the audio hardware
		/// </summary>
		/// <value>The sync mode.</value>
		public TBSyncMode syncMode
		{
			get
			{
				return eSyncMode_;
			}
			set
			{	
				eSyncMode_ = value;
				if (Decoder_ != null)
				{
					Decoder_.setSyncMode(eSyncMode_);
				}
			}
		}

		/// <summary>
		/// Set/get how often the synchroniser syncs with the external clock in milliseconds.
		/// Setting this to a larger value can help overcome drop outs or slow update times from the clock source
		/// Default recommended value: 200ms.
		/// This only has an effect if syncMode is set to TB_SYNC_EXTERNAL
		/// </summary>
		/// <value>How often (in milliseconds) the synchroniser syncs with the external clock. Default: 200ms</value>
		public float freewheelTimeMs
		{
			get
			{
				return fFreewheelTimeMs_;
			}
			set
			{
				fFreewheelTimeMs_ = value;
				if (Decoder_ != null)
				{
					Decoder_.setFreewheelTimeInMs(fFreewheelTimeMs_);
				}
			}
		}

		/// <summary>
		/// Gets or sets the volume in decibels.
		/// </summary>
		/// <value>The volume in decibels.</value>
		public float volumeInDecibels
		{
			get
			{
				if (Decoder_ != null)
				{
					return Decoder_.getVolumeInDecibels();
				}
				else
				{
					return 0.0f;
				}
			}
			set
			{
				if (Decoder_ != null)
				{
					Decoder_.setVolumeInDecibels(value);
				}
			}
		}

		/// <summary>
		/// Gets or set the linear amplitude.
		/// </summary>
		/// <value>The linear amplitude.</value>
		public float volume
		{
			get
			{
				return fVolume_;
			}
			set
			{
				fVolume_ = value;
				if (Decoder_ != null)
				{
					Decoder_.setVolume(fVolume_);
				}
			}
		}

		/// <summary>
		/// Enable mix focus. This gets a specfied area of the mix to be more audible than surrounding areas, by reducing the
		/// amplitude of the area that isn't in focus.
		/// The focus area is shaped as a cosine bump.
		/// </summary>
		/// <value="enableFocus">If set to <c>true</c> enables focus.</value>
		public bool focus
		{
			get
			{
				return bEnableFocus_;
			}
			set
			{
				bEnableFocus_ = value;
				enableFocus(bEnableFocus_, bFollowListener_);
			}
		}

		/// <summary>
		/// The focus area follows the listener's gaze.
		/// </summary>
		/// <value name="followListener">If set to <c>true</c>, the focus area follows the listener's gaze.</value>
		public bool followListener
		{
			get
			{
				return bFollowListener_;
			}
			set
			{
				bFollowListener_ = value;
				enableFocus(bEnableFocus_, bFollowListener_);
			}
		}

		/// <summary>
		/// The level of the area that isn't in focus. A clamped ranged between 0 and 1. 1 is no focus. 0 is maximum focus (the
		///                         off focus area is reduced by 14dB). Default = 1. 
		/// </summary>
		/// <value>The off focus level.</value>
		public float offFocusLevel
		{
			get
			{
				return fOffFocusLevel_;
			}
			set
			{
				fOffFocusLevel_ = value;
				setFocusProperties(fOffFocusLevel_, fFocusWidth_);
			}
		}

		/// <summary>
		/// The focus area specified in degrees. Clamped to a range of 40 to 120 degrees. Default = 90 degrees.
		/// </summary>
		/// <value>The width of the focus area.</value>
		public float focusWidth
		{
			get
			{
				return fFocusWidth_;
			}
			set
			{
				fFocusWidth_ = value;
				setFocusProperties(fOffFocusLevel_, fFocusWidth_);
			}
		}
	}
}

