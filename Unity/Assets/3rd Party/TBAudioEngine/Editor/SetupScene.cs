using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace TBE {

	public class SetupScene : EditorWindow 
	{
		
		bool toggleInitDestroy = true;
		bool toggleCreateListener = true;
		bool toggleScriptExecOrder = true;
		
		static Vector2 windowSize = new Vector2 (300, 200);
		
		public static SetupScene instance { get; private set; }
		
		[MenuItem ("Two Big Ears/Setup Scene")]
		static void Init () 
		{
			// Get existing open window or if none, make a new one:		
			instance = (SetupScene) EditorWindow.GetWindow (typeof(SetupScene));
			instance.ShowUtility ();

#if UNITY_5
			GUIContent Title = new GUIContent();
			Title.text = "Setup/Diagnose";
			instance.titleContent = Title;
#else
			instance.title = "Setup/Diagnose";
#endif		
			instance.minSize = windowSize;
			instance.maxSize = windowSize;
		}
		
		void OnGUI ()
		{	
			EditorGUILayout.Space ();
			
			GUILayout.Label ("TBAudioEngine — Project Setup", EditorStyles.boldLabel);
			
			EditorGUILayout.LabelField ("Chose the options below and click on 'Setup Scene' to automatically setup your scene.\n", EditorStyles.wordWrappedLabel);
			
			toggleInitDestroy = GUILayout.Toggle (toggleInitDestroy, "Create Init/Destroy Components");
			
			toggleCreateListener = GUILayout.Toggle (toggleCreateListener, "Create Listener On Main Camera");
			
			toggleScriptExecOrder = GUILayout.Toggle (toggleScriptExecOrder, "Set Execution Order");
			
			if (GUILayout.Button ("Setup Scene")) 
			{
				if (toggleInitDestroy) 
				{
					createInitDestroy ();
				}
				
				if (toggleCreateListener) 
				{
					createListener ();
				}
				
				if (toggleScriptExecOrder)
				{
					setScriptExecOrderForAll ();
				}
			}
			
			EditorGUILayout.Space ();
		}
		
		void createInitDestroy()
		{
			GameObject engineGlobal = GameObject.Find ("TBAudioEngine");
			
			if (engineGlobal == null) 
			{	
				engineGlobal = new GameObject("TBAudioEngine");
				engineGlobal.AddComponent<TBE.TBEngineInitialise>();
				engineGlobal.AddComponent<TBE.TBEngineDestroy>();
			}
		}
		
		void setScriptExecOrderForAll()
		{
			setScriptExecutionOrder(typeof(TBE.TBEngineInitialise).Name, -200);
			setScriptExecutionOrder(typeof(TBE.TBEngineDestroy).Name, 100);
			setScriptExecutionOrder(typeof(TBE.TBEngineListener).Name, -100);
			setScriptExecutionOrder(typeof(TBE.TBSpatDecoder).Name, -100);
		}
		
		void createListener()
		{
			Camera mainCam = Camera.main;

			if (mainCam != null) 
			{
				TBE.TBEngineListener listener = mainCam.GetComponent<TBE.TBEngineListener> ();
				if (listener == null) 
				{
					mainCam.gameObject.AddComponent<TBE.TBEngineListener>();
				}
			}


		}
		
		void setScriptExecutionOrder(string className, int order)
		{
			foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
			{
				if (monoScript.name == className)
				{	
					MonoImporter.SetExecutionOrder(monoScript, order);
					break;
				}
			}
		}
	}

}


