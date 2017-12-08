// 
// Copyright © Hexagon Star Softworks. All Rights Reserved.
// http://www.hexagonstar.com/
// 
// StatsMonitorUtils.cs - version 1.0.0 - Updated 01-02-2016 19:05
//  

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;


namespace StatsMonitor
{
	// ----------------------------------------------------------------------------
	// Enums
	// ----------------------------------------------------------------------------

	public enum SMAlignment : byte
	{
		UpperLeft,
		UpperCenter,
		UpperRight,
		LowerRight,
		LowerCenter,
		LowerLeft
	}

	internal enum SMViewInvalidationType : byte
	{
		All,
		Style,
		Layout
	}


	// ----------------------------------------------------------------------------
	// Stats Monitor Core Classes
	// ----------------------------------------------------------------------------

	internal sealed class SMAnchor
	{
		internal Vector2 position;
		internal Vector2 min;
		internal Vector2 max;
		internal Vector2 pivot;

		internal SMAnchor(float x, float y, float minX, float minY, float maxX, float maxY,
			float pivotX, float pivotY)
		{
			position = new Vector2(x, y);
			min = new Vector2(minX, minY);
			max = new Vector2(maxX, maxY);
			pivot = new Vector2(pivotX, pivotY);
		}
	}


	internal sealed class SMAnchors
	{
		internal SMAnchor upperLeft = new SMAnchor(0, 0, 0, 1, 0, 1, 0, 1);
		internal SMAnchor upperCenter = new SMAnchor(0, 0, .5f, 1, .5f, 1, .5f, 1);
		internal SMAnchor upperRight = new SMAnchor(0, 0, 1, 1, 1, 1, 1, 1);
		internal SMAnchor lowerRight = new SMAnchor(0, 0, 1, 0, 1, 0, 1, 0);
		internal SMAnchor lowerCenter = new SMAnchor(0, 0, .5f, 0, .5f, 0, .5f, 0);
		internal SMAnchor lowerLeft = new SMAnchor(0, 0, 0, 0, 0, 0, 0, 0);
	}


	/// <summary>
	///		A wrapper class for Texture2D that makes it easier to work with a
	///		Texture2D used in UI.
	/// </summary>
	internal sealed class SMBitmap
	{
		internal Texture2D texture;
		internal Color color;
		private readonly Rect _rect;

		internal SMBitmap(int width, int height, Color? color = null)
		{
			texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			texture.filterMode = FilterMode.Point;
			_rect = new Rect(0, 0, width, height);
			this.color = color ?? Color.black;
			Clear();
		}

		internal SMBitmap(float width, float height, Color? color = null)
		{
			texture = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
			texture.filterMode = FilterMode.Point;
			this.color = color ?? Color.black;
			Clear();
		}

		internal void Resize(int width, int height)
		{
			texture.Resize(width, height);
			texture.Apply();
		}

		/// <summary>
		///		Clears the Bitmap2D by filling it with a given color. If color is null
		///		the default color is being used.
		/// </summary>
		internal void Clear(Color? color = null)
		{
			Color c = color ?? this.color;
			Color[] a = texture.GetPixels();
			int i = 0;
			while (i < a.Length) a[i++] = c;
			texture.SetPixels(a);
			texture.Apply();
		}

		/// <summary>
		///		Fills an area in the Bitmap2D with a given color. If rect is null the
		///		whole bitmap is filled. If color is null the default color is used.
		/// </summary>
		internal void FillRect(Rect? rect = null, Color? color = null)
		{
			Rect r = rect ?? _rect;
			Color c = color ?? this.color;
			Color[] a = new Color[(int)(r.width * r.height)];
			int i = 0;
			while (i < a.Length) a[i++] = c;
			texture.SetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height, a);
		}

		/// <summary>
		///		Fills an area in the Bitmap2D with a given color. If rect is null the
		///		whole bitmap is filled. If color is null the default color is used.
		/// </summary>
		internal void FillRect(int x, int y, int w, int h, Color? color = null)
		{
			Color c = color ?? this.color;
			Color[] a = new Color[w * h];
			int i = 0;
			while (i < a.Length) a[i++] = c;
			texture.SetPixels(x, y, w, h, a);
		}

		/// <summary>
		///		Fills a one pixel column in the bitmap.
		/// </summary>
		internal void FillColumn(int x, Color? color = null)
		{
			FillRect(new Rect(x, 0, 1, texture.height), color);
		}

		internal void FillColumn(int x, int y, int height, Color? color = null)
		{
			FillRect(new Rect(x, y, 1, height), color);
		}

		/// <summary>
		///		Fills a one pixel row in the bitmap.
		/// </summary>
		internal void FillRow(int y, Color? color = null)
		{
			FillRect(new Rect(0, y, texture.width, 1), color);
		}

		/// <summary>
		///		Sets a pixel at x, y.
		/// </summary>
		internal void SetPixel(int x, int y, Color color)
		{
			texture.SetPixel(x, y, color);
		}

		/// <summary>
		///		Sets a pixel at x, y.
		/// </summary>
		internal void SetPixel(float x, float y, Color color)
		{
			texture.SetPixel((int)x, (int)y, color);
		}

		///  <summary>
		/// 		Scrolls the bitmap by a certain amount of pixels.
		///  </summary>
		internal void Scroll(int x, Color? fillColor = null)
		{
			x = ~x + 1;
			texture.SetPixels(0, 0, texture.width - x, texture.height,
				texture.GetPixels(x, 0, texture.width - x, texture.height));
			FillRect(texture.width - x, 0, x, texture.height, fillColor);
		}

		/// <summary>
		///		Applies changes to the bitmap.
		/// </summary>
		internal void Apply()
		{
			texture.Apply();
		}

		internal void Dispose()
		{
			Object.Destroy(texture);
		}
	}


	// ----------------------------------------------------------------------------
	// Stats Monitor Util Classes
	// ----------------------------------------------------------------------------

	/// <summary>
	///     A graphics factory class that can be used to quickly create various
	///     UnityEngine.UI.Graphic objects.
	/// </summary>
	internal sealed class SMGraphicsUtil
	{
		internal GameObject parent;
		internal Color defaultColor;
		internal Font defaultFontFace;
		internal int defaultFontSize;
		internal static Vector2 defaultEffectDistance = new Vector2(1, -1);

		/// <summary>
		///     Creates a GraphicsFactory instance.
		/// </summary>
		internal SMGraphicsUtil(GameObject parent, Color defaultColor,
			Font defaultFontFace = null, int defaultFontSize = 16)
		{
			this.parent = parent;
			this.defaultFontFace = defaultFontFace;
			this.defaultFontSize = defaultFontSize;
			this.defaultColor = defaultColor;
		}

		/// <summary>
		///     Creates an object of type UnityEngine.UI.Graphic, wraps it into a
		///     GameObject and applies a RectTransform to it, optionally setting
		///     defaultWidth and panelHeight if specified.
		/// </summary>
		internal Graphic Graphic(string name, Type type, float x = 0, float y = 0,
			float w = 0, float h = 0, Color? color = null)
		{
			GameObject wrapper = new GameObject();
			wrapper.name = name;
			wrapper.transform.parent = parent.transform;
			Graphic g = (Graphic)wrapper.AddComponent(type);
			g.color = color ?? defaultColor;
			RectTransform tr = wrapper.GetComponent<RectTransform>();
			if (tr == null) tr = wrapper.AddComponent<RectTransform>();
			tr.pivot = Vector2.up;
			tr.anchorMin = Vector2.up;
			tr.anchorMax = Vector2.up;
			tr.anchoredPosition = new Vector2(x, y);
			if (w > 0 && h > 0)
			{
				tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
				tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
			}
			return g;
		}

		/// <summary>
		///     Creates an object of type UnityEngine.UI.Image, wraps it into a
		///     GameObject and applies a RectTransform to it, optionally setting
		///     defaultWidth and panelHeight if specified.
		/// </summary>
		internal Image Image(string name, float x = 0, float y = 0, float w = 0,
			float h = 0, Color? color = null)
		{
			return (Image)Graphic(name, typeof(Image), x, y, w, h, color);
		}

		/// <summary>
		///     Creates an object of type UnityEngine.UI.RawImage, wraps it into a
		///     GameObject and applies a RectTransform to it, optionally setting
		///     defaultWidth and panelHeight if specified.
		/// </summary>
		internal RawImage RawImage(string name, float x = 0, float y = 0, float w = 0,
			float h = 0, Color? color = null)
		{
			return (RawImage)Graphic(name, typeof(RawImage), x, y, w, h, color);
		}

		/// <summary>
		///     Creates an object of type UnityEngine.UI.Text, wraps it into a
		///     GameObject and applies a RectTransform to it, optionally setting
		///     defaultWidth and panelHeight if specified.
		/// </summary>
		internal Text Text(string name, float x = 0, float y = 0, float w = 0,
			float h = 0, string text = "", Color? color = null, int fontSize = 0,
			Font fontFace = null, bool fitH = false, bool fitV = false)
		{
			Text t = (Text)Graphic(name, typeof(Text), x, y, w, h, color);
			t.font = fontFace ?? defaultFontFace;
			t.fontSize = fontSize < 1 ? defaultFontSize : fontSize;
			if (fitH) t.horizontalOverflow = HorizontalWrapMode.Overflow;
			if (fitV) t.verticalOverflow = VerticalWrapMode.Overflow;
			t.text = text;
			if (fitH || fitV) FitText(t, fitH, fitV);
			return t;
		}

		///  <summary>
		/// 		Used to create a text field whose size adapts to the text content.
		///  </summary>
		internal Text Text(string name, string text = "", Color? color = null, int fontSize = 0, Font fontFace = null, bool fitH = true, bool fitV = true)
		{
			Text t = (Text)Graphic(name, typeof(Text), 0, 0, 0, 0, color);
			t.font = fontFace ?? defaultFontFace;
			t.fontSize = fontSize < 1 ? defaultFontSize : fontSize;
			if (fitH) t.horizontalOverflow = HorizontalWrapMode.Overflow;
			if (fitV) t.verticalOverflow = VerticalWrapMode.Overflow;
			t.text = text;
			if (fitH || fitV) FitText(t, fitH, fitV);
			return t;
		}

		/// <summary>
		///		Adds a ContentSizeFitter to a given Text.
		/// </summary>
		internal static void FitText(Text text, bool h, bool v)
		{
			ContentSizeFitter csf = text.gameObject.GetComponent<ContentSizeFitter>();
			if (csf == null) csf = text.gameObject.AddComponent<ContentSizeFitter>();
			if (h) csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			if (v) csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		}

		internal static Outline AddOutline(GameObject obj, Color color, Vector2? distance = null)
		{
			Outline outline = obj.GetComponent<Outline>();
			if (outline == null) outline = obj.AddComponent<Outline>();
			outline.effectColor = color;
			outline.effectDistance = distance ?? defaultEffectDistance;
			return outline;
		}

		internal static Shadow AddShadow(GameObject obj, Color color, Vector2? distance = null)
		{
			Shadow shadow = obj.GetComponent<Shadow>();
			if (shadow == null) shadow = obj.AddComponent<Shadow>();
			shadow.effectColor = color;
			shadow.effectDistance = distance ?? defaultEffectDistance;
			return shadow;
		}

		internal static void AddOutlineAndShadow(GameObject obj, Color color, Vector2? distance = null)
		{
			Shadow shadow = obj.GetComponent<Shadow>();
			if (shadow == null) shadow = obj.AddComponent<Shadow>();
			shadow.effectColor = color;
			shadow.effectDistance = distance ?? defaultEffectDistance;
			Outline outline = obj.GetComponent<Outline>();
			if (outline == null) outline = obj.AddComponent<Outline>();
			outline.effectColor = color;
			outline.effectDistance = distance ?? defaultEffectDistance;
		}

		internal static void RemoveEffects(GameObject obj)
		{
			Shadow shadow = obj.GetComponent<Shadow>();
			if (shadow != null) Object.Destroy(shadow);
			Outline outline = obj.GetComponent<Outline>();
			if (outline != null) Object.Destroy(outline);
		}
	}


	/// <summary>
	///		Collection of various required util methods.
	/// </summary>
	internal sealed class SMUtil
	{
		/// <summary>
		///		Remove HTML from string with compiled Regex.
		/// </summary>
		internal static string StripHTMLTags(string s)
		{
			return Regex.Replace(s, "<.*?>", string.Empty);
		}

		/// <summary>
		///		Converts a Color32 object to a hex color string.
		/// </summary>
		internal static string Color32ToHex(Color32 color)
		{
			return color.r.ToString("x2") + color.g.ToString("x2")
				+ color.b.ToString("x2") + color.a.ToString("x2");
		}

		/// <summary>
		///		Converts a RGBA hex color string into a Color32 object.
		/// </summary>
		internal static Color HexToColor32(string hex)
		{
			if (hex.Length < 1) return Color.black;
			return new Color32(byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
				byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
				byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber),
				byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber));
		}

		/// <summary>
		///     normalize value 'a' within range min, max to range 0, 1
		/// </summary>
		public static float Normalize(float a, float min = 0.0f, float max = 255.0f)
		{
			if (max <= min) return 1.0f;
			return (Mathf.Clamp(a, min, max) - min) / (max - min);
		}


		/// <summary>
		///		Resets the transform of given object.
		/// </summary>
		internal static void ResetTransform(GameObject obj)
		{
			obj.transform.position = Vector3.zero;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.rotation = Quaternion.identity;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.one;
		}

		/// <summary>
		///		RectTransform's a given gameObject.
		/// </summary>
		internal static RectTransform RTransform(GameObject obj, Vector2 anchor,
			float x = 0, float y = 0, float w = 0, float h = 0)
		{
			RectTransform t = obj.GetComponent<RectTransform>();
			if (t == null) t = obj.AddComponent<RectTransform>();
			t.pivot = t.anchorMin = t.anchorMax = anchor;
			t.anchoredPosition = new Vector2(x, y);
			if (w > 0.0f)
				t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
			if (h > 0.0f)
				t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
			return t;
		}

		/// <summary>
		///		Adds the given game object to the UI layer if the layer exists.
		/// </summary>
		internal static void AddToUILayer(GameObject obj)
		{
			int uiLayerID = LayerMask.NameToLayer("UI");
			if (uiLayerID > -1) obj.layer = uiLayerID;
		}

		/// <summary>
		///		Returns a scale factor based on 96dpi for the current running screen DPI.
		///		Return -1 if the screen DPI could not be detected. Will not return
		///		a factor that is lower than 1.0.
		/// </summary>
		internal static float DPIScaleFactor(bool round = false)
		{
			float dpi = Screen.dpi;
			if (dpi <= 0) return -1.0f;
			float factor = dpi / 96.0f;
			if (factor < 1.0f) return 1.0f;
			return round ? Mathf.Round(factor) : factor;
		}
	}
}
