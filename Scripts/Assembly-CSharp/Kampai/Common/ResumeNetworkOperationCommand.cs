using Elevation.Logging;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ResumeNetworkOperationCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("ResumeNetworkOperationCommand") as IKampaiLogger;

		[Inject]
		public ReloadGameSignal reloadGameSignal { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		public override void Execute()
		{
			LoadStateType loadStateType = LoadState.Get();
			logger.Info("ResumeNetworkOperationCommand: {0}", loadStateType.ToString());
			switch (loadStateType)
			{
			case LoadStateType.BOOTING:
				reloadGameSignal.Dispatch();
				break;
			case LoadStateType.INTRO:
				logger.Info("IntroPlaying");
				break;
			case LoadStateType.DLC:
				if (dlcModel.PendingRequests == null || dlcModel.RunningRequests == null)
				{
					reloadGameSignal.Dispatch();
				}
				break;
			default:
				logger.Info("GameRunning");
				break;
			}
		}
	}
}
