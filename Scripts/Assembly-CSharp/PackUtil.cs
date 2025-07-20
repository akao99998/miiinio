using Kampai.Game;

public static class PackUtil
{
	public static bool HasPurchasedEnough(PackDefinition packDefinition, IPlayerService playerService)
	{
		int purchasedQuanityByUpsellID = playerService.GetPurchasedQuanityByUpsellID(packDefinition.ID);
		int num = packDefinition.CanBuyThisManyTimes;
		switch (num)
		{
		case 0:
			num = 1;
			break;
		case -1:
			num = int.MaxValue;
			break;
		}
		return purchasedQuanityByUpsellID >= num;
	}

	public static bool IsAudible(int id, PackDefinition packDefinition)
	{
		if (packDefinition.AudibleItemList == null)
		{
			return false;
		}
		return packDefinition.AudibleItemList.Contains(id);
	}

	public static bool IsItemExclusive(int id, PackDefinition packDefinition)
	{
		if (packDefinition.ExclusiveItemList == null)
		{
			return false;
		}
		return packDefinition.ExclusiveItemList.Contains(id);
	}
}
