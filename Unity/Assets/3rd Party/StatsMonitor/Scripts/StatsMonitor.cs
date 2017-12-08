// 
// Copyright © Hexagon Star Softworks. All Rights Reserved.
// http://www.hexagonstar.com/
// 
// StatsMonitor.cs - version 1.0.0 - Updated 01-02-2016 18:12
//  

using UnityEngine;


namespace StatsMonitor
{
	/// <summary>
	///		A wrapper for the actual stats monitor class (StatsMonitor)
	///		which does the main work for stats monitor. This class provides a wrapping
	///		UI canvas for the Stats Monitor.
	/// </summary>
	[DisallowMultipleComponent]
	public class StatsMonitor : MonoBehaviour
	{
		// ----------------------------------------------------------------------------
		// Constants
		// ----------------------------------------------------------------------------

		public const string NAME = "Stats Monitor";
		public const string VERSION = "1.3.3";


		// ----------------------------------------------------------------------------
		// Properties
		// ----------------------------------------------------------------------------

		internal static readonly SMAnchors anchors = new SMAnchors();

		public static StatsMonitor instance { get; private set; }
		private StatsMonitorWidget _statsMonitorWidget;
		private Canvas _canvas;


		// ----------------------------------------------------------------------------
		// Accessors
		// ----------------------------------------------------------------------------

		private static StatsMonitor InternalInstance
		{
			get
			{
				if (instance == null)
				{
					StatsMonitor statsMonitor = FindObjectOfType<StatsMonitor>();
					if (statsMonitor != null)
					{
						instance = statsMonitor;
					}
					else
					{
						GameObject container = new GameObject(NAME);
						container.AddComponent<StatsMonitor>();
					}
				}
				return instance;
			}
		}


		public static StatsMonitorWidget Widget
		{
			get
			{
				return instance._statsMonitorWidget;
			}
		}


		// ----------------------------------------------------------------------------
		// Public Methods
		// ----------------------------------------------------------------------------

		public static StatsMonitor AddToScene()
		{
			return InternalInstance;
		}


		public void SetRenderMode(SMRenderMode renderMode)
		{
			if (renderMode == SMRenderMode.Overlay)
			{
				_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				_canvas.sortingOrder = short.MaxValue;
			}
			else if (renderMode == SMRenderMode.Camera)
			{
				Camera cam = Camera.current ?? Camera.main;
				_canvas.renderMode = RenderMode.ScreenSpaceCamera;
				_canvas.worldCamera = cam;
				_canvas.planeDistance = cam.nearClipPlane;
					/* Set SM to be on the front-most layer since we can't create a sorting layer
					   via script! */
					_canvas.sortingLayerName = SortingLayer.layers[SortingLayer.layers.Length - 1].name;
				_canvas.sortingOrder = short.MaxValue;
			}
            else if (renderMode == SMRenderMode.World)
            {
                _canvas.renderMode = RenderMode.WorldSpace;
                _canvas.GetComponent<RectTransform>().position = new Vector3(1.5f,3.3f,6);
                //_canvas.GetComponent<RectTransform>().localScale = Vector3.one * 0.02f;
//                _canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 400);
                _canvas.sortingOrder = short.MaxValue;
            }
        }


		// ----------------------------------------------------------------------------
		// Protected & Private Methods
		// ----------------------------------------------------------------------------

		private void CreateUI()
		{
			/* Create UI canvas used for all StatsMonitorWrapper components. */
			_canvas = gameObject.AddComponent<Canvas>();
			_canvas.pixelPerfect = true;

			RectTransform tr = gameObject.GetComponent<RectTransform>();
			tr.pivot = Vector2.up;
			tr.anchorMin = Vector2.up;
			tr.anchorMax = Vector2.up;
			tr.anchoredPosition = new Vector2(0.0f, 0.0f);

			/* Find the widget game object to enable it on start. */
			Transform widgetTransform = transform.Find(StatsMonitorWidget.OBJECT_NAME);
			if (widgetTransform == null)
			{
				Debug.LogError(StatsMonitorWidget.OBJECT_NAME + " not found as a child of " + NAME + ".");
			}
			else
			{
				widgetTransform.gameObject.SetActive(true);

				/* Find StatsMonitor child object. */
				_statsMonitorWidget = FindObjectOfType<StatsMonitorWidget>();
				if (_statsMonitorWidget != null) _statsMonitorWidget.wrapper = this;
				else Debug.LogError(StatsMonitorWidget.OBJECT_NAME + " object has no '" + StatsMonitorWidget.OBJECT_NAME + "' component on it!");
			}
		}


		private void DisposeInternal()
		{
			if (_statsMonitorWidget != null) _statsMonitorWidget.Dispose();
			Destroy(this);
			if (instance == this) instance = null;
		}


		// ----------------------------------------------------------------------------
		// Unity Callbacks
		// ----------------------------------------------------------------------------

		private void Awake()
		{
			if (instance == null) instance = this;

			if (transform.parent != null)
			{
				Debug.LogWarning("Stats Monitor has been moved to root. It needs to be in root to function properly. To add Stats Monitor to a scene always use the menu 'Game Object/Create Other/Stats Monitor'.");
				transform.parent = null;
			}

			DontDestroyOnLoad(gameObject);
			CreateUI();
			if (_statsMonitorWidget != null) SetRenderMode(_statsMonitorWidget.RenderMode);
			SMUtil.AddToUILayer(gameObject);
		}


		// ----------------------------------------------------------------------------
		// Editor Integration
		// ----------------------------------------------------------------------------

#if UNITY_EDITOR
		private const string MENU_PATH = "GameObject/Create Other/" + NAME;

		[UnityEditor.MenuItem(MENU_PATH, false)]
		private static void AddToSceneInEditor()
		{
			StatsMonitor statsMonitor = FindObjectOfType<StatsMonitor>();
			if (statsMonitor == null)
			{
				GameObject wrapper = GameObject.Find(NAME);
				if (wrapper == null)
				{
					wrapper = new GameObject(NAME);
					UnityEditor.Undo.RegisterCreatedObjectUndo(wrapper, "Create " + NAME);
					SMUtil.ResetTransform(wrapper);
					SMUtil.AddToUILayer(wrapper);
					wrapper.AddComponent<StatsMonitor>();
					
					GameObject widget = new GameObject(StatsMonitorWidget.OBJECT_NAME);
					widget.transform.parent = wrapper.transform;
					widget.AddComponent<RectTransform>();
					widget.AddComponent<StatsMonitorWidget>();
					SMUtil.AddToUILayer(widget);
					widget.SetActive(false);
				}
				else
				{
					Debug.LogWarning("Another object named " + NAME
						+ " already exists in the scene! Rename or delete it"
						+ " before trying to add " + NAME + ".");
					
				}
				UnityEditor.Selection.activeObject = wrapper;
			}
			else
			{
				Debug.LogWarning(NAME + " already exists in the scene!");
			}
		}
	#endif
	}
}
