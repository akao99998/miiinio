using Kampai.UI.View;
using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.Splash.View
{
	public class NoWiFiView : KampaiView
	{
		public ButtonView continueButton1;

		public ButtonView exitButton1;

		public ButtonView continueButton2;

		public ButtonView exitButton2;

		public ButtonView settingsButton;

		public Text titleText;

		public Text messageText;

		public Text continueText;

		public Text settingsText;

		public void Init(bool twoButtons)
		{
			continueButton1.gameObject.SetActive(twoButtons);
			exitButton1.gameObject.SetActive(twoButtons);
			continueButton2.gameObject.SetActive(!twoButtons);
			exitButton2.gameObject.SetActive(!twoButtons);
			settingsButton.gameObject.SetActive(!twoButtons);
		}
	}
}
