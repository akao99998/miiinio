using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class SettingsLearnSystemsCategoryMediator : Mediator
	{
		[Inject]
		public SettingsLearnSystemsCategoryView view { get; set; }

		[Inject]
		public SettingsLearnSystemsCategorySelectedSignal categorySelectedSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		public override void OnRegister()
		{
			view.Button.ClickedSignal.AddListener(OnToggleSelected);
			categorySelectedSignal.AddListener(UpdateColor);
		}

		public override void OnRemove()
		{
			view.Button.ClickedSignal.RemoveListener(OnToggleSelected);
			categorySelectedSignal.RemoveListener(UpdateColor);
		}

		private void OnToggleSelected()
		{
			playSFXSignal.Dispatch("Play_button_click_01");
			categorySelectedSignal.Dispatch(view.Definition.ID);
		}

		private void UpdateColor(int origin)
		{
			if (origin != view.Definition.ID)
			{
				view.toggleImage.gameObject.SetActive(false);
			}
			else
			{
				view.toggleImage.gameObject.SetActive(true);
			}
		}
	}
}
