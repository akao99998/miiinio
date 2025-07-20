using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SettingsLearnSystemsCategoryPanelView : KampaiView
	{
		public Transform CategoryParent;

		public Signal<int> OnSelectCategorySignal = new Signal<int>();

		private bool firstCategory;

		internal void AddCategory(PlayerTrainingCategoryDefinition categoryDefinition, ILocalizationService localizationService)
		{
			GameObject gameObject = Object.Instantiate(KampaiResources.Load<GameObject>("cmp_SettingsPlayerTrainingCategory"));
			gameObject.transform.SetParent(CategoryParent, false);
			SettingsLearnSystemsCategoryView component = gameObject.GetComponent<SettingsLearnSystemsCategoryView>();
			bool selected = false;
			if (!firstCategory)
			{
				firstCategory = true;
				selected = true;
			}
			component.Init(categoryDefinition, localizationService, selected);
		}
	}
}
