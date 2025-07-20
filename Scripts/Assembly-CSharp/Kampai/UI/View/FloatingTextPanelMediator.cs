using Elevation.Logging;
using Kampai.Util;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class FloatingTextPanelMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("FloatingTextPanelMediator") as IKampaiLogger;

		[Inject]
		public FloatingTextPanelView view { get; set; }

		[Inject]
		public DisplayFloatingTextSignal displayFloatingTextSignal { get; set; }

		[Inject]
		public RemoveFloatingTextSignal removeFloatingTextSignal { get; set; }

		[Inject]
		public ToggleAllFloatingTextSignal toggleAllFloatingTextSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(logger);
			displayFloatingTextSignal.AddListener(CreateFloatingText);
			removeFloatingTextSignal.AddListener(RemoveFloatingText);
			toggleAllFloatingTextSignal.AddListener(ToggleAllFloatingText);
		}

		public override void OnRemove()
		{
			view.Cleanup();
			displayFloatingTextSignal.RemoveListener(CreateFloatingText);
			removeFloatingTextSignal.RemoveListener(RemoveFloatingText);
			toggleAllFloatingTextSignal.RemoveListener(ToggleAllFloatingText);
		}

		private void CreateFloatingText(FloatingTextSettings settings)
		{
			view.CreateFloatingText(settings);
		}

		private void RemoveFloatingText(int trackedId)
		{
			view.RemoveFloatingText(trackedId);
		}

		private void ToggleAllFloatingText(bool show)
		{
			view.ToggleAllFloatingText(show);
		}
	}
}
