﻿/*
	Copyright (c) 2016 Denis Zykov

	This is part of "Charon: Game Data Editor" Unity Plugin.

	Charon Game Data Editor Unity Plugin is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses.
*/

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.GameDevWare.Charon.Windows
{
	abstract class WebViewEditorWindow : EditorWindow, ISerializationCallbackReceiver
	{
		private bool syncingFocus;
		private int repeatedShow;
		private bool webViewHidden;
		[SerializeField]
		private ScriptableObject webView;

		protected Rect Paddings { get; set; }
		protected bool WebViewExists { get { return ReferenceEquals(this.webView, null) == false && this.webView; } }

		protected virtual void OnDestroy()
		{
			if (ReferenceEquals(this.webView, null) == false)
				ScriptableObject.DestroyImmediate(webView);
			this.webView = null;
		}
		protected virtual void OnGUI()
		{
			if (!this.WebViewExists)
				return;

			if (this.webViewHidden)
				return;

			if (this.repeatedShow-- > 0)
			{
				this.webView.Invoke("Hide");
				this.webView.Invoke("Show");
			}

			if (Event.current.type == EventType.Layout)
			{
				var engineAsm = typeof(ScriptableObject).Assembly;
				var webViewRect = (Rect)engineAsm.GetType("UnityEngine.GUIClip", throwOnError: true).InvokeMember("Unclip", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, null, new object[] { new Rect(0, 0, this.position.width, this.position.height) });
				this.webView.Invoke("SetSizeAndPosition", (int)(webViewRect.x + Paddings.x), (int)(webViewRect.y + Paddings.y), (int)(webViewRect.width - (Paddings.width + Paddings.x)), (int)(webViewRect.height - (Paddings.height + Paddings.y)));
			}
		}
		protected virtual void OnBeforeUnload()
		{

		}

		protected void OnFocus()
		{
			this.SetFocus(true);
		}
		protected void OnLostFocus()
		{
			this.SetFocus(false);
		}
		protected void OnBecameInvisible()
		{
			if (!this.WebViewExists) return;
			this.webView.Invoke("Hide");
			this.webView.Invoke("SetHostView", new object[] { null });
		}
		protected void SetFocus(bool focused)
		{
			if (this.syncingFocus)
				return;
			if (!this.WebViewExists)
				return;
			this.syncingFocus = true;

			if (focused)
			{
				var parent = this.GetField("m_Parent");
				if (ReferenceEquals(parent, null) == false)
					this.webView.Invoke("SetHostView", parent);

				if (!this.webViewHidden)
				{
					if (Application.platform != RuntimePlatform.WindowsEditor)
						this.repeatedShow = 5;
					else
						this.webView.Invoke("Show");
				}

			}

			this.webView.Invoke("SetFocus", focused);
			this.syncingFocus = false;
		}
		protected void SetWebViewVisibility(bool visible)
		{
			this.webViewHidden = !visible;

			if (!this.WebViewExists)
				return;

			if (this.webViewHidden)
				this.webView.Invoke("Hide");
			else
				this.SetFocus(true);
		}

		protected void LoadUrl(string url)
		{
			if (!this.webView)
			{
				var editorAsm = typeof(SceneView).Assembly;
				var engineAsm = typeof(ScriptableObject).Assembly;
				var webViewRect = (Rect)engineAsm.GetType("UnityEngine.GUIClip", throwOnError: true).InvokeMember("Unclip", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, null, new object[] { new Rect(0, 0, this.position.width, this.position.height) });
				this.webView = ScriptableObject.CreateInstance(editorAsm.GetType("UnityEditor.WebView", throwOnError: true));
				var hostView = this.GetField("m_Parent");
				this.webView.Invoke("InitWebView", hostView, (int)(webViewRect.x + Paddings.x), (int)(webViewRect.y + Paddings.y), (int)(webViewRect.width - (Paddings.width + Paddings.x)), (int)(webViewRect.height - (Paddings.height + Paddings.y)), false);
				this.webView.SetProperty("hideFlags", HideFlags.HideAndDontSave);

				if (Settings.Current.Verbose)
					Debug.Log("WebViewEditorWindow instantiated new WebView.");
			}
			this.webView.Invoke("SetDelegateObject", this);
			this.webView.Invoke("LoadURL", url);
			if (Settings.Current.Verbose)
				Debug.Log("WebViewEditorWindow is loading '" + url + "'.");
		}
		protected void Reload()
		{
			if (!this.WebViewExists)
				return;

			this.webView.Invoke("Reload");
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			this.OnBeforeUnload();
		}
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
		}
	}
}