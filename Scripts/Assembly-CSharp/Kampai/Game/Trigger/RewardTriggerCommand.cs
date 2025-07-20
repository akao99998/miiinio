using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.View;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class RewardTriggerCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("RewardTriggerCommand") as IKampaiLogger;

		[Inject]
		public TriggerInstance triggerInstance { get; set; }

		[Inject]
		public TriggerRewardDefinition triggerReward { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public CloseTSMModalSignal closeModalSignal { get; set; }

		[Inject]
		public HideTSMCharacterSignal HideTSMCharacterSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CheckTriggersSignal triggersSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public RewardTriggerSignal rewardTriggerSignal { get; set; }

		public override void Execute()
		{
			if (triggerInstance == null || triggerReward == null)
			{
				logger.Error("Null trigger");
				return;
			}
			int iD = triggerReward.ID;
			IList<int> recievedRewardIds = triggerInstance.RecievedRewardIds;
			if (triggerReward.IsUniquePerInstance && recievedRewardIds.Contains(iD))
			{
				logger.Warning("Duplicate reward {0}", iD);
				return;
			}
			TriggerDefinition definition = triggerInstance.Definition;
			recievedRewardIds.Add(iD);
			triggerReward.RewardPlayer(gameContext);
			telemetryService.Send_Telemetry_EVT_TSM_TRIGGER_ACTION(definition, triggerReward);
			IList<TriggerRewardDefinition> rewards = definition.rewards;
			bool flag = false;
			UpsellTriggerRewardDefinition upsellTriggerRewardDefinition = null;
			for (int i = 0; i < rewards.Count; i++)
			{
				TriggerRewardDefinition triggerRewardDefinition = rewards[i];
				if (triggerRewardDefinition.type == TriggerRewardType.Identifier.Upsell)
				{
					upsellTriggerRewardDefinition = triggerRewardDefinition as UpsellTriggerRewardDefinition;
				}
				else if (!recievedRewardIds.Contains(triggerRewardDefinition.ID))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				logger.Debug("Trigger {0} on cooldown.", triggerInstance.Definition.ID);
				triggerInstance.StartGameTime = playerDurationService.TotalGamePlaySeconds;
				closeModalSignal.Dispatch();
				OnRewardCompleted();
				if (upsellTriggerRewardDefinition != null)
				{
					rewardTriggerSignal.Dispatch(triggerInstance, upsellTriggerRewardDefinition);
				}
			}
		}

		private void OnRewardCompleted()
		{
			TSMCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<TSMCharacter>(70008);
			if (firstInstanceByDefinitionId == null)
			{
				logger.Error("Failed to Cancel Trigger {0} because there isn't a traveling sales minion found", triggerInstance.Definition.ID);
				return;
			}
			if (triggerInstance.Definition.TreasureIntro)
			{
				HideTSMCharacterSignal.Dispatch(TSMCharacterHideState.Chest);
			}
			else
			{
				HideTSMCharacterSignal.Dispatch(TSMCharacterHideState.Celebrate);
			}
			firstInstanceByDefinitionId.Created = false;
			removeWayFinderSignal.Dispatch(301);
			firstInstanceByDefinitionId.PreviousTaskUTCTime = timeService.CurrentTime();
			timeEventService.AddEvent(firstInstanceByDefinitionId.ID, firstInstanceByDefinitionId.PreviousTaskUTCTime, firstInstanceByDefinitionId.Definition.CooldownInSeconds, triggersSignal);
		}
	}
}
