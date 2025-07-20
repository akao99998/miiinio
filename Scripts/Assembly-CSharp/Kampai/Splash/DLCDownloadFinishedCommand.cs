using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Splash
{
	public class DLCDownloadFinishedCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DLCDownloadFinishedCommand") as IKampaiLogger;

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public LoginUserSignal loginSignal { get; set; }

		[Inject]
		public ReInitializeGameSignal reInitializeGameSignal { get; set; }

		[Inject]
		public IAssetsPreloadService assetsPreloadService { get; set; }

		public override void Execute()
		{
			logger.Info("DLCDownloadFinishedCommand");
			if (LoadState.Get() != LoadStateType.STARTED)
			{
				LoadState.Set(LoadStateType.BOOTING);
				if (!localPersistService.HasKeyPlayer("COPPA_Age_Year"))
				{
					assetsPreloadService.PreloadAllAssets();
					loginSignal.Dispatch();
				}
				else
				{
					reInitializeGameSignal.Dispatch(string.Empty);
				}
			}
		}
	}
}
