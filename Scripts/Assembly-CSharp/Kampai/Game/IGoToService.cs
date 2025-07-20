namespace Kampai.Game
{
	public interface IGoToService
	{
		void GoToClicked(QuestStep step, QuestStepDefinition stepDefinition, IQuestController questController, int stepNumber);

		void GoToClicked(MasterPlanQuestType.ComponentTaskDefinition taskDefinition);

		void GoToBuildingFromItem(int itemDefID);

		void OpenStoreFromAnywhere(int buildingDefinitionID);
	}
}
