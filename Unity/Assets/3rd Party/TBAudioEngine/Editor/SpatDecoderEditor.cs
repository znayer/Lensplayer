#pragma warning disable CS0169

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace TBE
{
	[CustomEditor(typeof(TBSpatDecoder))]
	public class SpatDecoderEditor : Editor
	{
		
		TBSpatDecoder Decoder_;
		GUIStyle boxStyle;

		private string[] triggerTypeOptions = {"None", "Awake", "Start"};
		private string[] syncModeOptions = {"Internal", "External"};
	
		public override void OnInspectorGUI() 
		{
			Decoder_ = (TBSpatDecoder) target;
			
			EditorGUILayout.Space ();

			Decoder_.streamingAsset = EditorGUILayout.TextField ("Asset Name", Decoder_.streamingAsset);

			Decoder_.loadTriggerType = (TriggerType)EditorGUILayout.Popup ("Load Asset On", (int)Decoder_.loadTriggerType, triggerTypeOptions);

			Decoder_.playTriggerType = (TriggerType)EditorGUILayout.Popup ("Play Asset On", (int)Decoder_.playTriggerType, triggerTypeOptions);

			Decoder_.volume = EditorGUILayout.Slider ("Volume", Decoder_.volume, 0.0f, 1.5f);

			Decoder_.syncMode = (TBSyncMode)EditorGUILayout.Popup ("Sync Mode", (int)Decoder_.syncMode, syncModeOptions);

			Decoder_.focus = EditorGUILayout.Toggle ("Focus", Decoder_.focus);

			EditorGUI.indentLevel = 1;
			
			GUI.enabled = Decoder_.focus;

			Decoder_.followListener = EditorGUILayout.Toggle ("Follow Listener", Decoder_.followListener);

			Decoder_.offFocusLevel = EditorGUILayout.Slider ("Off-focus Level", Decoder_.offFocusLevel, 0.0f, 1.0f);

			Decoder_.focusWidth = EditorGUILayout.Slider ("Width (degrees)", Decoder_.focusWidth, 40.0f, 120.0f);

			EditorGUI.indentLevel = 0;
			GUI.enabled = true;

			EditorGUILayout.Space ();
			
			EditorGUILayout.LabelField("v" + TBE.PInvAudioEngine.getVersion() + " — TwoBigEars.com");
		}
	}
}

