// 
// Copyright © Hexagon Star Softworks. All Rights Reserved.
// http://www.hexagonstar.com/
// 
// StatsMonitorViews.cs - version 1.0.0 - Updated 01-02-2016 18:16
//  

using UnityEngine;
using UnityEngine.UI;


namespace StatsMonitor
{
	// ----------------------------------------------------------------------------
	// Baase class for all SM views.
	// ----------------------------------------------------------------------------

	/// <summary>
	///		Base class for 2D views.
	/// </summary>
	internal abstract class SMView
	{
		/// <summary>
		///		The game object of this view. All child objects should be added
		///		inside this game object.
		/// </summary>
		internal GameObject gameObject;
		private RectTransform _rectTransform;
		protected StatsMonitorWidget _statsMonitorWidget;

		/// <summary>
		///		Returns the RectTransform for the view. If the views doesn't have a
		///		RectTransform compoents yet, one is added automatically.
		/// </summary>
		internal RectTransform RTransform
		{
			get
			{
				if (_rectTransform != null) return _rectTransform;
				_rectTransform = gameObject.GetComponent<RectTransform>();
				if (_rectTransform == null) _rectTransform = gameObject.AddComponent<RectTransform>();
				return _rectTransform;
			}
		}

		/// <summary>
		///		The width of the view.
		/// </summary>
		internal float Width
		{
			get { return RTransform.rect.width; }
			set { RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value); }
		}

		/// <summary>
		///		The height of the view.
		/// </summary>
		internal float Height
		{
			get { return RTransform.rect.height; }
			set { RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value); }
		}

		/// <summary>
		///		The X position of the view.
		/// </summary>
		internal float X
		{
			get { return RTransform.anchoredPosition.x; }
			set { RTransform.anchoredPosition = new Vector2(value, Y); }
		}

		/// <summary>
		///		The Y position of the view.
		/// </summary>
		internal float Y
		{
			get { return RTransform.anchoredPosition.y; }
			set { RTransform.anchoredPosition = new Vector2(X, value); }
		}

		/// <summary>
		///		The pivot vector of the view.
		/// </summary>
		internal Vector2 Pivot
		{
			get { return RTransform.pivot; }
			set { RTransform.pivot = value; }
		}

		/// <summary>
		///		The min anchor vector of the view.
		/// </summary>
		internal Vector2 AnchorMin
		{
			get { return RTransform.anchorMin; }
			set { RTransform.anchorMin = value; }
		}

		/// <summary>
		///		The max anchor vector of the view.
		/// </summary>
		internal Vector2 AnchorMax
		{
			get { return RTransform.anchorMax; }
			set { RTransform.anchorMax = value; }
		}

		/// <summary>
		///		Sets the scale of the view.
		/// </summary>
		internal void SetScale(float h = 1.0f, float v = 1.0f)
		{
			RTransform.localScale = new Vector3(h, v, 1.0f);
		}

		/// <summary>
		///		Allows to set all rect transform values at once.
		/// </summary>
		internal void SetRTransformValues(float x, float y, float width, float height, Vector2 pivotAndAnchor)
		{
			RTransform.anchoredPosition = new Vector2(x, y);
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
			Pivot = AnchorMin = AnchorMax = pivotAndAnchor;
		}

		/// <summary>
		///		Invalidates the view. This methods takes care of that all child
		///		objects have been created and then takes care that the style and layout
		///		is updated. This method needs to be called from a sub class constructor!
		/// </summary>
		internal void Invalidate(SMViewInvalidationType type = SMViewInvalidationType.All)
		{
			if (gameObject == null) gameObject = CreateChildren();
			/* Reset Z pos! */
			RTransform.anchoredPosition3D = new Vector3(RTransform.anchoredPosition.x, RTransform.anchoredPosition.y, 0.0f);
			SetScale();
			if (type == SMViewInvalidationType.Style || type == SMViewInvalidationType.All)
				UpdateStyle();
			if (type == SMViewInvalidationType.Layout || type == SMViewInvalidationType.All)
				UpdateLayout();
		}

		/// <summary>
		///		Resets the view and all its child objects.
		/// </summary>
		internal virtual void Reset()
		{
		}

		/// <summary>
		///		Updates the view.
		/// </summary>
		internal virtual void Update()
		{
		}

		/// <summary>
		///		Disposes the view and all its children.
		/// </summary>
		internal virtual void Dispose()
		{
			Destroy(gameObject);
			gameObject = null;
		}

		/// <summary>
		///		Static helper method to destroy child objects.
		/// </summary>
		internal static void Destroy(Object obj)
		{
			Object.Destroy(obj);
		}

		/// <summary>
		///		Used to create any child objects for the view. Should only be called
		///		once per object lifetime. This method must return the game object of
		///		the view!
		/// </summary>
		protected virtual GameObject CreateChildren()
		{
			return null;
		}

		/// <summary>
		///		Used to update the style of child objects. This can be used to make
		///		visual changes that don't require the layout to be updated, for example
		///		color changes.
		/// </summary>
		protected virtual void UpdateStyle()
		{
		}

		/// <summary>
		///		Used to layout all child objects in the view. This updates the
		///		transformations of all children and should be called to change the
		///		size, position, etc. of child objects.
		/// </summary>
		protected virtual void UpdateLayout()
		{
		}
	}


	// ----------------------------------------------------------------------------
	// FPSView
	// ----------------------------------------------------------------------------

	/// <summary>
	///		View class that displays only an FPS counter.
	/// </summary>
	internal class FPSView : SMView
	{
		private Text _text;
		private string[] _fpsTemplates;

		internal FPSView(StatsMonitorWidget statsMonitorWidget)
		{
			_statsMonitorWidget = statsMonitorWidget;
			Invalidate();
		}

		internal override void Reset()
		{
			_text.text = "";
		}

		internal override void Update()
		{
			_text.text = _fpsTemplates[_statsMonitorWidget.fpsLevel] + _statsMonitorWidget.fps + "FPS</color>";
		}

		internal override void Dispose()
		{
			Destroy(_text);
			_text = null;
			base.Dispose();
		}

		protected override GameObject CreateChildren()
		{
			_fpsTemplates = new string[3];

			GameObject container = new GameObject();
			container.name = "FPSView";
			container.transform.parent = _statsMonitorWidget.transform;

			var g = new SMGraphicsUtil(container, _statsMonitorWidget.colorFPS, _statsMonitorWidget.fontFace, _statsMonitorWidget.fontSizeSmall);
			_text = g.Text("Text", "000FPS");
			_text.alignment = TextAnchor.MiddleCenter;

			return container;
		}

		protected override void UpdateStyle()
		{
			_text.font = _statsMonitorWidget.fontFace;
			_text.fontSize = _statsMonitorWidget.FontSizeLarge;
			_text.color = _statsMonitorWidget.colorFPS;

			if (_statsMonitorWidget.colorOutline.a > 0.0f)
				SMGraphicsUtil.AddOutlineAndShadow(_text.gameObject, _statsMonitorWidget.colorOutline);
			else
				SMGraphicsUtil.RemoveEffects(_text.gameObject);

			_fpsTemplates[0] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPS) + ">";
			_fpsTemplates[1] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSWarning) + ">";
			_fpsTemplates[2] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSCritical) + ">";
		}

		protected override void UpdateLayout()
		{
			int padding = _statsMonitorWidget.padding;
			_text.rectTransform.anchoredPosition = new Vector2(padding, -padding);

			/* Center the text object */
			_text.rectTransform.anchoredPosition = Vector2.zero;
			_text.rectTransform.anchorMin = _text.rectTransform.anchorMax = _text.rectTransform.pivot = new Vector2(0.5f, 0.5f);

			/* Update panel size with calculated dimensions. */
			int w = padding + (int)_text.preferredWidth + padding;
			int h = padding + (int)_text.preferredHeight + padding;
			/* Normalize width to even number to prevent texture glitches. */
			w = w % 2 == 0 ? w : w + 1;

			SetRTransformValues(0, 0, w, h, Vector2.one);
		}
	}


	// ----------------------------------------------------------------------------
	// StatsView
	// ----------------------------------------------------------------------------

	/// <summary>
	///		View class that displays the textual stats information.
	/// </summary>
	internal class StatsView : SMView
	{
		private Text _text1;
		private Text _text2;
		private Text _text3;
		private Text _text4;

		private string[] _fpsTemplates;
		private string _fpsMinTemplate;
		private string _fpsMaxTemplate;
		private string _fpsAvgTemplate;
		private string _fxuTemplate;
		private string _msTemplate;
		private string _objTemplate;
		private string _memTotalTemplate;
		private string _memAllocTemplate;
		private string _memMonoTemplate;

		internal StatsView(StatsMonitorWidget statsMonitorWidget)
		{
			_statsMonitorWidget = statsMonitorWidget;
			Invalidate();
		}

		internal override void Reset()
		{
			/* Clear all text fields. */
			_text1.text = _text2.text = _text3.text = _text4.text = "";
		}

		internal override void Update()
		{
			_text1.text = _fpsTemplates[_statsMonitorWidget.fpsLevel] + _statsMonitorWidget.fps + "</color>";

			_text2.text =
				_fpsMinTemplate + (_statsMonitorWidget.fpsMin > -1 ? _statsMonitorWidget.fpsMin : 0) + "</color>\n"
				+ _fpsMaxTemplate + (_statsMonitorWidget.fpsMax > -1 ? _statsMonitorWidget.fpsMax : 0) + "</color>";

			_text3.text =
				_fpsAvgTemplate + _statsMonitorWidget.fpsAvg + "</color> " + _msTemplate + "" + _statsMonitorWidget.ms.ToString("F1") + "MS</color> "
				+ _fxuTemplate + _statsMonitorWidget.fixedUpdateRate + " </color>\n"
				+ _objTemplate + "OBJ:" + _statsMonitorWidget.renderedObjectCount + "/" + _statsMonitorWidget.renderObjectCount
				+ "/" + _statsMonitorWidget.objectCount + "</color>";

			_text4.text =
				_memTotalTemplate + _statsMonitorWidget.memTotal.ToString("F1") + "MB</color> "
				+ _memAllocTemplate + _statsMonitorWidget.memAlloc.ToString("F1") + "MB</color> "
				+ _memMonoTemplate + _statsMonitorWidget.memMono.ToString("F1") + "MB</color>";
		}

		internal override void Dispose()
		{
			Destroy(_text1);
			Destroy(_text2);
			Destroy(_text3);
			Destroy(_text4);
			_text1 = _text2 = _text3 = _text4 = null;
			base.Dispose();
		}

		protected override GameObject CreateChildren()
		{
			_fpsTemplates = new string[3];

			GameObject container = new GameObject();
			container.name = "StatsView";
			container.transform.parent = _statsMonitorWidget.transform;

			var g = new SMGraphicsUtil(container, _statsMonitorWidget.colorFPS, _statsMonitorWidget.fontFace, _statsMonitorWidget.fontSizeSmall);
			_text1 = g.Text("Text1", "FPS:000");
			_text2 = g.Text("Text2", "MIN:000\nMAX:000");
			_text3 = g.Text("Text3", "AVG:000\n[000.0 MS]");
			_text4 = g.Text("Text4", "TOTAL:000.0MB ALLOC:000.0MB MONO:00.0MB");

			return container;
		}

		protected override void UpdateStyle()
		{
			_text1.font = _statsMonitorWidget.fontFace;
			_text1.fontSize = _statsMonitorWidget.FontSizeLarge;
			_text2.font = _statsMonitorWidget.fontFace;
			_text2.fontSize = _statsMonitorWidget.FontSizeSmall;
			_text3.font = _statsMonitorWidget.fontFace;
			_text3.fontSize = _statsMonitorWidget.FontSizeSmall;
			_text4.font = _statsMonitorWidget.fontFace;
			_text4.fontSize = _statsMonitorWidget.FontSizeSmall;

			if (_statsMonitorWidget.colorOutline.a > 0.0f)
			{
				SMGraphicsUtil.AddOutlineAndShadow(_text1.gameObject, _statsMonitorWidget.colorOutline);
				SMGraphicsUtil.AddOutlineAndShadow(_text2.gameObject, _statsMonitorWidget.colorOutline);
				SMGraphicsUtil.AddOutlineAndShadow(_text3.gameObject, _statsMonitorWidget.colorOutline);
				SMGraphicsUtil.AddOutlineAndShadow(_text4.gameObject, _statsMonitorWidget.colorOutline);
			}
			else
			{
				SMGraphicsUtil.RemoveEffects(_text1.gameObject);
				SMGraphicsUtil.RemoveEffects(_text2.gameObject);
				SMGraphicsUtil.RemoveEffects(_text3.gameObject);
				SMGraphicsUtil.RemoveEffects(_text4.gameObject);
			}

			_fpsTemplates[0] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPS) + ">FPS:";
			_fpsTemplates[1] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSWarning) + ">FPS:";
			_fpsTemplates[2] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSCritical) + ">FPS:";
			_fpsMinTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSMin) + ">MIN:";
			_fpsMaxTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSMax) + ">MAX:";
			_fpsAvgTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSAvg) + ">AVG:";
			_fxuTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFXD) + ">FXD:";
			_msTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorMS) + ">";
			_objTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorObjCount) + ">";
			_memTotalTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorMemTotal) + ">TOTAL:";
			_memAllocTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorMemAlloc) + ">ALLOC:";
			_memMonoTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorMemMono) + ">MONO:";
		}

		protected override void UpdateLayout()
		{
			int padding = _statsMonitorWidget.padding;
			int hSpacing = _statsMonitorWidget.spacing;
			int vSpacing = (_statsMonitorWidget.spacing / 4);

			/* Make sure that string lengths keep initial set length before resizing text. */
			_text1.text = PadString(_text1.text, 7, 1);
			_text2.text = PadString(_text2.text.Split('\n')[0], 7, 2);
			_text3.text = PadString(_text3.text.Split('\n')[0], 20, 2);
			_text4.text = PadString(_text4.text, 39, 1);

			_text1.rectTransform.anchoredPosition = new Vector2(padding, -padding);
			int x = padding + (int)_text1.preferredWidth + hSpacing;
			_text2.rectTransform.anchoredPosition = new Vector2(x, -padding);
			x += (int)_text2.preferredWidth + hSpacing;
			_text3.rectTransform.anchoredPosition = new Vector2(x, -padding);
			x = padding;

			/* Workaround for correct preferredHeight which we'd have to wait for the next frame. */
			int text2DoubleHeight = (int)_text2.preferredHeight * 2;
			int y = padding + ((int)_text1.preferredHeight >= text2DoubleHeight ? (int)_text1.preferredHeight : text2DoubleHeight) + vSpacing;
			_text4.rectTransform.anchoredPosition = new Vector2(x, -y);
			y += (int)_text4.preferredHeight + padding;

			/* Update container size. */
			float row1Width = padding + _text1.preferredWidth + hSpacing + _text2.preferredWidth + hSpacing + _text3.preferredWidth + padding;
			float row2Width = padding + _text4.preferredWidth + padding;

			/* Pick larger width & normalize to even number to prevent texture glitches. */
			int w = row1Width > row2Width ? (int)row1Width : (int)row2Width;
			w = w % 2 == 0 ? w : w + 1;

			SetRTransformValues(0, 0, w, y, Vector2.one);
		}

		private static string PadString(string s, int minChars, int numRows)
		{
			s = SMUtil.StripHTMLTags(s);
			if (s.Length >= minChars) return s;
			int len = minChars - s.Length;
			for (int i = 0; i < len; i++)
			{
				s += "_";
			}
			return s;
		}
	}


	// ----------------------------------------------------------------------------
	// GraphView
	// ----------------------------------------------------------------------------

	/// <summary>
	///		View class that displays the stats graph.
	/// </summary>
	internal class GraphView : SMView
	{
		private RawImage _image;
		private SMBitmap _graph;
		private int _oldWidth;
		private int _width;
		private int _height;
		private int _graphStartX;
		private int _graphMaxY;
		private int _memCeiling;
		private int _lastGCCollectionCount = -1;
		private Color?[] _fpsColors;

		public GraphView(StatsMonitorWidget statsMonitorWidget)
		{
			_statsMonitorWidget = statsMonitorWidget;
			Invalidate();
		}

		internal override void Reset()
		{
			if (_graph != null) _graph.Clear();
		}

		internal override void Update()
		{
			if (_graph == null) return;

			/* Total Mem */
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, (int)Mathf.Ceil((_statsMonitorWidget.memTotal / _memCeiling) * _height)), _statsMonitorWidget.colorMemTotal);
			/* Alloc Mem */
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, (int)Mathf.Ceil((_statsMonitorWidget.memAlloc / _memCeiling) * _height)), _statsMonitorWidget.colorMemAlloc);
			/* Mono Mem */
			int monoMem = (int)Mathf.Ceil((_statsMonitorWidget.memMono / _memCeiling) * _height);
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, monoMem), _statsMonitorWidget.colorMemMono);
			/* MS */
			int ms = (int)_statsMonitorWidget.ms >> 1;
			if (ms == monoMem) ms += 1; // Don't overlay mono mem as they are often in the same range.
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, ms), _statsMonitorWidget.colorMS);
			/* FPS */
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, ((_statsMonitorWidget.fps / (_statsMonitorWidget.fpsMax > 60 ? _statsMonitorWidget.fpsMax : 60)) * _graphMaxY) - 1), _statsMonitorWidget.colorFPS);
			/* GC. */
			if (_lastGCCollectionCount != System.GC.CollectionCount(0))
			{
				_lastGCCollectionCount = System.GC.CollectionCount(0);
				_graph.FillColumn(_graphStartX, 0, 5, _statsMonitorWidget.colorGCBlip);
			}

			_graph.Scroll(-1, _fpsColors[_statsMonitorWidget.fpsLevel]);
			_graph.Apply();
		}

		internal override void Dispose()
		{
			if (_graph != null) _graph.Dispose();
			_graph = null;
			Destroy(_image);
			_image = null;
			base.Dispose();
		}

		internal void SetWidth(float width)
		{
			_width = (int)width;
		}

		protected override GameObject CreateChildren()
		{
			_fpsColors = new Color?[3];

			GameObject container = new GameObject();
			container.name = "GraphView";
			container.transform.parent = _statsMonitorWidget.transform;

			_graph = new SMBitmap(10, 10, _statsMonitorWidget.colorGraphBG);

			_image = container.AddComponent<RawImage>();
			_image.rectTransform.sizeDelta = new Vector2(10, 10);
			_image.color = Color.white;
			_image.texture = _graph.texture;

			/* Calculate estimated memory ceiling for application. */
			int sysMem = SystemInfo.systemMemorySize;
			if (sysMem <= 1024) _memCeiling = 512;
			else if (sysMem > 1024 && sysMem <= 2048) _memCeiling = 1024;
			else _memCeiling = 2048;

			return container;
		}

		protected override void UpdateStyle()
		{
			if (_graph != null) _graph.color = _statsMonitorWidget.colorGraphBG;
			if (_statsMonitorWidget.colorOutline.a > 0.0f)
				SMGraphicsUtil.AddOutlineAndShadow(_image.gameObject, _statsMonitorWidget.colorOutline);
			else
				SMGraphicsUtil.RemoveEffects(_image.gameObject);
			_fpsColors[0] = null;
			_fpsColors[1] = new Color(_statsMonitorWidget.colorFPSWarning.r, _statsMonitorWidget.colorFPSWarning.g, _statsMonitorWidget.colorFPSWarning.b, _statsMonitorWidget.colorFPSWarning.a / 4);
			_fpsColors[2] = new Color(_statsMonitorWidget.ColorFPSCritical.r, _statsMonitorWidget.ColorFPSCritical.g, _statsMonitorWidget.ColorFPSCritical.b, _statsMonitorWidget.ColorFPSCritical.a / 4);
		}

		protected override void UpdateLayout()
		{
			/* Make sure that dimensions for text size are valid! */
			if ((_width > 0 && _statsMonitorWidget.graphHeight > 0) && (_statsMonitorWidget.graphHeight != _height || _oldWidth != _width))
			{
				_oldWidth = _width;

				_height = _statsMonitorWidget.graphHeight;
				_height = _height % 2 == 0 ? _height : _height + 1;

				/* The X position in the graph for pixels to be drawn. */
				_graphStartX = _width - 1;
				_graphMaxY = _height - 1;

				_image.rectTransform.sizeDelta = new Vector2(_width, _height);
				_graph.Resize(_width, _height);
				_graph.Clear();

				SetRTransformValues(0, 0, _width, _height, Vector2.one);
			}
		}
	}


	// ----------------------------------------------------------------------------
	// SysInfoView
	// ----------------------------------------------------------------------------

	internal class SysInfoView : SMView
	{
		private int _width;
		private int _height;
		private Text _text;
		private bool _isDirty;

		internal SysInfoView(StatsMonitorWidget statsMonitorWidget)
		{
			_statsMonitorWidget = statsMonitorWidget;
			Invalidate();
		}

		internal override void Reset()
		{
			_text.text = "";
		}

		internal override void Update()
		{
			if (!_isDirty) return;

            string s = ""
                + "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorSysInfoOdd)
                + ">OS:" + SystemInfo.operatingSystem
                + "</color>\n<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorSysInfoEven)
                + ">CPU:" + SystemInfo.processorType
                + " [" + SystemInfo.processorCount + " cores]"
                + "</color>\n<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorSysInfoOdd)
                + ">GRAPHICS:" + SystemInfo.graphicsDeviceName
                + "\nAPI:" + SystemInfo.graphicsDeviceVersion
                + "\nShader Level:" + SystemInfo.graphicsShaderLevel
                + ", Video RAM:" + SystemInfo.graphicsMemorySize + " MB"
                + "</color>\n<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorSysInfoEven)
                + ">SYSTEM RAM:" + SystemInfo.systemMemorySize + " MB"
                + "</color>\n<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorSysInfoOdd)
                + ">SCREEN:" + Screen.currentResolution.width + " x "
                + Screen.currentResolution.height
                + " @" + Screen.currentResolution.refreshRate + "Hz"
                + ",\nwindow size:" + Screen.width + " x " + Screen.height
                + " " + Screen.dpi + "dpi</color>"

                + ",\n\nVideo Name:" + LensPlayer.currentVideoFilename
                + ",\nVideo Resolution:" + LensPlayer.currentVideoResolution;


            _text.text = s;
			_height = _statsMonitorWidget.padding + (int)_text.preferredHeight + _statsMonitorWidget.padding;

			Invalidate(SMViewInvalidationType.Layout);

			/* Invalidate stats monitor once more to update correct height but don't
			 * reinvalidate children or we'd be stuck in a loop! */
			_statsMonitorWidget.Invalidate(SMViewInvalidationType.Layout, StatsMonitorWidget.SMInvalidationFlag.Text, false);

			_isDirty = false;
		}

		internal override void Dispose()
		{
			Destroy(_text);
			_text = null;
			base.Dispose();
		}

		internal void SetWidth(float width)
		{
			_width = (int)width;
		}

		protected override GameObject CreateChildren()
		{
			GameObject container = new GameObject();
			container.name = "SysInfoView";
			container.transform.parent = _statsMonitorWidget.transform;

			var g = new SMGraphicsUtil(container, _statsMonitorWidget.colorFPS, _statsMonitorWidget.fontFace, _statsMonitorWidget.fontSizeSmall);
			_text = g.Text("Text", "", null, 0, null, false);

			return container;
		}

		protected override void UpdateStyle()
		{
			_text.font = _statsMonitorWidget.fontFace;
			_text.fontSize = _statsMonitorWidget.FontSizeSmall;
			if (_statsMonitorWidget.colorOutline.a > 0.0f)
				SMGraphicsUtil.AddOutlineAndShadow(_text.gameObject, _statsMonitorWidget.colorOutline);
			else
				SMGraphicsUtil.RemoveEffects(_text.gameObject);
			_isDirty = true;
		}

		protected override void UpdateLayout()
		{
			int padding = _statsMonitorWidget.padding;

			_text.rectTransform.anchoredPosition = new Vector2(padding, -padding);
			_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _width - (padding * 2));
			_height = padding + (int)_text.preferredHeight + padding;
			SetRTransformValues(0, 0, _width, _height, Vector2.one);
			_isDirty = true;
		}
	}
}
