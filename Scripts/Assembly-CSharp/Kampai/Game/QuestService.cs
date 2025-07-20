using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class QuestService : IQuestService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("QuestService") as IKampaiLogger;

		private Dictionary<int, QuestLine> questLines = new Dictionary<int, QuestLine>();

		private Dictionary<int, List<int>> questUnlockTree = new Dictionary<int, List<int>>();

		private Dictionary<int, IQuestController> quests = new Dictionary<int, IQuestController>();

		private bool isInitialized;

		private bool isMinionPartyUnlocked;

		private bool pulseMoveBuildingAccept;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public QuestTimeoutSignal timeoutSignal { get; set; }

		[Inject]
		public UpdateQuestWorldIconsSignal updateWorldIconSignal { get; set; }

		[Inject]
		public TimedQuestNotificationSignal questNoteSignal { get; set; }

		[Inject]
		public StartMinionPartyUnlockSequenceSignal startPartyUnlockSignal { get; set; }

		[Inject]
		public IQuestScriptService questScriptService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject(LocalizationServices.EVENT)]
		public ILocalizationService eventsLocalService { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public IMasterPlanQuestService masterPlanQuestService { get; set; }

		public void Initialize()
		{
			LoadQuestLines();
			CreateQuestMap();
			CreateQuestUnlockTree();
			UpdateQuestLineStateBasedOnDependency();
			ICollection<Quest> instancesByType = playerService.GetInstancesByType<Quest>();
			foreach (Quest item in instancesByType)
			{
				CheckQuestVersion(item);
				QuestState state = item.state;
				if (state != QuestState.Complete)
				{
					updateWorldIconSignal.Dispatch(item);
				}
				CheckAndStartQuestTimers(item);
				switch (state)
				{
				case QuestState.RunningStartScript:
					questScriptService.StartQuestScript(item, true);
					break;
				case QuestState.RunningCompleteScript:
					questScriptService.StartQuestScript(item, false);
					break;
				case QuestState.RunningTasks:
				{
					for (int i = 0; i < item.Steps.Count; i++)
					{
						QuestStepState state2 = item.Steps[i].state;
						if (state2 == QuestStepState.RunningStartScript || state2 == QuestStepState.RunningCompleteScript)
						{
							questScriptService.StartQuestScript(item, state2 == QuestStepState.RunningStartScript, false, i, true);
						}
					}
					break;
				}
				}
			}
			UpdateAllQuestsWithQuestStepType(QuestStepType.Construction);
			UpdateAllQuestsWithQuestStepType(QuestStepType.Harvest);
			UpdateMasterPlanQuestLine();
			isMinionPartyUnlocked = playerService.IsMinionPartyUnlocked();
			isInitialized = true;
		}

		public Dictionary<int, QuestLine> GetQuestLines()
		{
			return questLines;
		}

		public Dictionary<int, IQuestController> GetQuestMap()
		{
			return quests;
		}

		public Dictionary<int, List<int>> GetQuestUnlockTree()
		{
			return questUnlockTree;
		}

		public IQuestController GetQuestControllerByDefinitionID(int questDefinitionId)
		{
			if (!quests.ContainsKey(questDefinitionId))
			{
				logger.Info("QuestService: Quest {0} doesn't exist in the quest map.", questDefinitionId);
				return null;
			}
			return quests[questDefinitionId];
		}

		public Quest GetQuestByInstanceId(int id)
		{
			IQuestController questControllerByInstanceID = GetQuestControllerByInstanceID(id);
			return (questControllerByInstanceID != null) ? questControllerByInstanceID.Quest : null;
		}

		public IQuestController GetQuestControllerByInstanceID(int questInstanceId)
		{
			Quest byInstanceId = playerService.GetByInstanceId<Quest>(questInstanceId);
			if (byInstanceId == null)
			{
				if (quests.ContainsKey(questInstanceId))
				{
					return quests[questInstanceId];
				}
				logger.Error("quest doesn't exist for quest instance {0}", questInstanceId);
				return null;
			}
			int iD = byInstanceId.GetActiveDefinition().ID;
			return GetQuestControllerByDefinitionID(iD);
		}

		public bool ContainsQuest(int questInstanceId)
		{
			Quest byInstanceId = playerService.GetByInstanceId<Quest>(questInstanceId);
			if (byInstanceId == null)
			{
				return quests.ContainsKey(questInstanceId);
			}
			int iD = byInstanceId.GetActiveDefinition().ID;
			return quests.ContainsKey(iD);
		}

		public IQuestStepController GetQuestStepController(int questDefinitionID, int questStepIndex)
		{
			if (!quests.ContainsKey(questDefinitionID))
			{
				logger.Info("QuestService: Quest {0} doesn't exist in the quest map.", questDefinitionID);
				return null;
			}
			return quests[questDefinitionID].GetStepController(questStepIndex);
		}

		public bool HasActiveQuest(int surfaceId)
		{
			foreach (IQuestController value in quests.Values)
			{
				if (value.Definition.SurfaceID == surfaceId && value.State != QuestState.Complete)
				{
					return true;
				}
			}
			return false;
		}

		public IQuestController AddQuest(MasterPlanComponent componentQuest, bool isBuildQuest)
		{
			if (isBuildQuest)
			{
				int key = 711;
				if (quests.ContainsKey(key))
				{
					quests.Remove(key);
				}
				IQuestController questController = new MasterPlanQuestController(componentQuest, logger, playerService, definitionService, gameContext, masterPlanQuestService, true);
				quests.Add(key, questController);
				return questController;
			}
			int iD = componentQuest.ID;
			if (quests.ContainsKey(iD))
			{
				quests.Remove(iD);
			}
			IQuestController questController2 = new MasterPlanQuestController(componentQuest, logger, playerService, definitionService, gameContext, masterPlanQuestService, false);
			quests.Add(iD, questController2);
			return questController2;
		}

		public IQuestController AddMasterPlanQuest(MasterPlan masterPlanQuest)
		{
			if (masterPlanQuest == null)
			{
				return null;
			}
			int iD = masterPlanQuest.ID;
			if (quests.ContainsKey(iD))
			{
				quests.Remove(iD);
			}
			IQuestController questController = new MasterPlanQuestController(masterPlanQuest, logger, playerService, definitionService, gameContext, masterPlanQuestService);
			quests.Add(iD, questController);
			return questController;
		}

		public IQuestController AddQuest(Quest quest)
		{
			int iD = quest.GetActiveDefinition().ID;
			if (quests.ContainsKey(iD))
			{
				logger.Error("QuestService: Quest {0} already added.", iD);
				return quests[iD];
			}
			IQuestController questController = new QuestController(quest, logger, playerService, prestigeService, definitionService, questScriptService, gameContext, environment);
			quests.Add(iD, questController);
			playerService.Add(quest);
			SetQuestLineState(quest.GetActiveDefinition().QuestLineID, QuestLineState.Started);
			CheckAndStartQuestTimers(quest);
			return questController;
		}

		public int GetLongestIdleQuestDuration()
		{
			IQuestController longestIdleQuestController = GetLongestIdleQuestController();
			if (longestIdleQuestController == null)
			{
				return 0;
			}
			return longestIdleQuestController.GetIdleTime();
		}

		public IQuestController GetLongestIdleQuestController()
		{
			int num = 0;
			IQuestController result = null;
			foreach (KeyValuePair<int, IQuestController> quest in quests)
			{
				IQuestController value = quest.Value;
				if (value.State == QuestState.RunningTasks)
				{
					int idleTime = value.GetIdleTime();
					if (idleTime > num)
					{
						result = value;
						num = idleTime;
					}
				}
			}
			return result;
		}

		public int GetIdleQuestDuration(int questDefinitionID)
		{
			if (quests.ContainsKey(questDefinitionID))
			{
				return quests[questDefinitionID].GetIdleTime();
			}
			return 0;
		}

		public void RemoveQuest(IQuestController questController)
		{
			QuestDefinition definition = questController.Definition;
			int iD = definition.ID;
			if (!quests.ContainsKey(iD))
			{
				logger.Info("QuestService: Quest {0} has already been removed.", iD);
				return;
			}
			if (definition.SurfaceType == QuestSurfaceType.LimitedEvent || definition.SurfaceType == QuestSurfaceType.TimedEvent)
			{
				timeEventService.RemoveEvent(questController.ID);
			}
			questController.DeleteQuest();
			quests.Remove(iD);
			questController = null;
		}

		public void RemoveQuest(int questDefinitionID)
		{
			if (!quests.ContainsKey(questDefinitionID))
			{
				logger.Info("QuestService: Quest {0} has already been removed.", questDefinitionID);
			}
			else
			{
				RemoveQuest(quests[questDefinitionID]);
			}
		}

		public void SetQuestLineState(int questLineId, QuestLineState targetState)
		{
			if (questLines.ContainsKey(questLineId))
			{
				QuestLine questLine = questLines[questLineId];
				if (questLine.state == QuestLineState.NotStarted)
				{
					questLine.state = targetState;
				}
				else if (questLine.state == QuestLineState.Started && targetState == QuestLineState.Finished)
				{
					questLine.state = targetState;
				}
			}
		}

		public int IsOneOffCraftableDisplayable(int questDefinitionId, int trackedItemDefinitionID)
		{
			if (questDefinitionId == 0)
			{
				return 0;
			}
			if (!quests.ContainsKey(questDefinitionId))
			{
				return 0;
			}
			IQuestController questController = quests[questDefinitionId];
			if (questController.Definition.QuestVersion == -1)
			{
				return 0;
			}
			return questController.IsTrackingOneOffCraftable(trackedItemDefinitionID);
		}

		public bool IsQuestCompleted(int questDefinitionID)
		{
			if (!isInitialized)
			{
				return false;
			}
			if (questDefinitionID == 0)
			{
				return true;
			}
			if (quests.ContainsKey(questDefinitionID))
			{
				return quests[questDefinitionID].State == QuestState.Complete;
			}
			QuestDefinition questDefinition = definitionService.Get<QuestDefinition>(questDefinitionID);
			int unlockQuestId = questDefinition.UnlockQuestId;
			uint quantity = playerService.GetQuantity(StaticItem.LEVEL_ID);
			if (quantity < questDefinition.UnlockLevel || (unlockQuestId != 0 && quests.ContainsKey(questDefinition.UnlockQuestId) && quests[questDefinition.UnlockQuestId].State != QuestState.Complete))
			{
				return false;
			}
			return IsQuestAlreadyFinished(questDefinitionID);
		}

		private void CheckQuestVersion(Quest quest)
		{
			if (quest.GetActiveDefinition().QuestVersion == -1 && !quest.state.Equals(QuestState.Complete) && !quest.state.Equals(QuestState.Notstarted))
			{
				IQuestController questController = quests[quest.GetActiveDefinition().ID];
				questController.GoToQuestState(QuestState.Complete);
			}
			else if (quest.QuestVersion < quest.GetActiveDefinition().QuestVersion)
			{
				ReconstructQuest(quest);
			}
		}

		private void ReconstructQuest(Quest quest)
		{
			QuestState state = quest.state;
			quest.Clear();
			quest.state = state;
			int iD = quest.GetActiveDefinition().ID;
			quests.Remove(iD);
			IQuestController questController = new QuestController(quest, logger, playerService, prestigeService, definitionService, questScriptService, gameContext, environment);
			quests.Add(iD, questController);
			questController.SetUpTracking();
		}

		private bool IsQuestAlreadyFinished(int questDefinitionId)
		{
			QuestDefinition questDefinition = definitionService.Get<QuestDefinition>(questDefinitionId);
			if (questLines.ContainsKey(questDefinition.QuestLineID))
			{
				QuestLine questLine = questLines[questDefinition.QuestLineID];
				if (questLine.state == QuestLineState.NotStarted)
				{
					return false;
				}
				if (questLine.state == QuestLineState.Finished)
				{
					return true;
				}
				if (questLine.state == QuestLineState.Started)
				{
					foreach (QuestDefinition quest in questLine.Quests)
					{
						if (quests.ContainsKey(quest.ID))
						{
							if (quest.NarrativeOrder >= questDefinition.NarrativeOrder)
							{
								return true;
							}
							return false;
						}
					}
					return false;
				}
			}
			if (!questUnlockTree.ContainsKey(questDefinitionId))
			{
				return true;
			}
			foreach (int item in questUnlockTree[questDefinitionId])
			{
				if (quests.ContainsKey(item))
				{
					return true;
				}
			}
			foreach (int item2 in questUnlockTree[questDefinitionId])
			{
				if (IsQuestAlreadyFinished(item2))
				{
					return true;
				}
			}
			return false;
		}

		public void UnlockMinionParty(int QuestDefinitionID)
		{
			if (!isMinionPartyUnlocked)
			{
				MinionPartyDefinition minionPartyDefinition = definitionService.Get<MinionPartyDefinition>(80000);
				if (minionPartyDefinition != null && QuestDefinitionID == minionPartyDefinition.UnlockQuestID)
				{
					startPartyUnlockSignal.Dispatch();
					isMinionPartyUnlocked = true;
				}
			}
		}

		public void RushQuestStep(int questId, int step)
		{
			IQuestController questController = quests[questId];
			questController.RushQuestStep(step);
		}

		public bool IsBridgeQuestComplete(int bridgeDefId)
		{
			foreach (KeyValuePair<int, IQuestController> quest in quests)
			{
				IQuestController value = quest.Value;
				if (value.State != QuestState.Complete)
				{
					continue;
				}
				QuestDefinition definition = value.Definition;
				if (definition.SurfaceType != QuestSurfaceType.Bridge)
				{
					continue;
				}
				IList<QuestStepDefinition> questSteps = definition.QuestSteps;
				for (int i = 0; i < questSteps.Count; i++)
				{
					QuestStepDefinition questStepDefinition = questSteps[i];
					if (questStepDefinition.Type == QuestStepType.BridgeRepair && questStepDefinition.ItemDefinitionID == bridgeDefId)
					{
						return IsQuestCompleted(definition.ID);
					}
				}
			}
			return false;
		}

		public void UpdateAllQuestsWithQuestStepType(QuestStepType type, QuestTaskTransition questTaskTransition = QuestTaskTransition.Start, Building building = null, int buildingDefId = 0, int item = 0)
		{
			foreach (KeyValuePair<int, IQuestController> quest in quests)
			{
				IQuestController value = quest.Value;
				if (value.State == QuestState.RunningTasks)
				{
					value.UpdateTask(type, questTaskTransition, building, buildingDefId, item);
				}
			}
		}

		private void CheckAndStartQuestTimers(Quest quest)
		{
			if (quest.GetActiveDefinition().SurfaceType == QuestSurfaceType.LimitedEvent)
			{
				LimitedQuestDefinition limitedQuestDefinition = quest.GetActiveDefinition() as LimitedQuestDefinition;
				if (limitedQuestDefinition != null)
				{
					if (limitedQuestDefinition.ServerStopTimeUTC <= timeService.CurrentTime())
					{
						RemoveQuest(quest.GetActiveDefinition().ID);
						return;
					}
					int eventTime = limitedQuestDefinition.ServerStopTimeUTC - limitedQuestDefinition.ServerStartTimeUTC;
					timeEventService.AddEvent(quest.ID, limitedQuestDefinition.ServerStartTimeUTC, eventTime, timeoutSignal);
				}
			}
			else
			{
				if (quest.GetActiveDefinition().SurfaceType != QuestSurfaceType.TimedEvent || quest.state == QuestState.Notstarted || quest.state == QuestState.Complete)
				{
					return;
				}
				TimedQuestDefinition timedQuestDefinition = quest.GetActiveDefinition() as TimedQuestDefinition;
				if (timedQuestDefinition != null)
				{
					if (quest.UTCQuestStartTime == 0)
					{
						logger.Log(KampaiLogLevel.Error, "The UTCQuestStartTime is not set for the timed quest!");
						return;
					}
					if (timeService.CurrentTime() > timedQuestDefinition.Duration + quest.UTCQuestStartTime)
					{
						RemoveQuest(quest.GetActiveDefinition().ID);
						return;
					}
					questNoteSignal.Dispatch(quest.ID);
					timeEventService.AddEvent(quest.ID, quest.UTCQuestStartTime, timedQuestDefinition.Duration, timeoutSignal);
				}
			}
		}

		private void CreateQuestMap()
		{
			ICollection<Quest> instancesByType = playerService.GetInstancesByType<Quest>();
			foreach (Quest item in instancesByType)
			{
				QuestDefinition activeDefinition = item.GetActiveDefinition();
				int iD = activeDefinition.ID;
				if (quests.ContainsKey(iD))
				{
					continue;
				}
				logger.Info("Restoring quest def id:{0} from player data", iD);
				IQuestController value = new QuestController(item, logger, playerService, prestigeService, definitionService, questScriptService, gameContext, environment);
				quests.Add(iD, value);
				if (!item.IsDynamic())
				{
					QuestLine questLine = questLines[activeDefinition.QuestLineID];
					if (questLine.state == QuestLineState.NotStarted)
					{
						questLine.state = QuestLineState.Started;
					}
					if (questLine.Quests.Count == activeDefinition.NarrativeOrder + 1 && item.state == QuestState.Complete)
					{
						questLine.state = QuestLineState.Finished;
					}
				}
			}
		}

		private void LoadQuestLines()
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			IList<QuestDefinition> all = definitionService.GetAll<QuestDefinition>();
			foreach (QuestDefinition item in all)
			{
				if (item.SurfaceID < 0 || item.SurfaceType < QuestSurfaceType.Building)
				{
					continue;
				}
				dictionary.Add(item.ID, item.QuestLineID);
				if (questLines.ContainsKey(item.QuestLineID))
				{
					int num = 0;
					bool flag = false;
					IList<QuestDefinition> list = questLines[item.QuestLineID].Quests;
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].NarrativeOrder < item.NarrativeOrder)
						{
							list.Insert(i, item);
							flag = true;
							break;
						}
						if (list[i].NarrativeOrder == item.NarrativeOrder)
						{
							if (item.ID >= list[i].ID)
							{
								list.Insert(i, item);
								flag = true;
								break;
							}
							num = i;
						}
					}
					if (!flag)
					{
						if (num == 0)
						{
							list.Add(item);
						}
						else
						{
							list.Insert(num, item);
						}
					}
				}
				else
				{
					List<QuestDefinition> list2 = new List<QuestDefinition>();
					list2.Add(item);
					QuestLine questLine = new QuestLine();
					questLine.state = QuestLineState.NotStarted;
					questLine.Quests = list2;
					questLines.Add(item.QuestLineID, questLine);
				}
			}
			SetupQuestLineCharacterInfo(dictionary);
		}

		private void CreateQuestLine(MasterPlanComponent component)
		{
			if (questLines.ContainsKey(component.ID))
			{
				questLines.Remove(component.ID);
			}
			QuestLine value = masterPlanQuestService.ConvertComponentToQuestLine(component);
			questLines.Add(component.ID, value);
		}

		private void CreateQuestLine(MasterPlan masterPlan)
		{
			if (questLines.ContainsKey(masterPlan.ID))
			{
				questLines.Remove(masterPlan.ID);
			}
			QuestLine value = masterPlanQuestService.ConvertMasterPlanToQuestLine(masterPlan);
			questLines.Add(masterPlan.ID, value);
		}

		public void UpdateMasterPlanQuestLine()
		{
			IList<MasterPlan> instancesByType = playerService.GetInstancesByType<MasterPlan>();
			IList<MasterPlanComponent> instancesByType2 = playerService.GetInstancesByType<MasterPlanComponent>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				MasterPlan masterPlan = instancesByType[i];
				CreateQuestLine(masterPlan);
				for (int j = 0; j < instancesByType2.Count; j++)
				{
					MasterPlanComponent masterPlanComponent = instancesByType2[j];
					if (masterPlan.ID == masterPlanComponent.planTrackingInstance && masterPlanComponent.State >= MasterPlanComponentState.InProgress)
					{
						CreateQuestLine(masterPlanComponent);
					}
				}
			}
			gameContext.injectionBinder.GetInstance<GetNewQuestSignal>().Dispatch();
		}

		private void SetupQuestLineCharacterInfo(Dictionary<int, int> questToQuestLine)
		{
			IList<PrestigeDefinition> all = definitionService.GetAll<PrestigeDefinition>();
			foreach (PrestigeDefinition item in all)
			{
				int iD = item.ID;
				if (item.Type != 0 || item.PrestigeLevelSettings == null)
				{
					continue;
				}
				for (int i = 0; i < item.PrestigeLevelSettings.Count; i++)
				{
					CharacterPrestigeLevelDefinition characterPrestigeLevelDefinition = item.PrestigeLevelSettings[i];
					Prestige prestige = prestigeService.GetPrestige(iD, false);
					if (characterPrestigeLevelDefinition.UnlockQuestID != 0 && questToQuestLine.ContainsKey(characterPrestigeLevelDefinition.UnlockQuestID))
					{
						int num = questToQuestLine[characterPrestigeLevelDefinition.UnlockQuestID];
						QuestLine questLine = questLines[num];
						questLine.UnlockCharacterPrestigeLevel = i;
						if (prestige != null && prestige.CurrentPrestigeLevel >= i && questLine.state == QuestLineState.NotStarted)
						{
							SetQuestLineState(num, QuestLineState.Started);
						}
					}
					if (characterPrestigeLevelDefinition.AttachedQuestID == 0 || !questToQuestLine.ContainsKey(characterPrestigeLevelDefinition.AttachedQuestID))
					{
						continue;
					}
					int num2 = questToQuestLine[characterPrestigeLevelDefinition.AttachedQuestID];
					QuestLine questLine2 = questLines[num2];
					questLine2.GivenByCharacterID = iD;
					questLine2.GivenByCharacterPrestigeLevel = i;
					if (prestige != null)
					{
						if (prestige.CurrentPrestigeLevel > i || (prestige.CurrentPrestigeLevel == i && prestige.state == PrestigeState.Taskable))
						{
							questLine2.state = QuestLineState.Finished;
						}
						else if (prestige.CurrentPrestigeLevel == i && (prestige.state == PrestigeState.Questing || prestige.state == PrestigeState.TaskableWhileQuesting) && questLine2.state == QuestLineState.NotStarted)
						{
							SetQuestLineState(num2, QuestLineState.Started);
						}
					}
				}
			}
		}

		private void CreateQuestUnlockTree()
		{
			foreach (KeyValuePair<int, QuestLine> questLine in questLines)
			{
				IList<QuestDefinition> list = questLine.Value.Quests;
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					QuestDefinition questDefinition = list[i];
					int iD = questDefinition.ID;
					if (questDefinition.SurfaceID < 0)
					{
						continue;
					}
					if (i < count - 1)
					{
						QuestDefinition questDefinition2 = list[i + 1];
						int iD2 = questDefinition2.ID;
						if (!questUnlockTree.ContainsKey(questDefinition2.ID))
						{
							List<int> list2 = new List<int>();
							list2.Add(iD);
							questUnlockTree.Add(iD2, list2);
						}
						else if (!questUnlockTree[iD2].Contains(iD))
						{
							questUnlockTree[iD2].Add(iD);
						}
					}
					if (questDefinition.UnlockQuestId != 0)
					{
						if (!questUnlockTree.ContainsKey(questDefinition.UnlockQuestId))
						{
							List<int> list3 = new List<int>();
							list3.Add(iD);
							questUnlockTree.Add(questDefinition.UnlockQuestId, list3);
						}
						else if (!questUnlockTree[questDefinition.UnlockQuestId].Contains(iD))
						{
							questUnlockTree[questDefinition.UnlockQuestId].Add(iD);
						}
						SetQuestDependency(questDefinition);
					}
				}
			}
		}

		private void SetQuestDependency(QuestDefinition questDefinition)
		{
			QuestDefinition questDefinition2 = definitionService.Get<QuestDefinition>(questDefinition.UnlockQuestId);
			if (questLines.ContainsKey(questDefinition.QuestLineID) && questLines.ContainsKey(questDefinition2.QuestLineID) && questLines[questDefinition2.QuestLineID].Quests[0].ID == questDefinition2.ID && questDefinition.NarrativeOrder == 0)
			{
				questLines[questDefinition.QuestLineID].unlockByQuestLine = questLines[questDefinition2.QuestLineID].QuestLineID;
			}
		}

		private void UpdateQuestLineStateBasedOnDependency()
		{
			List<QuestLine> list = new List<QuestLine>();
			foreach (KeyValuePair<int, QuestLine> questLine2 in questLines)
			{
				QuestLine value = questLine2.Value;
				if (value.state == QuestLineState.NotStarted || list.Contains(value))
				{
					continue;
				}
				int unlockByQuestLine = value.unlockByQuestLine;
				while (unlockByQuestLine != 0 && unlockByQuestLine != -1 && questLines.ContainsKey(unlockByQuestLine))
				{
					QuestLine questLine = questLines[unlockByQuestLine];
					if (list.Contains(questLine))
					{
						break;
					}
					questLine.state = QuestLineState.Finished;
					if (questLine.GivenByCharacterID != 0)
					{
						Prestige prestige = prestigeService.GetPrestige(questLine.GivenByCharacterID);
						if (prestige.CurrentPrestigeLevel <= questLine.GivenByCharacterPrestigeLevel)
						{
							prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Taskable, prestige.CurrentPrestigeLevel + 1);
						}
						checkForInjectedPrestige(prestige, questLine);
					}
					list.Add(questLine);
					unlockByQuestLine = questLine.unlockByQuestLine;
				}
			}
		}

		public void checkForInjectedPrestige(Prestige p, QuestLine prevQuestLine)
		{
			if (p.CurrentPrestigeLevel >= p.Definition.PrestigeLevelSettings.Count - 1)
			{
				return;
			}
			for (int i = p.CurrentPrestigeLevel; i < p.Definition.PrestigeLevelSettings.Count - 1; i++)
			{
				QuestDefinition questDefinition = definitionService.Get<QuestDefinition>(p.Definition.PrestigeLevelSettings[i].AttachedQuestID);
				if (questDefinition != null && questLines.ContainsKey(questDefinition.QuestLineID))
				{
					QuestLine questLine = questLines[questDefinition.QuestLineID];
					if (questLine.QuestLineID == prevQuestLine.QuestLineID && playerService.GetQuantity(StaticItem.LEVEL_ID) >= p.Definition.PrestigeLevelSettings[i + 1].UnlockLevel)
					{
						logger.Log(KampaiLogLevel.Info, "Character " + p.Definition.LocalizedKey + " has a lower prestige than they should. User has already completed questLine " + questLine.QuestLineID + " increasing current prestige to " + (i + 1));
						p.CurrentPrestigeLevel = i + 1;
						break;
					}
				}
			}
		}

		public string GetQuestName(string key, params object[] args)
		{
			if (localService.Contains(key))
			{
				return localService.GetString(key, args);
			}
			string ext;
			return localService.GetString(ParseLocalizationKey(key, out ext), args);
		}

		public string GetEventName(string key, params object[] args)
		{
			if (key == null)
			{
				return key;
			}
			string text = key;
			string ext;
			key = ParseLocalizationKey(key, out ext);
			if (eventsLocalService.Contains(key))
			{
				text = eventsLocalService.GetString(key, args);
				if (!string.IsNullOrEmpty(ext))
				{
					text = text + " " + ext;
				}
			}
			else
			{
				logger.Warning("QuestService: Failed to translate Event Name: {0} - Translation Service: {1}, {2}", text, eventsLocalService.IsInitialized(), eventsLocalService.GetLanguageKey());
			}
			return text;
		}

		public void SetPulseMoveBuildingAccept(bool enablePulse)
		{
			pulseMoveBuildingAccept = enablePulse;
		}

		public bool ShouldPulseMoveButtonAccept()
		{
			return pulseMoveBuildingAccept;
		}

		private string ParseLocalizationKey(string key, out string ext)
		{
			string[] array = key.Split('_');
			if (array.Length > 1)
			{
				ext = array[1];
				return string.Format("{0}_{1}", array[0], ext);
			}
			if (key.Length > 2)
			{
				ext = key.Substring(key.Length - 2, 2);
				char[] array2 = ext.ToCharArray();
				if (char.IsNumber(array2[0]) || char.IsNumber(array2[1]))
				{
					return key.Remove(key.Length - 2);
				}
			}
			ext = null;
			return key;
		}
	}
}
