using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class GetNewQuestCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("GetNewQuestCommand") as IKampaiLogger;

		private Dictionary<int, QuestLine> questLines;

		private Dictionary<int, IQuestController> questMap;

		private Dictionary<int, List<int>> questUnlockTree;

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IPrestigeService characterService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			logger.Debug("Get New Quest Command");
			questLines = questService.GetQuestLines();
			questMap = questService.GetQuestMap();
			questUnlockTree = questService.GetQuestUnlockTree();
			foreach (QuestLine value in questLines.Values)
			{
				if (value.Quests != null && value.Quests.Count != 0 && value.state != QuestLineState.Finished)
				{
					MasterPlanQuestType.PlanDefinition planDefinition = value as MasterPlanQuestType.PlanDefinition;
					if (planDefinition != null)
					{
						ExamineQuestLine(planDefinition);
					}
					else
					{
						ExamineQuestLine(value);
					}
				}
			}
		}

		private void ExamineQuestLine(MasterPlanQuestType.PlanDefinition planQuestLine)
		{
			if (planQuestLine == null || planQuestLine.state == QuestLineState.NotStarted)
			{
				return;
			}
			int questLineID = planQuestLine.QuestLineID;
			if (planQuestLine.components != null)
			{
				if (!questService.ContainsQuest(questLineID))
				{
					questService.AddMasterPlanQuest(planQuestLine.plan);
				}
				return;
			}
			if (!questService.ContainsQuest(questLineID))
			{
				bool isBuildQuest = planQuestLine.component.State > MasterPlanComponentState.TasksCollected;
				questService.AddQuest(planQuestLine.component, isBuildQuest);
			}
			IQuestController questControllerByInstanceID = questService.GetQuestControllerByInstanceID(questLineID);
			if (questControllerByInstanceID != null)
			{
				if (questControllerByInstanceID.State != QuestState.Complete)
				{
					questControllerByInstanceID.SetUpTracking();
				}
				else
				{
					questService.AddQuest(planQuestLine.component, true);
				}
			}
		}

		private void ExamineQuestLine(QuestLine questLine)
		{
			for (int i = 0; i < questLine.Quests.Count; i++)
			{
				QuestDefinition questDefinition = questLine.Quests[i];
				int iD = questDefinition.ID;
				if (UnlockValidation(questLine, questDefinition, i))
				{
					logger.Info("Unlocked Quest {0}", iD);
					Quest quest = new Quest(questDefinition);
					quest.Initialize();
					if (!QuestValidation(quest))
					{
						continue;
					}
					IQuestController questController = questService.AddQuest(quest);
					logger.Debug("Unlocking New Quests... Quest Def ID: {0} Quest Surface Type: {1}", questController.Definition.ID, questController.Definition.SurfaceType.ToString());
					if (questDefinition.QuestVersion != -1)
					{
						if (questDefinition.SurfaceType == QuestSurfaceType.Automatic || questDefinition.SurfaceType == QuestSurfaceType.Bridge)
						{
							questController.ProcessAutomaticQuest();
						}
						else
						{
							questController.GoToQuestState(QuestState.Notstarted);
						}
					}
					else
					{
						questController.GoToQuestState(QuestState.Complete);
					}
					TryDeleteOldQuest(questLine, questDefinition, i);
					if (quest.state != QuestState.Complete)
					{
						routineRunner.StartCoroutine(WaitAFrame(questController));
					}
					break;
				}
				IQuestController value;
				questMap.TryGetValue(iD, out value);
				if (value != null)
				{
					value.SetUpTracking();
				}
				if (questMap.ContainsKey(iD))
				{
					break;
				}
			}
		}

		private void TryDeleteOldQuest(QuestLine questLine, QuestDefinition qd, int index)
		{
			if (index < questLine.Quests.Count - 1)
			{
				int iD = questLine.Quests[index + 1].ID;
				if (questMap.ContainsKey(iD) && QuestDeleteValidation(iD))
				{
					logger.Debug("Removing Quests... Quest Def ID: {0}", iD);
					questService.RemoveQuest(questMap[iD]);
				}
			}
			int unlockQuestId = qd.UnlockQuestId;
			if (unlockQuestId != 0 && questMap.ContainsKey(unlockQuestId) && QuestDeleteValidation(unlockQuestId))
			{
				QuestDefinition definition = questMap[unlockQuestId].Definition;
				if (questLine.unlockByQuestLine == definition.QuestLineID)
				{
					logger.Debug("Removing Dependency Quests... Quest Def ID: {0}", unlockQuestId);
					questService.RemoveQuest(questMap[unlockQuestId]);
				}
			}
		}

		private bool UnlockValidation(QuestLine questLine, QuestDefinition questDefinition, int indexInQuestLine)
		{
			int iD = questDefinition.ID;
			if (questMap.ContainsKey(iD))
			{
				return false;
			}
			if (playerService.GetQuantity(StaticItem.LEVEL_ID) < questDefinition.UnlockLevel)
			{
				return false;
			}
			if (questDefinition.UnlockQuestId != 0 && !questService.IsQuestCompleted(questDefinition.UnlockQuestId))
			{
				return false;
			}
			if (indexInQuestLine < questLine.Quests.Count - 1)
			{
				int iD2 = questLine.Quests[indexInQuestLine + 1].ID;
				if (questMap.ContainsKey(iD2) && questMap[iD2].State != QuestState.Complete)
				{
					return false;
				}
				if (!questMap.ContainsKey(iD2))
				{
					return false;
				}
			}
			if (questUnlockTree.ContainsKey(iD))
			{
				foreach (int item in questUnlockTree[iD])
				{
					if (questMap.ContainsKey(item) || questService.IsQuestCompleted(item))
					{
						if (indexInQuestLine == questLine.Quests.Count - 1)
						{
							questLine.state = QuestLineState.Finished;
						}
						return false;
					}
				}
			}
			if (questDefinition.SurfaceType == QuestSurfaceType.Character || (questDefinition.SurfaceType == QuestSurfaceType.Automatic && questDefinition.SurfaceID > 0))
			{
				Prestige prestige = characterService.GetPrestige(questDefinition.SurfaceID);
				if (prestige == null || (prestige.state != PrestigeState.Questing && prestige.state != PrestigeState.TaskableWhileQuesting))
				{
					return false;
				}
				if (questLine.GivenByCharacterID != 0)
				{
					Prestige prestige2 = characterService.GetPrestige(questLine.GivenByCharacterID);
					if (prestige2 == null || (prestige2.state != PrestigeState.Questing && prestige2.state != PrestigeState.TaskableWhileQuesting) || prestige2.CurrentPrestigeLevel < questLine.GivenByCharacterPrestigeLevel)
					{
						return false;
					}
					if (prestige2.state == PrestigeState.Questing)
					{
						bool flag = true;
						int iD3 = prestige2.Definition.ID;
						if (prestige2.Definition.Type == PrestigeType.Villain || (prestige2.CurrentPrestigeLevel > 0 && (iD3 == 40003 || iD3 == 40004)))
						{
							flag = false;
						}
						BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
						TikiBarBuildingObjectView tikiBarBuildingObjectView = component.GetBuildingObject(313) as TikiBarBuildingObjectView;
						if (tikiBarBuildingObjectView != null && !tikiBarBuildingObjectView.ContainsCharacter(prestige2.trackedInstanceId) && flag)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private bool QuestDeleteValidation(int targetQuestDeleteDefID)
		{
			if (questUnlockTree.ContainsKey(targetQuestDeleteDefID))
			{
				foreach (int item in questUnlockTree[targetQuestDeleteDefID])
				{
					if (!questMap.ContainsKey(item) && !IsQuestDeleted(targetQuestDeleteDefID))
					{
						return false;
					}
				}
				return true;
			}
			return true;
		}

		private bool IsQuestDeleted(int questDefID)
		{
			if (questUnlockTree.ContainsKey(questDefID))
			{
				foreach (int item in questUnlockTree[questDefID])
				{
					if (!questMap.ContainsKey(item) && !IsQuestDeleted(item))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private bool QuestValidation(Quest quest)
		{
			if (quest.GetActiveDefinition().SurfaceType == QuestSurfaceType.LimitedEvent)
			{
				LimitedQuestDefinition limitedQuestDefinition = quest.GetActiveDefinition() as LimitedQuestDefinition;
				if (limitedQuestDefinition == null)
				{
					return false;
				}
				if (limitedQuestDefinition.ServerStartTimeUTC > timeService.CurrentTime() || limitedQuestDefinition.ServerStopTimeUTC < timeService.CurrentTime())
				{
					return false;
				}
			}
			return true;
		}

		private IEnumerator WaitAFrame(IQuestController questController)
		{
			yield return null;
			questController.SetUpTracking();
		}
	}
}
