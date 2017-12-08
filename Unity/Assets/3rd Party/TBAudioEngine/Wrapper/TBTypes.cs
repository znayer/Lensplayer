using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace TBE
{
	public enum TBEngineFlags
	{
		INIT_DEFAULT = 0,
		INIT_NO_SINK = (1 << 0)
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct TBVector3 
	{
		public float x;
		public float y;
		public float z;

		public TBVector3 (float xValue, float yValue, float zValue) 
		{	
			x = xValue;
			y = yValue;
			z = zValue;
		}

		public void set(float xValue, float yValue, float zValue) 
		{	
			x = xValue;
			y = yValue;
			z = zValue;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct TBQuat4 
	{
		public float x;
		public float y;
		public float z;
		public float w;

		public TBQuat4(float xValue, float yValue, float zValue, float wValue)
		{
			x = xValue;
			y = yValue;
			z = zValue;
			w = wValue;
		}
	}

	public enum TriggerType
	{
		None,
		Awake,
		Start
	}

	/// For specifying if the asset must be found from the app bundle
	/// or if the asset name includes the absolute path
	public enum TBAssetLocation
	{
		ASSET_APP_BUNDLE,   /// The asset must be found from the app bundle (within the app resources on iOS, 'assets' folder on Android)
		ASSET_ABSOLUTE_PATH /// The asset name includes the absolute path
	};

	public enum TBAssetLoadType
	{
		TB_TYPE_MEMORY = 0,
		TB_TYPE_STREAM = 1,
	};
	
	public enum TBError
	{	
		TB_CURL_FAIL = -19,
		TB_AUDIO_DEVICE_FAIL = -18,
		TB_NO_SINK = -17,
		TB_INVALID_FILE_CHANNELS = -16,
		TB_CANNOT_INIT_DECODER = -15,
		TB_NOT_INITIALISED = -14,
		TB_INVALID_FILE_SIZE = -13,
		TB_ERROR_OPENING_FILE = -12,
		TB_INVALID_FILE_NAME = -11,
		TB_NO_ASSET = -10,
		TB_ENGINE_NOT_INIT = -9,
		TB_3DCEPTION_SOURCE_INIT_FAIL = -8,
		TB_CANNOT_ALLOCATE_MEMORY = -7,
		TB_CANNOT_CREATE_SINK = -6,
		TB_3DCEPTION_INIT_ERROR = -5,
		TB_INVALID_BUFFER_SIZE = -4,
		TB_INVALID_SAMPLE_RATE = -3,
		TB_CANNOT_FIND_FILE = -2,
		TB_FAIL = -1,
		TB_SUCCESS = 0,
		TB_ALREADY_INITIALISED = 1
	};
	
	public enum TBPlayState
	{
		TB_STATE_PLAYING,
		TB_STATE_PAUSED,
		TB_STATE_STOPPED,
		TB_STATE_INVALID
	};

	public enum TBSyncMode
	{
		TB_SYNC_INTERNAL,
		TB_SYNC_EXTERNAL
	};

}
