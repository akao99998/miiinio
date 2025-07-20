using System.Collections;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class UpdatePlayerDLCTierCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UpdatePlayerDLCTierCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public LaunchDownloadSignal launchDownloadSignal { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public ReloadGameSignal reloadSignal { get; set; }

		[Inject]
		public SavePlayerSignal saveSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		public override void Execute()
		{
			int quantity = (int)playerService.GetQuantity(StaticItem.TIER_ID);
			int quantity2 = (int)playerService.GetQuantity(StaticItem.TIER_GATE_ID);
			logger.Info("UpdatePlayerDLCTierCommand: tier={0} gate={1}", quantity, quantity2);
			dlcService.SetPlayerDLCTier(quantity);
			if (dlcModel.HighestTierDownloaded < quantity2)
			{
				saveSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, string.Empty, true));
				routineRunner.StartCoroutine(WaitAFrame());
			}
			else
			{
				launchDownloadSignal.Dispatch(true);
			}
		}

		private IEnumerator WaitAFrame()
		{
			yield return null;
			logger.Debug("UpdatePlayerDLCTierCommand: Kicking player out of game.");
			reloadSignal.Dispatch();
		}
	}
}
