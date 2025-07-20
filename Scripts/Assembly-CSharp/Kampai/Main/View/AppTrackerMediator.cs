using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.mediation.impl;

namespace Kampai.Main.View
{
	public class AppTrackerMediator : Mediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("AppTrackerMediator") as IKampaiLogger;

		[Inject]
		public AppTrackerView view { get; set; }

		[Inject]
		public AppPauseSignal pauseSignal { get; set; }

		[Inject]
		public AppResumeSignal resumeSignal { get; set; }

		[Inject]
		public AppQuitSignal quitSignal { get; set; }

		[Inject]
		public AppFocusGainedSignal focusGainedSignal { get; set; }

		[Inject]
		public AppEarlyPauseSignal earlyPauseSignal { get; set; }

		public override void OnRegister()
		{
			logger.Debug("AppTrackerMediator.OnRegister");
			view.pauseSignal = pauseSignal;
			view.resumeSignal = resumeSignal;
			view.quitSignal = quitSignal;
			view.focusGainedSignal = focusGainedSignal;
			view.earlyPauseSignal = earlyPauseSignal;
			view.logger = logger;
			view.SetIsInitialized(true);
		}
	}
}
