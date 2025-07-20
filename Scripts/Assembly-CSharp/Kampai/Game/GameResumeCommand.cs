using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Main;
using Kampai.Splash;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class GameResumeCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("GameResumeCommand") as IKampaiLogger;

		[Inject]
		public SavePlayerSignal savePlayerSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CancelAllNotificationSignal signal { get; set; }

		[Inject]
		public RestoreCraftingBuildingsSignal restoreCraftingSignal { get; set; }

		[Inject]
		public TeleportMinionToBuildingSignal teleportSignal { get; set; }

		[Inject]
		public RefreshQueueSlotSignal updateQueueSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal changeStateSignal { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistence { get; set; }

		[Inject]
		public IClientHealthService clientHealthService { get; set; }

		[Inject]
		public MuteVolumeSignal muteVolumeSignal { get; set; }

		[Inject]
		public AppResumeCompletedSignal appResumeCompletedSignal { get; set; }

		[Inject]
		public ReconcileDLCSignal reconcileDLCSignal { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public IBackgroundDownloadDlcService backgroundDownloadDlcService { get; set; }

		[Inject]
		public LoadServerSalesSignal loadServerSalesSignal { get; set; }

		[Inject]
		public ReloadGameSignal reloadGameSignal { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public CheckAvailableStorageSignal checkAvailableStorageSignal { get; set; }

		private void updateBuildings()
		{
			foreach (Building item in playerService.GetInstancesByType<Building>())
			{
				TaskableBuilding taskableBuilding = item as TaskableBuilding;
				if (taskableBuilding != null)
				{
					if (taskableBuilding.GetNumCompleteMinions() > 0)
					{
						changeStateSignal.Dispatch(item.ID, BuildingState.Harvestable);
					}
					continue;
				}
				CraftingBuilding craftingBuilding = item as CraftingBuilding;
				if (craftingBuilding != null)
				{
					restoreCraftingSignal.Dispatch(craftingBuilding);
				}
			}
		}

		public override void Execute()
		{
			logger.Info("GameResumeCommand");
			Native.LogInfo(string.Format("AppResume, GameResumeCommand, realtimeSinceStartup: {0}", timeService.RealtimeSinceStartup()));
			playerDurationService.OnAppResume();
			currencyService.RefreshCatalog();
			LoadState.Set(LoadStateType.STARTED);
			if (ShouldSaveGame())
			{
				savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, string.Empty, false));
			}
			muteVolumeSignal.Dispatch();
			signal.Dispatch();
			foreach (Minion item in playerService.GetInstancesByType<Minion>())
			{
				if (item.State == MinionState.Tasking)
				{
					teleportSignal.Dispatch(item.ID);
				}
			}
			updateBuildings();
			updateQueueSignal.Dispatch(false);
			clientHealthService.MarkMeterEvent("AppFlow.Resume");
			loadServerSalesSignal.Dispatch();
			currencyService.ResumeTransactionsHandling();
			appResumeCompletedSignal.Dispatch();
			ReconcileDlc();
		}

		private bool ShouldSaveGame()
		{
			if (localPersistence.GetData("SocialInProgress").Equals("True"))
			{
				return false;
			}
			if (localPersistence.GetData("MtxPurchaseInProgress").Equals("True"))
			{
				return false;
			}
			if (localPersistence.GetData("ExternalLinkOpened").Equals("True"))
			{
				localPersistence.PutData("ExternalLinkOpened", "False");
				return false;
			}
			if (localPersistence.GetData("AdVideoInProgress").Equals("True"))
			{
				logger.Debug("Ads: GameResumeCommand.ShouldSaveGame(): reset AdVideoInProgress flag");
				localPersistence.PutData("AdVideoInProgress", "False");
				return false;
			}
			return true;
		}

		private void ReconcileDlc()
		{
			reconcileDLCSignal.Dispatch(false);
			if (dlcModel.NeededBundles == null)
			{
				return;
			}
			int count = dlcModel.NeededBundles.Count;
			if (count != 0)
			{
				if (LoadState.Get() != LoadStateType.DLC || (count < 5 && (dlcModel.AllowDownloadOnMobileNetwork || networkModel.reachability == NetworkReachability.ReachableViaLocalAreaNetwork)))
				{
					checkAvailableStorageSignal.Dispatch(GameConstants.PERSISTENT_DATA_PATH, dlcModel.TotalSize, backgroundDownloadDlcService.Start);
				}
				else
				{
					reloadGameSignal.Dispatch();
				}
			}
		}
	}
}
