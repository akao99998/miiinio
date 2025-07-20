using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;

namespace Kampai.UI.View
{
	public class WatchRewardedAdModalMediator : UIStackMediator<WatchRewardedAdModalView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("WatchRewardedAdModalMediator") as IKampaiLogger;

		private string prefabName;

		private AdPlacementInstance adPlacementInstance;

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IRewardedAdService rewardedAdService { get; set; }

		[Inject]
		public DeclineRewardedAdShowSignal declineRewadedAdShowSignal { get; set; }

		public override void OnRegister()
		{
			base.closeAllOtherMenuSignal.Dispatch(null);
			base.OnRegister();
			base.view.Init(localService);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.YesButton.onClick.AddListener(OnAcceptAd);
			base.view.NoButton.onClick.AddListener(OnDeclineAd);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			CleanupListeners();
		}

		private void CleanupListeners()
		{
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.YesButton.onClick.RemoveListener(OnAcceptAd);
			base.view.NoButton.onClick.RemoveListener(OnDeclineAd);
		}

		public override void Initialize(GUIArguments args)
		{
			prefabName = args.Get<string>();
			adPlacementInstance = args.Get<AdPlacementInstance>();
		}

		protected override void Close()
		{
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			base.view.Close();
		}

		private void OnMenuClose()
		{
			hideSkrimSignal.Dispatch("RewardedAdWatch");
			guiService.Execute(GUIOperation.Unload, prefabName);
		}

		private void OnAcceptAd()
		{
			if (!base.view.IsAnimationPlaying("Close"))
			{
				rewardedAdService.ShowRewardedVideo(adPlacementInstance);
				base.view.Close(true);
			}
		}

		private void OnDeclineAd()
		{
			declineRewadedAdShowSignal.Dispatch(adPlacementInstance);
			base.view.Close();
		}
	}
}
