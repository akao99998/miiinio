using Kampai.Main;
using UnityEngine;

namespace Kampai.Game
{
	public interface IQuestStepController
	{
		string DeliverButtonLocKey { get; }

		QuestStepType StepType { get; }

		QuestStepState StepState { get; }

		int StepInstanceTrackedID { get; }

		int ItemDefinitionID { get; }

		bool NeedActiveDeliverButton { get; }

		bool NeedActiveProgressBar { get; }

		bool NeedGoToButton { get; }

		int AmountNeeded { get; }

		int ProgressBarAmount { get; }

		int ProgressBarTotal { get; }

		string GetStepAction(ILocalizationService localService);

		string GetStepDescription(ILocalizationService localService, IDefinitionService defService);

		void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite);

		void SetupTracking();

		int IsTrackingOneOffCraftable(int itemDefinitionID);

		void GoToNextState(bool isTaskComplete = false);

		void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId);
	}
}
