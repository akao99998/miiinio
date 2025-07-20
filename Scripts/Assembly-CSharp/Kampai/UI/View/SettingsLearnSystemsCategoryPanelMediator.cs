using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class SettingsLearnSystemsCategoryPanelMediator : Mediator
	{
		[Inject]
		public SettingsLearnSystemsCategoryPanelView view { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public SettingsLearnSystemsCategorySelectedSignal categorySelectedSignal { get; set; }

		public override void OnRegister()
		{
			SetupCategories();
		}

		private void SetupCategories()
		{
			List<PlayerTrainingCategoryDefinition> all = definitionService.GetAll<PlayerTrainingCategoryDefinition>();
			if (all == null || all.Count < 1)
			{
				return;
			}
			foreach (PlayerTrainingCategoryDefinition item in all)
			{
				view.AddCategory(item, localizationService);
			}
			categorySelectedSignal.Dispatch(all[0].ID);
		}
	}
}
