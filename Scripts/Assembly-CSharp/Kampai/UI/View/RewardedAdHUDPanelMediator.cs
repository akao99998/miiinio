using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class RewardedAdHUDPanelMediator : EventMediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("RewardedAdHUDPanelMediator") as IKampaiLogger;

		private RewardedVideoHUDView rewardedVideoHUDView;

		private AdPlacementName placementName = AdPlacementName.HUD;

		[Inject]
		public RewardedAdHUDPanelView view { get; set; }

		[Inject]
		public IRewardedAdService rewardedAdService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RewardedAdRewardSignal rewardedAdRewardSignal { get; set; }

		[Inject]
		public OpenRewardedAdDailyRewardPickerModalSignal openRewardedAdDailyRewardPickerModalSignal { get; set; }

		[Inject]
		public AdPlacementActivityStateChangedSignal adPlacementActivityStateChangedSignal { get; set; }

		[Inject]
		public UpdateAdHUDSignal updateAdHUDSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			view.Init();
			rewardedAdRewardSignal.AddListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.AddListener(OnAdPlacementActivityStateChanged);
			updateAdHUDSignal.AddListener(UpdateHud);
			UpdateHud();
		}

		public override void OnRemove()
		{
			rewardedAdRewardSignal.RemoveListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.RemoveListener(OnAdPlacementActivityStateChanged);
			updateAdHUDSignal.RemoveListener(UpdateHud);
			SubscribeOnHudButtonHidingAnimationSignal(false);
		}

		private void SubscribeOnHudButtonHidingAnimationSignal(bool subscribe)
		{
			if (subscribe)
			{
				if (rewardedVideoHUDView != null)
				{
					rewardedVideoHUDView.SlideOutAnimationCompleteSignal.AddListener(OnHUDButtonSlideOutAnimationComplete);
				}
			}
			else if (rewardedVideoHUDView != null)
			{
				rewardedVideoHUDView.SlideOutAnimationCompleteSignal.RemoveListener(OnHUDButtonSlideOutAnimationComplete);
			}
		}

		private void OnAdPlacementActivityStateChanged(AdPlacementInstance placement, bool enabled)
		{
			UpdateHud();
		}

		private void UpdateHud()
		{
			bool flag = rewardedAdService.IsPlacementActive(placementName);
			if (!flag)
			{
				logger.Debug("Ads: placement '{0}' is disabled.", placementName);
			}
			bool flag2 = false;
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			if (minionPartyInstance != null)
			{
				flag2 = minionPartyInstance.IsPartyReady || minionPartyInstance.IsPartyHappening;
			}
			EnableHudButton(flag && !flag2, rewardedAdService.GetPlacementInstance(placementName));
		}

		private void EnableHudButton(bool enable, AdPlacementInstance instance)
		{
			if (enable && instance != null)
			{
				if (rewardedVideoHUDView != null)
				{
					rewardedVideoHUDView.gameObject.SetActive(true);
					rewardedVideoHUDView.PlayPanelAnimation(true);
				}
				else
				{
					rewardedVideoHUDView = CreateRewardedVideoHudButton();
				}
				rewardedVideoHUDView.InitPlacement(instance);
			}
			else if (rewardedVideoHUDView != null)
			{
				SubscribeOnHudButtonHidingAnimationSignal(true);
				rewardedVideoHUDView.PlayPanelAnimation(false);
			}
		}

		private void OnHUDButtonSlideOutAnimationComplete()
		{
			SubscribeOnHudButtonHidingAnimationSignal(false);
			rewardedVideoHUDView.gameObject.SetActive(false);
		}

		private void OnRewardedAdReward(AdPlacementInstance placement)
		{
			if (placement.Definition.Name == placementName)
			{
				OnTheGlassDailyRewardDefinition onTheGlassDailyRewardDefinition = placement.Definition as OnTheGlassDailyRewardDefinition;
				if (onTheGlassDailyRewardDefinition != null)
				{
					openRewardedAdDailyRewardPickerModalSignal.Dispatch(placement, onTheGlassDailyRewardDefinition);
				}
			}
		}

		private RewardedVideoHUDView CreateRewardedVideoHudButton()
		{
			RewardedVideoHUDView rewardedVideoHUDView = BuildRewardedVideoHudControl();
			rewardedVideoHUDView.transform.SetParent(view.transform, false);
			rewardedVideoHUDView.transform.localScale = new Vector3(1f, 1f, 1f);
			return rewardedVideoHUDView;
		}

		private RewardedVideoHUDView BuildRewardedVideoHudControl()
		{
			string path = "HUD_RewardedVideo";
			GameObject gameObject = KampaiResources.Load(path) as GameObject;
			if (gameObject == null)
			{
				logger.Error("Unable to load Rewarded Video HUD prefab.");
				return null;
			}
			GameObject gameObject2 = Object.Instantiate(gameObject);
			return gameObject2.GetComponent<RewardedVideoHUDView>();
		}
	}
}
