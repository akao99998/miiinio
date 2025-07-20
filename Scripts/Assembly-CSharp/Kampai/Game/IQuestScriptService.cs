namespace Kampai.Game
{
	public interface IQuestScriptService
	{
		bool HasScript(int questInstanceID, bool pre, int stepID = -1, bool isQuestStep = false);

		bool HasScript(Quest quest, bool pre, int stepID = -1, bool isQuestStep = false);

		void StartQuestScript(int questInstanceID, bool pre, bool isReward = false, int stepID = -1, bool isStepQuest = false);

		void StartQuestScript(Quest quest, bool pre, bool isReward = false, int stepID = -1, bool isStepQuest = false);

		void stop();

		void PauseQuestScripts();

		void ResumeQuestScripts();
	}
}
