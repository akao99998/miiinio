using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class ButtonView : KampaiView
	{
		public bool PlaySoundOnClick = true;

		public string AudioButtonClick = "Play_button_click_01";

		public Signal ClickedSignal = new Signal();

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		public virtual void OnClickEvent()
		{
			if (PlaySoundOnClick)
			{
				playSFXSignal.Dispatch(AudioButtonClick);
			}
			ClickedSignal.Dispatch();
		}

		public void StartButtonPulse(bool isTrue)
		{
			Animator component = base.gameObject.GetComponent<Animator>();
			if (!(component == null))
			{
				component.SetBool("Pulse", isTrue);
			}
		}
	}
}
