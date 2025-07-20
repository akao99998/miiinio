using Kampai.Game;
using Kampai.Main;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SettingsLearnSystemsCategoryItemPanelMediator : Mediator
	{
		[Inject]
		public SettingsLearnSystemsCategoryItemPanelView view { get; set; }

		[Inject]
		public SettingsLearnSystemsCategorySelectedSignal categorySelectedSignal { get; set; }

		[Inject]
		public SettingsLearnSystemsCategoryItemSelectedSignal categoryItemSelectedSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IPlayerTrainingService playerTrainingService { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displayPlayerTrainingSignal { get; set; }

		public override void OnRegister()
		{
			categorySelectedSignal.AddListener(OnCategorySelected);
			categoryItemSelectedSignal.AddListener(OnCategoryItemSelected);
		}

		public override void OnRemove()
		{
			categorySelectedSignal.RemoveListener(OnCategorySelected);
			categoryItemSelectedSignal.RemoveListener(OnCategoryItemSelected);
		}

		private void OnCategorySelected(int categoryDefinitionId)
		{
			PlayerTrainingCategoryDefinition playerTrainingCategoryDefinition = definitionService.Get<PlayerTrainingCategoryDefinition>(categoryDefinitionId);
			if (playerTrainingCategoryDefinition == null)
			{
				return;
			}
			view.ClearPlayerTraining();
			foreach (int trainingDefinitionID in playerTrainingCategoryDefinition.trainingDefinitionIDs)
			{
				PlayerTrainingDefinition definition = definitionService.Get<PlayerTrainingDefinition>(trainingDefinitionID);
				view.AddPlayerTraining(definition, localizationService, playerTrainingService.HasSeen(trainingDefinitionID, PlayerTrainingVisiblityType.SETTINGS));
			}
		}

		private void OnCategoryItemSelected(int categoryItemDefinitionId)
		{
			playerTrainingService.MarkSeen(categoryItemDefinitionId, PlayerTrainingVisiblityType.SETTINGS);
			PlayerTrainingDefinition playerTrainingDefinition = definitionService.Get<PlayerTrainingDefinition>(categoryItemDefinitionId);
			if (playerTrainingDefinition != null)
			{
				view.AddPlayerTraining(playerTrainingDefinition, localizationService, playerTrainingService.HasSeen(categoryItemDefinitionId, PlayerTrainingVisiblityType.SETTINGS));
			}
			displayPlayerTrainingSignal.Dispatch(categoryItemDefinitionId, true, new Signal<bool>());
		}
	}
}
