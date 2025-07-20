using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class MinionPartyCountDownInfoPopupView : PopupMenuView, IGenericPopupView
	{
		public Text CountDownText;

		internal void UpdateCountDownText(string text)
		{
			CountDownText.text = text;
		}

		public void Init(ILocalizationService localizationService)
		{
			base.Init();
		}

		public void Display(Vector3 itemCenter)
		{
			base.Init();
			base.transform.localPosition = Vector3.zero;
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.anchorMin = itemCenter;
			rectTransform.anchorMax = itemCenter;
			base.Open();
		}
	}
}
