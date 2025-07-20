using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Common
{
	public class ShowSocialPartyRewardCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ShowSocialPartyRewardCommand") as IKampaiLogger;

		[Inject]
		public int eventIndex { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public SpawnDooberSignal spawnDooberSignal { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		[Inject]
		public SocialOrderBoardCompleteSignal orderBoardCompleteSignal { get; set; }

		public override void Execute()
		{
			StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
			if (firstInstanceByDefinitionId != null)
			{
				BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
				BuildingObject buildingObject = component.GetBuildingObject(firstInstanceByDefinitionId.ID);
				StageBuildingObject stageBuildingObject = buildingObject as StageBuildingObject;
				if (stageBuildingObject != null)
				{
					stageBuildingObject.UpdateStageState(BuildingState.SocialComplete);
					logger.Info("Social Event has been Completed! Collecting rewards.");
				}
				TimedSocialEventDefinition socialEvent = timedSocialEventService.GetSocialEvent(eventIndex);
				TransactionDefinition reward = socialEvent.GetReward(definitionService);
				if (reward != null)
				{
					playerService.RunEntireTransaction(reward, TransactionTarget.NO_VISUAL, TransactionCallback);
				}
				else
				{
					logger.Info("Reward is null, nothing to do.");
				}
			}
		}

		public void TransactionCallback(PendingCurrencyTransaction pct)
		{
			if (!pct.Success)
			{
				logger.Error("CollectTransactionCallback PendingCurrencyTransaction was a failure.");
			}
			SocialTeamResponse socialEventStateCached = timedSocialEventService.GetSocialEventStateCached(eventIndex);
			if (socialEventStateCached == null)
			{
				logger.Error("SocialPartyRewardMediator: Failed to get cached event state for social event definition id {0}.", eventIndex);
				return;
			}
			if (socialEventStateCached.Team == null)
			{
				logger.Error("SocialPartyRewardMediator: Team is null.");
				return;
			}
			TimedSocialEventDefinition socialEvent = timedSocialEventService.GetSocialEvent(eventIndex);
			TransactionDefinition reward = socialEvent.GetReward(definitionService);
			StageBuilding firstInstanceByDefintion = playerService.GetFirstInstanceByDefintion<StageBuilding, StageBuildingDefinition>();
			for (int i = 0; i < reward.Outputs.Count; i++)
			{
				DestinationType destinationType = DooberUtil.GetDestinationType(reward.ID, definitionService);
				spawnDooberSignal.Dispatch(uiCamera.WorldToScreenPoint(firstInstanceByDefintion.Location.ToVector3()), destinationType, reward.Outputs[i].ID, false);
			}
			Signal<SocialTeamResponse, ErrorResponse> signal = new Signal<SocialTeamResponse, ErrorResponse>();
			signal.AddListener(ClaimRewardResponse);
			timedSocialEventService.ClaimReward(eventIndex, timedSocialEventService.GetSocialEventStateCached(eventIndex).Team.ID, signal);
		}

		public void ClaimRewardResponse(SocialTeamResponse response, ErrorResponse error)
		{
			if (error != null)
			{
				networkConnectionLostSignal.Dispatch();
				return;
			}
			if (response != null && response.Team != null && response.Team.Members != null)
			{
				telemetryService.Send_Telemetry_EVT_SOCIAL_EVENT_COMPLETION(response.Team.Members.Count);
			}
			TimedSocialEventDefinition socialEvent = timedSocialEventService.GetSocialEvent(eventIndex);
			int iD = socialEvent.ID;
			if (timedSocialEventService.GetCurrentSocialEvent() == null || timedSocialEventService.GetCurrentSocialEvent().ID == iD)
			{
				StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
				if (firstInstanceByDefinitionId != null)
				{
					removeWayFinderSignal.Dispatch(firstInstanceByDefinitionId.ID);
				}
			}
			orderBoardCompleteSignal.Dispatch();
		}
	}
}
