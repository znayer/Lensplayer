﻿// 
// Copyright © Hexagon Star Softworks. All Rights Reserved.
// http://www.hexagonstar.com/
// 
// StatsMonitorEditor.cs - version 1.0.0 - Updated 01-02-2016 17:41
//  

using UnityEditor;
using UnityEngine;


namespace StatsMonitor
{
	[CustomEditor(typeof(StatsMonitor))]
	internal sealed class StatsMonitorEditor : Editor
	{
		// ----------------------------------------------------------------------------
		// Properties
		// ----------------------------------------------------------------------------

		private StatsMonitor _self;


		// ----------------------------------------------------------------------------
		// Unity Editor Callbacks
		// ----------------------------------------------------------------------------

		internal void OnEnable()
		{
			_self = (target as StatsMonitor);
		}


		public override void OnInspectorGUI()
		{
			if (_self == null) return;
			serializedObject.Update();
			EditorGUILayout.Space();

			if (GUILayout.Button("Stats Monitor Parameters"))
			{
				Selection.activeObject = _self.transform.Find(StatsMonitorWidget.OBJECT_NAME);
			}

			EditorGUILayout.Space();

			EditorGUILayout.HelpBox(StatsMonitor.NAME + " v" + StatsMonitor.VERSION + " by Hexagon Star Softworks"
				
				+ "\n\n\nSTATS DESCRIPTION:"
				+ "\n\nFPS: The current frames per second at which the game is rendered. The Update loop is running at this rate."
				+ "\n\nMIN: The lowest measured FPS, since starting the game."
				+ "\n\nMAX: The highest measured FPS, since starting the game."
				+ "\n\nAVG: The average FPS, measured within the last average FPS samples cycle (by default 50 samples)."
				+ "\n\nMS: The milliseconds that a frame needs to render at the current framerate."
				+ "\n\nFXD: The Fixed Update loop rate. This is a calculated value at which the fixed update loop is running. By default Unity is running with a Fixed Timestep 0.02 which represents a rate of 50. You could understand this as 'framerate' for the fixed update loop but keep in mind that the fixed update loop is running at a constant time and isn't influenced by framerate fluctuations."
				+ "\n\nOBJ: Display's the current scene's object count in the following order: Currently Rendered Objects / Total Render Objects* / Total Objects (*Render Objects are objects that have a Renderer component)."
				+ "\n\nTOTAL: The total private memory reserved by the OS for the game. This is essentially a pool of memory that the OS decides to reserve for the game but the game might not necessarily use all of it all the time."
				+ "\n\nALLOC: The amount of private memory that is currently used by the game."
				+ "\n\nMONO: The amount of memory that is currently allocated for Mono objects or in other words: every object that derives from UnityEngine.Object."

				, MessageType.None);
			EditorGUILayout.Space();
			serializedObject.ApplyModifiedProperties();
		}
	}
}
