using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class SettingsLearnSystemsCategoryItemMediator : Mediator
	{
		[Inject]
		public SettingsLearnSystemsCategoryItemView view { get; set; }

		[Inject]
		public SettingsLearnSystemsCategoryItemSelectedSignal categoryItemSelectedSignal { get; set; }

		public override void OnRegister()
		{
			view.Button.ClickedSignal.AddListener(OnClick);
		}

		public override void OnRemove()
		{
			view.Button.ClickedSignal.RemoveListener(OnClick);
		}

		private void OnClick()
		{
			categoryItemSelectedSignal.Dispatch(view.Definition.ID);
		}
	}
}
