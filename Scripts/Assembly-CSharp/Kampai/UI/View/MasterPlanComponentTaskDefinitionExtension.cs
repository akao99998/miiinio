using Kampai.Game;
using Kampai.Main;

namespace Kampai.UI.View
{
	public static class MasterPlanComponentTaskDefinitionExtension
	{
		public static string GetTaskImage(this MasterPlanComponentTaskDefinition task, ILocalizationService localizationService, IDefinitionService definitionService, string defaultLocKey, out DisplayableDefinition displayableDefinition)
		{
			displayableDefinition = null;
			if (task == null)
			{
				return string.Empty;
			}
			string result = string.Empty;
			switch (task.Type)
			{
			case MasterPlanComponentTaskType.Deliver:
			case MasterPlanComponentTaskType.Collect:
			{
				ItemDefinition definition2;
				definitionService.TryGet<ItemDefinition>(task.requiredItemId, out definition2);
				result = definition2.LocalizedKey;
				displayableDefinition = definition2;
				break;
			}
			case MasterPlanComponentTaskType.CompleteOrders:
				displayableDefinition = new DisplayableDefinition
				{
					Image = "img_orderboard_item_fill",
					Mask = "img_orderboard_item_mask"
				};
				break;
			case MasterPlanComponentTaskType.PlayMiniGame:
			case MasterPlanComponentTaskType.MiniGameScore:
			case MasterPlanComponentTaskType.EarnPartyPoints:
			case MasterPlanComponentTaskType.EarnLeisurePartyPoints:
			case MasterPlanComponentTaskType.EarnMignettePartyPoints:
			case MasterPlanComponentTaskType.EarnSandDollars:
			{
				BuildingDefinition definition;
				bool buildingExists = definitionService.TryGet<BuildingDefinition>(task.requiredItemId, out definition);
				bool isGrindCurrency = task.Type == MasterPlanComponentTaskType.EarnSandDollars;
				result = SetBuildingImage(localizationService, definitionService, defaultLocKey, buildingExists, isGrindCurrency, definition, out displayableDefinition);
				break;
			}
			}
			return result;
		}

		public static string GetTaskImage(this MasterPlanComponentTaskDefinition task, KampaiImage itemImage, ILocalizationService localizationService, IDefinitionService definitionService, string defaultLocKey)
		{
			DisplayableDefinition displayableDefinition;
			string taskImage = task.GetTaskImage(localizationService, definitionService, defaultLocKey, out displayableDefinition);
			UIUtils.SetItemIcon(itemImage, displayableDefinition);
			return taskImage;
		}

		private static string SetBuildingImage(ILocalizationService localizationService, IDefinitionService definitionService, string defaultLocKey, bool buildingExists, bool isGrindCurrency, BuildingDefinition buildingDef, out DisplayableDefinition displayableDefinition)
		{
			string result;
			if (buildingExists)
			{
				result = ((localizationService != null) ? localizationService.GetString(buildingDef.LocalizedKey) : string.Empty);
				displayableDefinition = buildingDef;
			}
			else
			{
				int id = ((!isGrindCurrency) ? 2 : 0);
				result = ((localizationService != null) ? localizationService.GetString(defaultLocKey) : string.Empty);
				displayableDefinition = definitionService.Get<DisplayableDefinition>(id);
			}
			return result;
		}
	}
}
