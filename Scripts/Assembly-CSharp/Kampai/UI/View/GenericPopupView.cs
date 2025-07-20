using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class GenericPopupView : PopupMenuView, IGenericPopupView
	{
		public Text itemName;

		public Text itemDuration;

		public Text itemOrigin;

		public KampaiImage itemDurationIcon;

		public ScrollableButtonView gotoButton;

		public float offsetValue = 1.8f;

		private ILocalizationService localizationService;

		public void Init(ILocalizationService localizationService)
		{
			base.Init();
			this.localizationService = localizationService;
		}

		public void Display(Vector3 itemCenter)
		{
			base.Init();
			RectTransform rectTransform = base.transform as RectTransform;
			Vector3 anchoredPosition3D = rectTransform.sizeDelta / offsetValue;
			anchoredPosition3D.x = 0f;
			rectTransform.anchoredPosition3D = anchoredPosition3D;
			rectTransform.anchorMin = itemCenter;
			rectTransform.anchorMax = itemCenter;
			base.Open();
		}

		internal void SetName(string localizedName)
		{
			if (itemName != null)
			{
				itemName.text = localizedName;
			}
		}

		internal void SetTime(int duration)
		{
			if (itemDuration != null)
			{
				itemDuration.text = UIUtils.FormatTime(duration, localizationService);
			}
		}

		internal void SetItemOrigin(string localizedOrigin)
		{
			if (itemOrigin != null)
			{
				itemOrigin.text = localizedOrigin;
			}
		}

		internal void ShowGotoButton()
		{
			gotoButton.gameObject.SetActive(true);
		}

		internal void DisableDurationInfo()
		{
			itemDuration.enabled = false;
			itemDurationIcon.enabled = false;
		}
	}
}
