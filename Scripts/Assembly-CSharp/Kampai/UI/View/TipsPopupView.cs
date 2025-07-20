using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class TipsPopupView : KampaiView
	{
		public Text TipsText;

		public ButtonView closeButton;

		internal void Display(string text)
		{
			TipsText.text = text;
			base.gameObject.SetActive(true);
		}
	}
}
