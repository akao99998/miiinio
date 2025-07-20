using System.Collections.Generic;
using Kampai.Game;

namespace Kampai.UI.View
{
	public abstract class AbstractQuestWayFinderView : AbstractWayFinderView, IQuestWayFinderView, IWayFinderView, IWorldToGlassView
	{
		private List<int> allQuests;

		private bool ignoreFirstPriorityUpdate = true;

		private int currentActiveQuestIndex = -1;

		public Quest CurrentActiveQuest { get; private set; }

		protected override string WayFinderDefaultIcon
		{
			get
			{
				return wayFinderDefinition.NewQuestIcon;
			}
		}

		protected virtual string WayFinderQuestCompleteIcon
		{
			get
			{
				return wayFinderDefinition.QuestCompleteIcon;
			}
		}

		protected virtual string WayFinderTaskCompleteIcon
		{
			get
			{
				return wayFinderDefinition.TaskCompleteIcon;
			}
		}

		protected override void InitSubView()
		{
			allQuests = new List<int>();
			WayFinderSettings wayFinderSettings = m_Settings as WayFinderSettings;
			AddQuest(wayFinderSettings.QuestDefId);
			ignoreFirstPriorityUpdate = false;
		}

		internal override void Clear()
		{
			if (allQuests != null)
			{
				allQuests.Clear();
			}
		}

		protected override bool OnCanUpdate()
		{
			if (m_Prestige != null)
			{
				int iD = m_Prestige.Definition.ID;
				if (iD != 40003 && m_Prestige.Definition.Type != PrestigeType.SpecialEventMinion && iD != 40004)
				{
					return false;
				}
			}
			return true;
		}

		public void AddQuest(int questDefId)
		{
			int num = allQuests.IndexOf(questDefId);
			if (num != -1)
			{
				Quest firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Quest>(questDefId);
				if (firstInstanceByDefinitionId.state == QuestState.Harvestable)
				{
					SetNextQuest(num);
				}
				UpdateQuestIcon();
			}
			else
			{
				allQuests.Add(questDefId);
				SetNextQuest(allQuests.Count - 1);
			}
		}

		public void RemoveQuest(int questDefId)
		{
			int num = allQuests.IndexOf(questDefId);
			if (num != -1)
			{
				allQuests.Remove(questDefId);
				if (allQuests.Count == 0)
				{
					CurrentActiveQuest = null;
					RemoveWayFinderSignal.Dispatch();
				}
				else
				{
					SetNextQuest(num);
				}
			}
		}

		public void SetNextQuest(int indexToSet = -1)
		{
			if (allQuests == null || allQuests.Count == 0)
			{
				return;
			}
			List<Quest> priorizedQuests = GetPriorizedQuests();
			if (indexToSet == -1)
			{
				for (int i = 0; i < priorizedQuests.Count; i++)
				{
					Quest quest = priorizedQuests[i];
					QuestDefinition activeDefinition = quest.GetActiveDefinition();
					if (quest.state == QuestState.Harvestable || quest.state == QuestState.Notstarted)
					{
						currentActiveQuestIndex = allQuests.IndexOf(activeDefinition.ID);
						break;
					}
					if (targetObject.ID == 78 && quest.Definition.SurfaceID == 40000)
					{
						currentActiveQuestIndex = allQuests.IndexOf(activeDefinition.ID);
					}
				}
			}
			else
			{
				currentActiveQuestIndex = indexToSet;
			}
			currentActiveQuestIndex %= allQuests.Count;
			CurrentActiveQuest = GetQuestByDefId(allQuests[currentActiveQuestIndex]);
			if (CurrentActiveQuest == null)
			{
				return;
			}
			foreach (Quest item in priorizedQuests)
			{
				QuestDefinition activeDefinition2 = item.GetActiveDefinition();
				if (CurrentActiveQuest.GetActiveDefinition().SurfaceID == activeDefinition2.SurfaceID)
				{
					if (CurrentActiveQuest.ID != item.ID && CurrentActiveQuest.GetActiveDefinition().QuestPriority < activeDefinition2.QuestPriority)
					{
						CurrentActiveQuest = item;
						currentActiveQuestIndex = allQuests.IndexOf(activeDefinition2.ID);
					}
					break;
				}
			}
			UpdateQuestIcon();
		}

		public Quest GetQuestByDefId(int questDefId)
		{
			return playerService.GetFirstInstanceByDefinitionId<Quest>(questDefId);
		}

		protected virtual bool CanUpdateQuestIcon()
		{
			return true;
		}

		private void UpdateQuestIcon()
		{
			if (CanUpdateQuestIcon())
			{
				if (IsQuestComplete())
				{
					SetQuestIcon(WayFinderQuestCompleteIcon);
				}
				else if (IsNewQuestAvailable())
				{
					SetQuestIcon(WayFinderDefaultIcon);
				}
				else if (IsTaskReady())
				{
					SetQuestIcon(WayFinderTaskCompleteIcon);
				}
				else if (IsQuestAvailable())
				{
					SetQuestIcon(WayFinderDefaultIcon);
				}
			}
		}

		private void SetQuestIcon(string icon)
		{
			UpdateIcon(icon);
			if (!ignoreFirstPriorityUpdate)
			{
				UpdateWayFinderPrioritySignal.Dispatch();
			}
		}

		public bool IsNewQuestAvailable()
		{
			if (CurrentActiveQuest != null)
			{
				QuestState state = CurrentActiveQuest.state;
				if (state == QuestState.Notstarted || state == QuestState.RunningStartScript)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsQuestAvailable()
		{
			return CurrentActiveQuest != null && CurrentActiveQuest.state == QuestState.RunningTasks && !IsTaskReady();
		}

		public bool IsTaskReady()
		{
			if (CurrentActiveQuest != null && CurrentActiveQuest.state == QuestState.RunningTasks)
			{
				foreach (QuestStep step in CurrentActiveQuest.Steps)
				{
					if (step.state == QuestStepState.Ready)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsQuestComplete()
		{
			if (CurrentActiveQuest != null)
			{
				switch (CurrentActiveQuest.state)
				{
				case QuestState.RunningCompleteScript:
				case QuestState.Harvestable:
				case QuestState.Complete:
					return true;
				}
			}
			return false;
		}

		private List<Quest> GetPriorizedQuests()
		{
			List<Quest> list = new List<Quest>();
			foreach (int allQuest in allQuests)
			{
				Quest questByDefId = GetQuestByDefId(allQuest);
				list.Add(questByDefId);
			}
			return QuestUtils.ResolveQuests(list);
		}
	}
}
