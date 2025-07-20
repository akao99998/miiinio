using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SettingsPanelView : KampaiView
	{
		public ButtonView DLCButton;

		public ButtonView notificationsButton;

		public ButtonView notificationsOffButton;

		public ButtonView doubleConfirmButton;

		public GameObject notificationsPanel;

		public Text musicValue;

		public Text soundValue;

		public Text server;

		public Text buildNumber;

		public Text DLCText;

		public Text doubleConfirmText;

		public Text notificationsText;

		public Slider MusicSlider;

		public Slider SFXSlider;

		public Signal<bool> volumeSliderChangedSignal = new Signal<bool>();

		public void OnMusicSliderChanged()
		{
			volumeSliderChangedSignal.Dispatch(true);
		}

		public void OnSFXSliderChanged()
		{
			volumeSliderChangedSignal.Dispatch(false);
		}

		internal void ToggleNotificationsOn(bool enable)
		{
			notificationsButton.GetComponent<Button>().interactable = enable;
			notificationsOffButton.gameObject.SetActive(!enable);
		}
	}
}
