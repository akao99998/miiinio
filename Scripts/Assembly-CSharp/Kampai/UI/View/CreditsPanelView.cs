using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class CreditsPanelView : KampaiView
	{
		public ScrollRect scrollRect;

		public Image overlayImage;

		public ButtonView closeButton;

		public Text creditText;

		internal List<Text> textList = new List<Text>();

		internal float scrolledOffsetY;

		private bool initialized;

		internal void SetupDivisions(float firstTextHeight)
		{
			float num = firstTextHeight;
			foreach (Text text in textList)
			{
				float num2 = elementHeight(text.gameObject);
				text.rectTransform.anchoredPosition = new Vector2(0f, 0f - num);
				num += num2;
				text.gameObject.SetActive(false);
			}
			scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, num);
			scrolledOffsetY = 0.01f;
		}

		public void Update()
		{
			float y = scrollRect.content.anchoredPosition.y;
			if (y == scrolledOffsetY && initialized)
			{
				return;
			}
			initialized = true;
			scrolledOffsetY = y;
			float num = elementHeight(base.gameObject);
			foreach (Text text in textList)
			{
				RectTransform rectTransform = text.gameObject.transform as RectTransform;
				float num2 = rectTransform.offsetMax.y + scrolledOffsetY;
				float num3 = rectTransform.offsetMin.y + scrolledOffsetY;
				text.gameObject.SetActive(num3 < 0f && num2 > 0f - num);
			}
		}

		internal void Cleanup()
		{
			textList.Clear();
		}

		private float elementHeight(GameObject go)
		{
			return (go.transform as RectTransform).rect.height;
		}
	}
}
