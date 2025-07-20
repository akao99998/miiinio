using System.Collections.Generic;

namespace Kampai.Game
{
	public interface IMasterPlanQuestService
	{
		Quest GetQuestByInstanceId(int id);

		List<Quest> GetQuests();

		QuestLine ConvertComponentToQuestLine(MasterPlanComponent component);

		QuestLine ConvertMasterPlanToQuestLine(MasterPlan masterPlan);

		Quest ConvertMasterPlanToQuest(MasterPlan masterPlan);

		Quest ConvertMasterPlanComponentToQuest(MasterPlanComponent component);

		Quest ConvertMasterPlanComponentToQuest(MasterPlanComponent component, bool buildTask);

		QuestStep ConvertMasterPlanComponentTaskToQuestStep(MasterPlanComponentTask task);

		QuestStepDefinition ConvertMasterPlanComponentTaskDefToQuestStepDef(MasterPlanComponentTaskDefinition task);

		QuestDefinition ConvertMasterPlanComponentDefToQuestDef(MasterPlanComponent component, int questLineId);
	}
}
