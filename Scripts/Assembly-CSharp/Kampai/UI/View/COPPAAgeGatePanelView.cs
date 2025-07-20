using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class COPPAAgeGatePanelView : KampaiView
	{
		public Slider AgeSlider;

		public Text AgeText;

		public ButtonView TOSButton;

		public ButtonView EULAButton;

		public ButtonView PrivacyButton;

		public ButtonView DeclineButton;

		public ButtonView AcceptButton;

		private float lastSoundPlayed;

		private PlayGlobalSoundFXSignal soundFXSignal;

		private ITimeService timeService;

		internal void Init(PlayGlobalSoundFXSignal soundSignal, ITimeService time)
		{
			AcceptButton.enabled = true;
			soundFXSignal = soundSignal;
			timeService = time;
		}

		public void OnSliderChanged()
		{
			if (AgeSlider != null && AgeText != null)
			{
				float num = (float)timeService.AppTime() - lastSoundPlayed;
				if (num >= 0.17f)
				{
					soundFXSignal.Dispatch("Play_minion_confirm_select_02");
					lastSoundPlayed = timeService.AppTime();
				}
				AgeText.text = AgeSlider.value.ToString();
				bool flag = AgeSlider.value > 0f;
				Button component = AcceptButton.gameObject.GetComponent<Button>();
				if (component.interactable != flag)
				{
					component.interactable = flag;
					component.gameObject.SetActive(false);
					component.gameObject.SetActive(true);
				}
			}
		}
	}
}
