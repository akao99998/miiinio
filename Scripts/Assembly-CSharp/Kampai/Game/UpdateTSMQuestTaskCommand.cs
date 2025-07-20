using System.Text;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.Telemetry;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class UpdateTSMQuestTaskCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UpdateTSMQuestTaskCommand") as IKampaiLogger;

		[Inject]
		public Quest Quest { get; set; }

		[Inject]
		public bool Completed { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public HideTSMCharacterSignal HideTSMCharacterSignal { get; set; }

		[Inject]
		public RemoveQuestWorldIconSignal RemoveQuestWorldIconSignal { get; set; }

		[Inject]
		public CheckTriggersSignal TriggersSignal { get; set; }

		[Inject]
		public IQuestService QuestService { get; set; }

		[Inject]
		public ITimeEventService TimeEventService { get; set; }

		[Inject]
		public ITimeService TimeService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITelemetryService TelemetryService { get; set; }

		public override void Execute()
		{
			if (Quest == null)
			{
				logger.Error("Quest does not exist on traveling sales minion.");
				return;
			}
			TSMCharacter firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<TSMCharacter>(70008);
			if (firstInstanceByDefinitionId == null)
			{
				logger.Error("Failed to Cancel QuestId {0} because there isn't a traveling sales minion found", Quest.ID);
				return;
			}
			HideTSM(firstInstanceByDefinitionId);
			if (Quest.dynamicDefinition == null)
			{
				logger.Error("Quest dynamic definition does not exist on traveling sales minion.");
				return;
			}
			int xPOutputForTransaction = TransactionUtil.GetXPOutputForTransaction(Quest.GetActiveDefinition().GetReward(definitionService));
			string achievementName = new StringBuilder().Append("ProceduralQuest").Append(Quest.dynamicDefinition.ID).ToString();
			TelemetryService.Send_Telemetry_EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL_ProceduralQuest(achievementName, (!Completed) ? ProceduralQuestEndState.Dismissed : ProceduralQuestEndState.Completed, xPOutputForTransaction);
		}

		private void HideTSM(TSMCharacter tsmCharacter)
		{
			HideTSMCharacterSignal.Dispatch(Completed ? TSMCharacterHideState.Celebrate : TSMCharacterHideState.Hide);
			tsmCharacter.Created = false;
			RemoveQuestWorldIconSignal.Dispatch(Quest);
			tsmCharacter.PreviousTaskUTCTime = TimeService.CurrentTime();
			QuestService.RemoveQuest(Quest.GetActiveDefinition().ID);
			TimeEventService.AddEvent(tsmCharacter.ID, tsmCharacter.PreviousTaskUTCTime, tsmCharacter.Definition.CooldownInSeconds, TriggersSignal);
		}
	}
}
