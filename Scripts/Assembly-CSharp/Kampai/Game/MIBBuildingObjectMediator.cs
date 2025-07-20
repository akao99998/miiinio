using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class MIBBuildingObjectMediator : Mediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("MIBBuildingObjectMediator") as IKampaiLogger;

		private MIBBuilding building;

		private Signal<int> expiredSignal = new Signal<int>();

		private Signal<int> preloadSignal = new Signal<int>();

		private int timeEventId;

		[Inject]
		public MIBBuildingObjectView view { get; set; }

		[Inject]
		public DisplayMIBBuildingSignal displayMIBBuildingSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IHindsightService hindsightService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public HindsightContentPreloadSignal hindsightContentPreloadSignal { get; set; }

		[Inject]
		public HindsightContentDismissSignal hindsightContentDismissSignal { get; set; }

		[Inject]
		public GrantMIBRewardsSignal grantMIBRewardsSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public AppResumeCompletedSignal appResumeCompletedSignal { get; set; }

		[Inject]
		public AppFocusGainedCompletedSignal appFocusGainedCompletedSignal { get; set; }

		[Inject]
		public SpawnDooberSignal spawnDooberSignal { get; set; }

		[Inject]
		public CreateResourceIconSignal createResourceIconSignal { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public IMIBService mibService { get; set; }

		public override void OnRegister()
		{
			DisplayBuilding(false);
			if (playerService.GetHighestFtueCompleted() >= 999999)
			{
				displayMIBBuildingSignal.AddListener(DisplayBuilding);
				hindsightContentPreloadSignal.AddListener(SetReadyState);
				hindsightContentDismissSignal.AddListener(OnContentDismissed);
				grantMIBRewardsSignal.AddListener(OnGrantRewards);
				expiredSignal.AddListener(OnExpiredSignalFired);
				preloadSignal.AddListener(OnPreloadSignalFired);
				appResumeCompletedSignal.AddListener(OnAppResume);
				appFocusGainedCompletedSignal.AddListener(OnAppResume);
				building = playerService.GetFirstInstanceByDefinitionId<MIBBuilding>(3129);
				timeEventId = building.ID;
				UpdateState(building.MIBState);
				CheckForRewards();
			}
		}

		public override void OnRemove()
		{
			displayMIBBuildingSignal.RemoveListener(DisplayBuilding);
			hindsightContentPreloadSignal.RemoveListener(SetReadyState);
			hindsightContentDismissSignal.RemoveListener(OnContentDismissed);
			grantMIBRewardsSignal.RemoveListener(OnGrantRewards);
			expiredSignal.RemoveListener(OnExpiredSignalFired);
			preloadSignal.RemoveListener(OnPreloadSignalFired);
			appResumeCompletedSignal.RemoveListener(OnAppResume);
			appFocusGainedCompletedSignal.RemoveListener(OnAppResume);
			if (timeEventId != 0)
			{
				timeEventService.RemoveEvent(timeEventId);
			}
		}

		private void UpdateState(MIBBuildingState state, bool registerTimeEvents = true)
		{
			switch (state)
			{
			case MIBBuildingState.READY:
				PreloadThenSetReadyState();
				break;
			case MIBBuildingState.PRELOADING:
				SetPreloadingState();
				break;
			case MIBBuildingState.EXPIRED:
				SetExpiredState();
				break;
			}
			if (registerTimeEvents)
			{
				SetupTimeEvents();
			}
		}

		private void SetupTimeEvents()
		{
			timeEventService.AddEvent(timeEventId, timeService.CurrentTime(), building.UTCExpiryTime - timeService.CurrentTime(), expiredSignal);
			timeEventService.AddEvent(timeEventId, timeService.CurrentTime(), building.UTCCooldownTime - timeService.CurrentTime(), preloadSignal);
		}

		private void OnExpiredSignalFired(int id)
		{
			UpdateState(MIBBuildingState.EXPIRED, false);
		}

		private void OnPreloadSignalFired(int id)
		{
			UpdateState(MIBBuildingState.PRELOADING);
		}

		private void SetExpiredState()
		{
			building.MIBState = MIBBuildingState.EXPIRED;
			logger.Debug("MIB: Ad expired (or interacted with), setting building state to {0}", building.MIBState);
			DisplayBuilding(false);
		}

		private void SetPreloadingState()
		{
			building.MIBState = MIBBuildingState.PRELOADING;
			logger.Debug("MIB: Ad is ready to preload, setting building state to {0}", building.MIBState);
			DisplayBuilding(false);
			building.UTCExpiryTime = timeService.CurrentTime() + building.Definition.ExpiryInSeconds;
			building.UTCCooldownTime = timeService.CurrentTime() + building.Definition.CooldownInSeconds;
			PreloadThenSetReadyState();
		}

		private void PreloadThenSetReadyState()
		{
			HindsightCampaign cachedContent = hindsightService.GetCachedContent(HindsightCampaign.Scope.message_in_a_bottle);
			if (cachedContent != null)
			{
				SetReadyState(HindsightCampaign.Scope.message_in_a_bottle);
			}
		}

		private void SetReadyState(HindsightCampaign.Scope scope)
		{
			if (scope == HindsightCampaign.Scope.message_in_a_bottle)
			{
				building.MIBState = MIBBuildingState.READY;
				logger.Debug("MIB: Ad is ready to show, setting building state to {0}", building.MIBState);
				DisplayBuilding(true);
			}
		}

		private void OnContentDismissed(HindsightCampaign.Scope scope, HindsightCampaign.DismissType dismissType)
		{
			if (scope == HindsightCampaign.Scope.message_in_a_bottle)
			{
				if (dismissType == HindsightCampaign.DismissType.ACCEPTED)
				{
					mibService.SetReturningKey();
				}
				else
				{
					mibService.ClearReturningKey();
				}
				logger.Debug("MIB: Ad has been dismissed, dismiss type: {0}", dismissType);
				MIBBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MIBBuilding>(3129);
				int num = firstInstanceByDefinitionId.Definition.FirstXTapsWeightedDefinitionId;
				if (firstInstanceByDefinitionId.NumOfRewardsCollectedOnTap > firstInstanceByDefinitionId.Definition.AfterXTapRewards)
				{
					num = firstInstanceByDefinitionId.Definition.SecondXTapsWeightedDefinitionId;
				}
				TransactionDefinition transactionDefinition = mibService.PickWeightedTransaction(num);
				if (transactionDefinition == null)
				{
					logger.Error("MIB: Failed to find weighted definition id: {0}, will not grant user rewards", num);
				}
				else
				{
					OnGrantRewards(MIBRewardType.ON_TAP, transactionDefinition, new Vector3(firstInstanceByDefinitionId.Location.x, 0f, firstInstanceByDefinitionId.Location.y));
					telemetryService.Send_Telemetry_EVT_DCN(GetButtonPressedString(dismissType), scope.ToString(), scope.ToString());
				}
			}
		}

		private string GetButtonPressedString(HindsightCampaign.DismissType dismissType)
		{
			if (dismissType == HindsightCampaign.DismissType.ACCEPTED)
			{
				return "Yes";
			}
			return "No";
		}

		private void OnAppResume()
		{
			logger.Debug("MIB: Ad has resumed from the background, let's check if we should grant the user rewards");
			CheckForRewards();
		}

		private void CheckForRewards()
		{
			if (IsUserReturning())
			{
				DisplayResourceIcon();
			}
		}

		private bool IsUserReturning()
		{
			return building != null && !building.Definition.DisableReturnRewards && mibService.IsUserReturning();
		}

		private void DisplayResourceIcon()
		{
			logger.Debug("MIB: Showing resource icon");
			createResourceIconSignal.Dispatch(new ResourceIconSettings(building.ID, -1, 1));
		}

		private void DisplayBuilding(bool display)
		{
			if (!display && IsUserReturning())
			{
				view.EnableRenderers(true);
			}
			else
			{
				view.EnableRenderers(display);
			}
			view.UpdateColliders(display);
		}

		private void OnGrantRewards(MIBRewardType rewardType, TransactionDefinition pickedTransactionDef, Vector3 tweenLocation)
		{
			switch (rewardType)
			{
			case MIBRewardType.ON_TAP:
				UpdateState(MIBBuildingState.EXPIRED, false);
				break;
			case MIBRewardType.ON_RETURN:
				mibService.ClearReturningKey();
				UpdateState(building.MIBState, false);
				break;
			}
			GrantRewards(rewardType, pickedTransactionDef, tweenLocation);
		}

		private bool IsRewardTypeDisabled(MIBRewardType rewardType)
		{
			switch (rewardType)
			{
			case MIBRewardType.ON_TAP:
				return building.Definition.DisableTapRewards;
			case MIBRewardType.ON_RETURN:
				return building.Definition.DisableReturnRewards;
			default:
				return false;
			}
		}

		private void IncrementRewardCollected(MIBRewardType rewardType)
		{
			switch (rewardType)
			{
			case MIBRewardType.ON_TAP:
				building.NumOfRewardsCollectedOnTap++;
				break;
			case MIBRewardType.ON_RETURN:
				building.NumOfRewardsCollectedOnReturn++;
				break;
			}
		}

		private void GrantRewards(MIBRewardType rewardType, TransactionDefinition pickedTransactionDef, Vector3 tweenLocation)
		{
			if (IsRewardTypeDisabled(rewardType))
			{
				return;
			}
			playerService.RunEntireTransaction(pickedTransactionDef, TransactionTarget.NO_VISUAL, null, new TransactionArg(rewardType.ToString()));
			IncrementRewardCollected(rewardType);
			if (rewardType == MIBRewardType.ON_RETURN)
			{
				spawnDooberSignal.Dispatch(uiCamera.WorldToScreenPoint(tweenLocation), DooberUtil.GetDestinationType(pickedTransactionDef.Outputs[0].ID, definitionService), -1, false);
				return;
			}
			foreach (QuantityItem output in pickedTransactionDef.Outputs)
			{
				spawnDooberSignal.Dispatch(tweenLocation, DooberUtil.GetDestinationType(output.ID, definitionService), -1, true);
			}
		}
	}
}
