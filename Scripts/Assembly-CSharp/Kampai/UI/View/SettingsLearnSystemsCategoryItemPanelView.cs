using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class SettingsLearnSystemsCategoryItemPanelView : KampaiView
	{
		public Transform PlayerTrainingParent;

		private Dictionary<int, SettingsLearnSystemsCategoryItemView> children = new Dictionary<int, SettingsLearnSystemsCategoryItemView>();

		internal void AddPlayerTraining(PlayerTrainingDefinition definition, ILocalizationService localizationService, bool hasSeen)
		{
			int iD = definition.ID;
			if (children.ContainsKey(iD))
			{
				children[iD].Init(definition, localizationService, hasSeen);
				return;
			}
			GameObject gameObject = Object.Instantiate(KampaiResources.Load<GameObject>("cmp_SettingsPlayerTrainingCategoryItem"));
			gameObject.transform.SetParent(PlayerTrainingParent, false);
			SettingsLearnSystemsCategoryItemView component = gameObject.GetComponent<SettingsLearnSystemsCategoryItemView>();
			component.Init(definition, localizationService, hasSeen);
			children[definition.ID] = component;
		}

		internal void ClearPlayerTraining()
		{
			foreach (SettingsLearnSystemsCategoryItemView value in children.Values)
			{
				Object.Destroy(value.gameObject);
			}
			children.Clear();
		}
	}
}
