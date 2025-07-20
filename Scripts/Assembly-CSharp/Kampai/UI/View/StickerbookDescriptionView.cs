using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class StickerbookDescriptionView : PopupMenuView
	{
		public Text title;

		public Text description;

		public Text date;

		public RectTransform downArrow;

		private float magicNumber = 0.02f;

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		internal void Display(Vector3 stickerCenter)
		{
			base.Init();
			RectTransform rectTransform = base.transform as RectTransform;
			Vector3 anchoredPosition3D = rectTransform.sizeDelta / 3f;
			anchoredPosition3D.x = 0f;
			rectTransform.anchoredPosition3D = anchoredPosition3D;
			rectTransform.anchorMin = stickerCenter;
			rectTransform.anchorMax = stickerCenter;
			float x = (glassCanvas.transform as RectTransform).sizeDelta.x;
			float num = rectTransform.anchorMin.x - rectTransform.sizeDelta.x / x / 2f;
			if (num < 0f)
			{
				rectTransform.anchorMin = new Vector2(stickerCenter.x - num + magicNumber, stickerCenter.y);
				rectTransform.anchorMax = new Vector2(stickerCenter.x - num + magicNumber, stickerCenter.y);
				downArrow.anchoredPosition = new Vector2(downArrow.anchoredPosition.x + num * x, downArrow.anchoredPosition.y);
			}
			else
			{
				float num2 = rectTransform.anchorMin.x + rectTransform.sizeDelta.x / x / 2f;
				if (num2 > 1f)
				{
					rectTransform.anchorMin = new Vector2(stickerCenter.x - (num2 - 1f) - magicNumber, stickerCenter.y);
					rectTransform.anchorMax = new Vector2(stickerCenter.x - (num2 - 1f) - magicNumber, stickerCenter.y);
					downArrow.anchoredPosition = new Vector2(downArrow.anchoredPosition.x + (num2 - 1f) * x, downArrow.anchoredPosition.y);
				}
			}
			base.Open();
		}

		internal void SetTitle(string localizedString)
		{
			title.text = localizedString;
		}

		internal void SetDescription(bool locked, Sticker sticker, string localizedString, ILocalizationService localizationService)
		{
			if (!locked)
			{
				date.gameObject.SetActive(true);
				date.text = UIUtils.FormatDate(sticker.UTCTimeEarned, localizationService);
			}
			description.text = localizedString;
		}
	}
}
