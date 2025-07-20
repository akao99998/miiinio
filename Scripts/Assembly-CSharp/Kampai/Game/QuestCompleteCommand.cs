using System.Collections.Generic;
using Kampai.Common;
using Kampai.Common.Service.Telemetry;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class QuestCompleteCommand : Command
	{
		[Inject]
		public UpdateQuestBookBadgeSignal updateBadgeSignal { get; set; }

		[Inject]
		public GetNewQuestSignal getNewQuestSignal { get; set; }

		[Inject]
		public UpdateQuestWorldIconsSignal updateQuestWorldIconsSignal { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersist { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService characterService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displaySignal { get; set; }

		[Inject]
		public ReconcileSalesSignal reconcileSalesSignal { get; set; }

		[Inject]
		public SetupSpecialEventCharacterSignal setupSpecialEventCharacterSignal { get; set; }

		[Inject]
		public ValidateCurrentTriggerSignal validateCurrentTriggerSignal { get; set; }

		[Inject]
		public UpdateUIButtonsSignal updateStoreButtonsSignal { get; set; }

		[Inject]
		public Quest quest { get; set; }

		public override void Execute()
		{
			if (quest.Definition.SurfaceType == QuestSurfaceType.Character || quest.Definition.SurfaceType == QuestSurfaceType.Automatic)
			{
				CheckIfQuestLineIsCompleted();
			}
			updateBadgeSignal.Dispatch();
			updateQuestWorldIconsSignal.Dispatch(quest);
			getNewQuestSignal.Dispatch();
			prestigeService.UpdateEligiblePrestigeList();
			playerService.IncreaseCompletedQuests();
			string questGiver = string.Empty;
			QuestDefinition activeDefinition = quest.GetActiveDefinition();
			if (activeDefinition.SurfaceType == QuestSurfaceType.Character)
			{
				Prestige prestige = characterService.GetPrestige(quest.GetActiveDefinition().SurfaceID, false);
				if (prestige != null)
				{
					questGiver = prestige.Definition.LocalizedKey;
				}
			}
			else if (activeDefinition.SurfaceType == QuestSurfaceType.ProcedurallyGenerated)
			{
				displaySignal.Dispatch(19000013, false, new Signal<bool>());
			}
			displaySignal.Dispatch(activeDefinition.QuestCompletePlayerTrainingCategoryItemId, false, new Signal<bool>());
			PackDefinition packDefinition = definitionService.Get<PackDefinition>(50001);
			if (quest.Definition.ID == packDefinition.UnlockQuestId)
			{
				reconcileSalesSignal.Dispatch(0);
			}
			CheckIfSpecialEventMinionIsUnlocked();
			if (activeDefinition.QuestVersion != -1)
			{
				SendTelemetry(questGiver);
			}
			validateCurrentTriggerSignal.Dispatch();
			CleanPersistance(activeDefinition);
			updateStoreButtonsSignal.Dispatch(false);
		}

		private void CleanPersistance(QuestDefinition questDef)
		{
			if (localPersist.HasKey(questDef.ID.ToString()))
			{
				localPersist.DeleteKey(questDef.ID.ToString());
			}
		}

		private void SendTelemetry(string questGiver)
		{
			QuestDefinition activeDefinition = quest.GetActiveDefinition();
			if (activeDefinition == null)
			{
				return;
			}
			TransactionDefinition reward = activeDefinition.GetReward(definitionService);
			int xPOutputForTransaction = TransactionUtil.GetXPOutputForTransaction(reward);
			string localizedKey = activeDefinition.LocalizedKey;
			telemetryService.Send_Telemetry_EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL(questService.GetEventName(localizedKey), TelemetryAchievementType.Quest, xPOutputForTransaction, questGiver);
			for (int i = 0; i < reward.GetOutputCount(); i++)
			{
				QuantityItem outputItem = reward.GetOutputItem(i);
				BuildingDefinition definition;
				if (outputItem != null && definitionService.TryGet<BuildingDefinition>(outputItem.ID, out definition))
				{
					telemetryService.Send_Telemetry_EVT_USER_ACQUIRES_BUILDING(definition.TaxonomyType, definition, activeDefinition.ID);
				}
			}
		}

		private void CheckIfSpecialEventMinionIsUnlocked()
		{
			foreach (SpecialEventItemDefinition item in definitionService.GetAll<SpecialEventItemDefinition>())
			{
				if (quest.Definition.ID == item.UnlockQuestID && item.IsActive)
				{
					setupSpecialEventCharacterSignal.Dispatch(item.ID);
					break;
				}
			}
		}

		private void CheckIfQuestLineIsCompleted()
		{
			QuestDefinition activeDefinition = quest.GetActiveDefinition();
			bool flag = false;
			Dictionary<int, QuestLine> questLines = questService.GetQuestLines();
			QuestLine questLine = questLines[activeDefinition.QuestLineID];
			IList<QuestDefinition> quests = questLine.Quests;
			if (quests[0].ID == activeDefinition.ID)
			{
				flag = true;
				questService.SetQuestLineState(activeDefinition.QuestLineID, QuestLineState.Finished);
			}
			Prestige prestige = prestigeService.GetPrestige(activeDefinition.SurfaceID);
			if (prestige == null)
			{
				prestige = prestigeService.GetPrestige(questLine.GivenByCharacterID);
				if (prestige == null)
				{
					return;
				}
			}
			if (prestige.Definition.PrestigeLevelSettings != null && flag && (prestige.state == PrestigeState.Questing || prestige.state == PrestigeState.TaskableWhileQuesting))
			{
				prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Taskable);
			}
		}
	}
}
