using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class HelpTipView : PopupMenuView, IGenericPopupView
	{
		public Text Title;

		public Text Message;

		public GameObject Pointer;

		public GameObject GlassCanvas;

		private ILocalizationService localizationService;

		public void Init(ILocalizationService localizationService)
		{
			base.Init();
			this.localizationService = localizationService;
		}

		public void SetTip(HelpTipDefinition tip)
		{
			Title.text = localizationService.GetString(tip.Title);
			Message.text = localizationService.GetString(tip.Message);
		}

		public void SetUICanvas(GameObject glassCanvas)
		{
			GlassCanvas = glassCanvas;
		}

		public void Display(Vector3 itemCenter)
		{
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.anchoredPosition3D = Vector3.zero;
			float num = 0.01f;
			float num2 = 0f;
			float num3 = (float)Screen.width * num;
			float num4 = itemCenter.x * (float)Screen.width;
			float num5 = 1f;
			if (GlassCanvas != null)
			{
				RectTransform rectTransform2 = GlassCanvas.transform as RectTransform;
				if (rectTransform2 != null)
				{
					num5 = (float)Screen.width / rectTransform2.sizeDelta.x;
				}
			}
			float num6 = rectTransform.offsetMax.x * num5 + num3;
			float num7 = num4 + num6;
			if (num7 > (float)Screen.width)
			{
				num2 = num7 - (float)Screen.width;
			}
			else
			{
				float num8 = rectTransform.offsetMin.x - num3;
				float num9 = num4 + num8;
				if (num9 < 0f)
				{
					num2 = num9;
				}
			}
			itemCenter.x -= num2 / (float)Screen.width;
			Vector3 localPosition = Pointer.gameObject.transform.localPosition;
			localPosition.x += num2 / num5;
			Pointer.gameObject.transform.localPosition = localPosition;
			rectTransform.anchorMin = itemCenter;
			rectTransform.anchorMax = itemCenter;
			base.Open();
		}
	}
}
