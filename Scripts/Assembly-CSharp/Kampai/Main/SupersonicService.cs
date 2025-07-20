using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;

namespace Kampai.Main
{
	public class SupersonicService : ISupersonicService
	{
		private const string TAG = "KampaiSupersonic: ";

		private IKampaiLogger logger = LogManager.GetClassLogger("SupersonicService") as IKampaiLogger;

		private bool initialized;

		private bool adRewarded;

		[Inject]
		public ReInitializeGameSignal reInitializeGameSignal { get; set; }

		[Inject]
		public AppPauseSignal appPauseSignal { get; set; }

		[Inject]
		public AppResumeSignal appResumeSignal { get; set; }

		[Inject]
		public SupersonicVideoAdAvailabilityChangedSignal videoAdAvailabilityChangedSignal { get; set; }

		[Inject]
		public SupersonicVideoAdShowSignal videoAdShowSignal { get; set; }

		[Inject]
		public SupersonicVideoAdRewardedSignal videoAdRewardedSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		public void Init()
		{
			string sUPERSONIC_APP_ID = GameConstants.StaticConfig.SUPERSONIC_APP_ID;
			logger.Debug("{0}Init(): Supersonic app ID: {1}", "KampaiSupersonic: ", sUPERSONIC_APP_ID);
			SubscribeOnAppLifecycleEvents(true);
			Supersonic agent = Supersonic.Agent;
			logger.Debug("{0}Init(): agent start", "KampaiSupersonic: ");
			agent.start();
			agent.reportAppStarted();
			SupersonicConfig.Instance.setClientSideCallbacks(true);
			SubscribeOnSupersonicEvents(true);
			logger.Debug("{0}Init(): validation integration", "KampaiSupersonic: ");
			agent.validateIntegration();
			string advertiserId = agent.getAdvertiserId();
			agent.initRewardedVideo(sUPERSONIC_APP_ID, advertiserId);
			localPersistanceService.PutData("AdVideoInProgress", "False");
			initialized = true;
		}

		public bool IsRewardedVideoAvailable()
		{
			if (!initialized)
			{
				return false;
			}
			return Supersonic.Agent.isRewardedVideoAvailable();
		}

		public void ShowRewardedVideo(string placement = null)
		{
			if (initialized)
			{
				videoAdShowSignal.Dispatch();
				localPersistanceService.PutData("AdVideoInProgress", "True");
				if (string.IsNullOrEmpty(placement))
				{
					Supersonic.Agent.showRewardedVideo();
				}
				else
				{
					Supersonic.Agent.showRewardedVideo(placement);
				}
			}
		}

		public void ShowOfferwall()
		{
			if (initialized)
			{
				Supersonic.Agent.showOfferwall();
			}
		}

		private void SubscribeOnAppLifecycleEvents(bool subscribe)
		{
			if (subscribe)
			{
				reInitializeGameSignal.AddListener(OnGameReinitialize);
				appPauseSignal.AddListener(OnAppPause);
				appResumeSignal.AddListener(OnAppResume);
			}
			else
			{
				reInitializeGameSignal.RemoveListener(OnGameReinitialize);
				appPauseSignal.RemoveListener(OnAppPause);
				appResumeSignal.RemoveListener(OnAppResume);
			}
		}

		private void OnAppResume()
		{
			if (initialized)
			{
				Supersonic.Agent.onResume();
			}
		}

		private void OnAppPause()
		{
			if (initialized)
			{
				Supersonic.Agent.onPause();
			}
		}

		private void OnGameReinitialize(string levelToLoad)
		{
			if (initialized)
			{
				SubscribeOnSupersonicEvents(false);
				SubscribeOnAppLifecycleEvents(false);
			}
		}

		private void SubscribeOnSupersonicEvents(bool subscribe)
		{
			if (subscribe)
			{
				SupersonicEvents.onRewardedVideoInitSuccessEvent += RewardedVideoInitSuccessEvent;
				SupersonicEvents.onRewardedVideoInitFailEvent += RewardedVideoInitFailEvent;
				SupersonicEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
				SupersonicEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
				SupersonicEvents.onVideoAvailabilityChangedEvent += VideoAvailabilityChangedEvent;
				SupersonicEvents.onVideoStartEvent += VideoStartEvent;
				SupersonicEvents.onVideoEndEvent += VideoEndEvent;
				SupersonicEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
			}
			else
			{
				SupersonicEvents.onRewardedVideoInitSuccessEvent -= RewardedVideoInitSuccessEvent;
				SupersonicEvents.onRewardedVideoInitFailEvent -= RewardedVideoInitFailEvent;
				SupersonicEvents.onRewardedVideoAdOpenedEvent -= RewardedVideoAdOpenedEvent;
				SupersonicEvents.onRewardedVideoAdClosedEvent -= RewardedVideoAdClosedEvent;
				SupersonicEvents.onVideoAvailabilityChangedEvent -= VideoAvailabilityChangedEvent;
				SupersonicEvents.onVideoStartEvent -= VideoStartEvent;
				SupersonicEvents.onVideoEndEvent -= VideoEndEvent;
				SupersonicEvents.onRewardedVideoAdRewardedEvent -= RewardedVideoAdRewardedEvent;
			}
		}

		private void VideoAvailabilityChangedEvent(bool canShowAd)
		{
			logger.Debug("{0}VideoAvailabilityChangedEvent() canShowAd: {1}", "KampaiSupersonic: ", canShowAd);
			videoAdAvailabilityChangedSignal.Dispatch(canShowAd);
		}

		private void RewardedVideoAdRewardedEvent(SupersonicPlacement ssp)
		{
			adRewarded = true;
		}

		private void RewardedVideoInitSuccessEvent()
		{
			logger.Debug("{0}RewardedVideoInitSuccessEvent()", "KampaiSupersonic: ");
		}

		private void RewardedVideoInitFailEvent(SupersonicError error)
		{
			logger.Error("{0}RewardedVideoInitFailEvent() code: {1}, description: {2}", "KampaiSupersonic: ", error.getCode(), error.getDescription());
		}

		private void RewardedVideoAdOpenedEvent()
		{
			logger.Debug("{0}RewardedVideoAdOpenedEvent()", "KampaiSupersonic: ");
		}

		private void RewardedVideoAdClosedEvent()
		{
			logger.Debug("{0}Ads: RewardedVideoAdClosedEvent(): reset AdVideoInProgress flag", "KampaiSupersonic: ");
			localPersistanceService.PutData("AdVideoInProgress", "False");
			if (adRewarded)
			{
				videoAdRewardedSignal.Dispatch();
				adRewarded = false;
			}
			logger.Debug("{0}RewardedVideoAdClosedEvent()", "KampaiSupersonic: ");
		}

		private void VideoStartEvent()
		{
			logger.Debug("{0}VideoStartEvent()", "KampaiSupersonic: ");
		}

		private void VideoEndEvent()
		{
			logger.Debug("{0}VideoEndEvent()", "KampaiSupersonic: ");
		}
	}
}
