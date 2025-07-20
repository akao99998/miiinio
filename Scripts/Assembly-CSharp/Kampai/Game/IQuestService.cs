using System.Collections.Generic;

namespace Kampai.Game
{
	public interface IQuestService
	{
		void Initialize();

		void RushQuestStep(int questId, int step);

		void UpdateAllQuestsWithQuestStepType(QuestStepType type, QuestTaskTransition questTaskTransition = QuestTaskTransition.Start, Building building = null, int buildingDefId = 0, int item = 0);

		void UpdateMasterPlanQuestLine();

		int IsOneOffCraftableDisplayable(int questDefinitionId, int trackedItemDefinitionID);

		bool IsQuestCompleted(int questDefinitionID);

		void SetQuestLineState(int questLineId, QuestLineState targetState);

		bool IsBridgeQuestComplete(int bridgeDefId);

		int GetLongestIdleQuestDuration();

		IQuestController GetLongestIdleQuestController();

		int GetIdleQuestDuration(int questDefinitionID);

		Dictionary<int, QuestLine> GetQuestLines();

		Dictionary<int, IQuestController> GetQuestMap();

		Dictionary<int, List<int>> GetQuestUnlockTree();

		IQuestController GetQuestControllerByDefinitionID(int questDefinitionId);

		IQuestController GetQuestControllerByInstanceID(int questInstanceId);

		IQuestStepController GetQuestStepController(int questDefinitionID, int questStepIndex);

		IQuestController AddQuest(Quest quest);

		bool ContainsQuest(int questInstanceId);

		IQuestController AddQuest(MasterPlanComponent componentQuest, bool isBuildQuest);

		Quest GetQuestByInstanceId(int id);

		IQuestController AddMasterPlanQuest(MasterPlan masterPlanQuest);

		void RemoveQuest(IQuestController questController);

		void RemoveQuest(int questDefinitionID);

		string GetQuestName(string key, params object[] args);

		string GetEventName(string key, params object[] args);

		void UnlockMinionParty(int QuestDefinitionID);

		bool HasActiveQuest(int surfaceId);

		void SetPulseMoveBuildingAccept(bool enablePulse);

		bool ShouldPulseMoveButtonAccept();
	}
}
