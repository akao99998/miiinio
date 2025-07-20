using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game
{
	public interface IQuestController
	{
		int ID { get; }

		Quest Quest { get; }

		int StepCount { get; }

		int QuestIconTrackedInstanceID { get; }

		bool AreAllStepsComplete { get; }

		QuestDefinition Definition { get; }

		QuestState State { get; }

		bool AutoGrantReward { get; set; }

		IQuestStepController GetStepController(int stepIndex);

		int GetIdleTime();

		IList<QuantityItem> GetRequiredQuantityItems();

		void SetUpTracking();

		bool IsTrackingThisBuilding(int buildingID, QuestStepType StepType);

		int IsTrackingOneOffCraftable(int itemDefinitionID);

		void DeleteQuest();

		void OnQuestScriptComplete(QuestScriptInstance questScriptInstance);

		void RushQuestStep(int stepIndex);

		void UpdateTask(QuestStepType stepType, QuestTaskTransition questTaskTransition = QuestTaskTransition.Start, Building building = null, int buildingDefId = 0, int itemDefId = 0);

		void GoToQuestState(QuestState targetState);

		void CheckAndUpdateQuestCompleteState();

		void ProcessAutomaticQuest();

		void Debug_SetQuestToInProgressIfNotAlready();
	}
}
