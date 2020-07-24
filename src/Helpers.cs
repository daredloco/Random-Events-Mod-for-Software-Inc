using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RandomEvents
{
	public class Helpers : ModBehaviour
	{
		#region ControlHelpers
		public static Text AddLabel(string text, Rect labelRect, GUIWindow window, string name = "", bool isBold = false, string tttitle = "", string ttdesc = "")
		{
			Text label = WindowManager.SpawnLabel();

			if (!isBold)
				label.text = text;
			else
				label.text = "<b>" + text + "</b>";

			if (string.IsNullOrEmpty(name))
				label.name = name;
			WindowManager.AddElementToWindow(label.gameObject, window, labelRect, new Rect(0, 0, 0, 0));

			if (string.IsNullOrEmpty(tttitle) && string.IsNullOrEmpty(ttdesc))
				AddTooltip(tttitle, ttdesc, label.gameObject);

			return label;
		}

		public static Button AddButton(string text, Rect rectButton, UnityAction action, GUIWindow window, string tttitle = "", string ttdesc = "")
		{
			Button button = WindowManager.SpawnButton();
			button.GetComponentInChildren<Text>().text = text;
			button.onClick.AddListener(action);
			WindowManager.AddElementToWindow(button.gameObject, window, rectButton, new Rect(0, 0, 0, 0));

			if (string.IsNullOrEmpty(tttitle) && string.IsNullOrEmpty(ttdesc))
				AddTooltip(tttitle, ttdesc, button.gameObject);

			return button;
		}

		public static Slider AddSlider(int min, int max, GUIWindow window, Rect rectSlider, string tttitle = "", string ttdesc = "")
		{
			Slider slider = WindowManager.SpawnSlider();
			slider.minValue = min;
			slider.maxValue = max;
			slider.value = slider.minValue;
			WindowManager.AddElementToWindow(slider.gameObject, window, rectSlider, new Rect(0, 0, 0, 0));

			if (string.IsNullOrEmpty(tttitle) && string.IsNullOrEmpty(ttdesc))
				AddTooltip(tttitle, ttdesc, slider.gameObject);

			return slider;
		}

		public static GUICombobox AddCombobox(string[] items, GUIWindow window, Rect rectBox, string tttitle = "", string ttdesc = "")
		{
			GUICombobox box = WindowManager.SpawnComboBox();
			box.Items.AddRange(items);
			box.Selected = 0;
			WindowManager.AddElementToWindow(box.gameObject, window, rectBox, new Rect(0, 0, 0, 0));

			if (string.IsNullOrEmpty(tttitle) && string.IsNullOrEmpty(ttdesc))
				AddTooltip(tttitle, ttdesc, box.gameObject);

			return box;
		}

		public static GUIListView AddList(GUIWindow window, Rect rectList)
		{
			GUIListView list = WindowManager.SpawnList();
			WindowManager.AddElementToWindow(list.gameObject, window, rectList, new Rect(0, 0, 0, 0));
			return list;
		}

		public static bool AddListColumn(string[] columnlabels, Func<object,object>[] contents, GUIListView list, bool isvolatile = false, int width = 128)
		{
			if (columnlabels.Length != contents.Length)
				return false; 
			
			for (int i = 0; i < contents.Length; i++)
			{
				list.AddColumn(columnlabels[0], contents[i], new Comparison<object>(CompareString), isvolatile, width);
			}
			return true;
		}

		static int CompareString(object s1, object s2)
		{
			return string.Compare((string)s1, (string)s2);
		}

		public static void AddTooltip(string title, string description, GameObject go)
		{
			GUIToolTipper tt = (GUIToolTipper)go.AddComponent(typeof(GUIToolTipper));
			tt.Localize = false;
			tt.TooltipDescription = description;
			tt.ToolTipValue = title;
		}

		public static Toggle AddCheckbox(string label, GUIWindow window, Rect rect, UnityAction<bool> action, bool state = false)
		{
			Toggle t = WindowManager.SpawnCheckbox();
			Text ttxt = t.GetComponentInChildren<Text>();
			t.isOn = state;
			ttxt.text = label;
			ttxt.resizeTextForBestFit = true;
			ttxt.alignment = TextAnchor.MiddleCenter;
			t.onValueChanged.AddListener(action);
			WindowManager.AddElementToWindow(t.gameObject, window, rect, new Rect(0, 0, 0, 0));
			return t;
		}

		public override void OnDeactivate()
		{
			//throw new NotImplementedException();
		}

		public override void OnActivate()
		{
			//throw new NotImplementedException();
		}
		#endregion
	}
}
