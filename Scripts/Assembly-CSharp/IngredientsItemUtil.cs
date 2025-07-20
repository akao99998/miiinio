using System.Collections.Generic;
using Kampai.Game;

public static class IngredientsItemUtil
{
	public static int GetHarvestTimeFromIngredientDefinition(IngredientsItemDefinition iid, IDefinitionService definitionService)
	{
		int timeToHarvest = (int)iid.TimeToHarvest;
		if (timeToHarvest != 0)
		{
			return timeToHarvest;
		}
		List<VillainLairDefinition> all = definitionService.GetAll<VillainLairDefinition>();
		foreach (VillainLairDefinition item in all)
		{
			if (item.ResourceItemID == iid.ID)
			{
				return item.SecondsToHarvest;
			}
		}
		return timeToHarvest;
	}
}
