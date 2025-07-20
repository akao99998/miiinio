using System.Collections;
using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetupTSMCharacterCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SetupTSMCharacterCommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService DefinitionService { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public ITimeEventService TimeEventService { get; set; }

		[Inject]
		public CheckTriggersSignal TriggersSignal { get; set; }

		[Inject]
		public CreateNamedCharacterViewSignal CreateNamedCharacterViewSignal { get; set; }

		[Inject]
		public UpdateQuestWorldIconsSignal UpdateQuestWorldIconsSignal { get; set; }

		[Inject]
		public IRoutineRunner RoutineRunner { get; set; }

		public override void Execute()
		{
			if (PlayerService.GetHighestFtueCompleted() < 9)
			{
				logger.Info("Ignoring setup tsm signal since we are still in the ftue!");
				return;
			}
			TSMCharacterDefinition tSMCharacterDefinition = DefinitionService.Get<TSMCharacterDefinition>(70008);
			if (tSMCharacterDefinition == null)
			{
				logger.Error("Failed to find TSM Character Definition");
				return;
			}
			TSMCharacter firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<TSMCharacter>(70008);
			if (firstInstanceByDefinitionId == null)
			{
				logger.Error("Failed to find TSM Character in player inventory");
				return;
			}
			Quest firstInstanceByDefinitionId2 = PlayerService.GetFirstInstanceByDefinitionId<Quest>(77777);
			if (firstInstanceByDefinitionId2 == null)
			{
				TimeEventService.AddEvent(firstInstanceByDefinitionId.ID, firstInstanceByDefinitionId.PreviousTaskUTCTime, firstInstanceByDefinitionId.Definition.CooldownInSeconds, TriggersSignal);
			}
			else
			{
				RoutineRunner.StartCoroutine(ShowTSMCharacter(firstInstanceByDefinitionId));
			}
		}

		private IEnumerator ShowTSMCharacter(TSMCharacter existingCharacter)
		{
			yield return null;
			Quest quest = PlayerService.GetFirstInstanceByDefinitionId<Quest>(77777);
			if (quest != null && !existingCharacter.Created)
			{
				CreateNamedCharacterViewSignal.Dispatch(existingCharacter);
				UpdateQuestWorldIconsSignal.Dispatch(quest);
			}
			else
			{
				logger.Warning("Quest id: {0} was not found in player inventory or TSM character is already created!");
			}
		}
	}
}
