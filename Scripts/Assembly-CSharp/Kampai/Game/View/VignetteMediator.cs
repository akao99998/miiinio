using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class VignetteMediator : Mediator
	{
		[Inject]
		public VignetteView view { get; set; }

		[Inject]
		public ToggleVignetteSignal toggleSignal { get; set; }

		public override void OnRegister()
		{
			toggleSignal.AddListener(Toggle);
			view.gameObject.SetActive(false);
		}

		public override void OnRemove()
		{
			toggleSignal.RemoveListener(Toggle);
		}

		private void Toggle(bool enable, float? size)
		{
			if (view != null)
			{
				if (enable)
				{
					view.SetVignetteSize(size);
				}
				view.gameObject.SetActive(enable);
			}
		}
	}
}
