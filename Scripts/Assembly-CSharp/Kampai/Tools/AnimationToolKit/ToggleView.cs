using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.Tools.AnimationToolKit
{
	public class ToggleView : KampaiView
	{
		internal void SetLabel(string labelText)
		{
			Text componentInChildren = GetComponentInChildren<Text>();
			componentInChildren.text = labelText;
		}

		internal void SetPosition(Vector3 position)
		{
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.localPosition = position;
		}
	}
}
