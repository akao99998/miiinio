using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetupSpecialEventCharacterCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SetupSpecialEventCharacterCommand") as IKampaiLogger;

		[Inject]
		public int specialEventItemDefinitionID { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CreateNamedCharacterViewSignal createNamedCharacterViewSignal { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public ShowSpecialEventCharacterSignal showSpecialEventCharacterSignal { get; set; }

		[Inject]
		public UpdateQuestWorldIconsSignal updateWorldIconSignal { get; set; }

		[Inject]
		public GetNewQuestSignal getNewQuestSignal { get; set; }

		public override void Execute()
		{
			if (specialEventItemDefinitionID != -1)
			{
				SpecialEventCharacter specialEventCharacter = null;
				ICollection<SpecialEventCharacter> instancesByType = playerService.GetInstancesByType<SpecialEventCharacter>();
				if (instancesByType != null)
				{
					foreach (SpecialEventCharacter item in instancesByType)
					{
						if (item.SpecialEventID == specialEventItemDefinitionID)
						{
							specialEventCharacter = item;
							break;
						}
					}
				}
				if (specialEventCharacter != null)
				{
					routineRunner.StartCoroutine(SetupAndShowSpecialEventCharacter(specialEventCharacter));
					return;
				}
				logger.Error("SetupSpecialEventCharacterCommand: Failed to find Special Event Character instance for Event Definition ID {0}", specialEventItemDefinitionID.ToString());
				return;
			}
			SpecialEventCharacterDefinition specialEventCharacterDefinition = definitionService.Get<SpecialEventCharacterDefinition>(70009);
			if (specialEventCharacterDefinition == null)
			{
				logger.Error("SetupSpecialEventCharacterCommand: Failed to find Special Event Character Definition");
				return;
			}
			ICollection<SpecialEventCharacter> byDefinitionId = playerService.GetByDefinitionId<SpecialEventCharacter>(70009);
			if (byDefinitionId != null && byDefinitionId.Count > 0)
			{
				routineRunner.StartCoroutine(SetupSpecialEventCharacters(byDefinitionId));
			}
		}

		private IEnumerator SetupSpecialEventCharacters(ICollection<SpecialEventCharacter> specialEventCharacters)
		{
			foreach (SpecialEventCharacter existingCharacter2 in specialEventCharacters)
			{
				SpecialEventItemDefinition eventDefinition2 = definitionService.Get<SpecialEventItemDefinition>(existingCharacter2.SpecialEventID);
				if (eventDefinition2 != null && questService.IsQuestCompleted(eventDefinition2.UnlockQuestID))
				{
					CreateSpecialEventCharacter(existingCharacter2);
				}
			}
			yield return null;
			getNewQuestSignal.Dispatch();
			List<Quest> quests = playerService.GetInstancesByType<Quest>();
			foreach (SpecialEventCharacter existingCharacter in specialEventCharacters)
			{
				SpecialEventItemDefinition eventDefinition = definitionService.Get<SpecialEventItemDefinition>(existingCharacter.SpecialEventID);
				foreach (Quest quest in quests)
				{
					if (quest.Definition.SurfaceID == eventDefinition.PrestigeDefinitionID)
					{
						updateWorldIconSignal.Dispatch(quest);
					}
				}
			}
			showSpecialEventCharacterSignal.Dispatch(3f, -1);
		}

		private IEnumerator SetupAndShowSpecialEventCharacter(SpecialEventCharacter existingCharacter)
		{
			CreateSpecialEventCharacter(existingCharacter);
			yield return null;
			showSpecialEventCharacterSignal.Dispatch(3f, existingCharacter.ID);
		}

		private void CreateSpecialEventCharacter(SpecialEventCharacter existingCharacter)
		{
			createNamedCharacterViewSignal.Dispatch(existingCharacter);
			Prestige prestige = prestigeService.GetPrestige(existingCharacter.PrestigeDefinitionID);
			if (prestige == null)
			{
				PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(existingCharacter.PrestigeDefinitionID);
				if (prestigeDefinition != null)
				{
					prestige = new Prestige(prestigeDefinition);
					prestige.trackedInstanceId = existingCharacter.ID;
					prestige.state = PrestigeState.Questing;
					prestigeService.AddPrestige(prestige);
					logger.Info("SetupSpecialEventCharacterCommand: Added Prestige ID {0} for Special Event Character ID {1} tracked instance ID to:", existingCharacter.PrestigeDefinitionID, existingCharacter.ID.ToString());
				}
				else
				{
					logger.Warning("SetupSpecialEventCharacterCommand: Added Prestige ID {0} for Special Event Character ID {1} tracked instance ID to:", existingCharacter.PrestigeDefinitionID, existingCharacter.ID.ToString());
				}
			}
		}
	}
}
