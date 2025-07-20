using System;
using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV6 : PlayerSerializerV5
	{
		public override int Version
		{
			get
			{
				return 6;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 6)
			{
				PurchasedLandExpansion byInstanceId = player.GetByInstanceId<PurchasedLandExpansion>(354);
				if (player.GetCountByDefinitionId(3509) > 0 && !byInstanceId.HasPurchased(1579033))
				{
					byInstanceId.PurchasedExpansions.Add(1579033);
				}
				if (player.GetCountByDefinitionId(3505) > 0 && !byInstanceId.HasPurchased(1973791))
				{
					byInstanceId.PurchasedExpansions.Add(1973791);
				}
				if (player.GetCountByDefinitionId(3508) > 0 && !byInstanceId.HasPurchased(2960686))
				{
					byInstanceId.PurchasedExpansions.Add(2960686);
				}
				if (player.GetCountByDefinitionId(3507) > 0 && !byInstanceId.HasPurchased(3355444))
				{
					byInstanceId.PurchasedExpansions.Add(3355444);
				}
				if (player.GetCountByDefinitionId(3104) > 0 && !byInstanceId.HasPurchased(9671571))
				{
					byInstanceId.PurchasedExpansions.Add(9671571);
				}
				int craftingStartTime = Convert.ToInt32((DateTime.UtcNow - GameConstants.Timers.epochStart).TotalSeconds);
				foreach (CraftingBuilding item in player.GetInstancesByType<CraftingBuilding>())
				{
					if (item.PartyTimeReduction < 0)
					{
						item.PartyTimeReduction = 0;
						item.CraftingStartTime = craftingStartTime;
					}
				}
				player.Version = 6;
			}
			return player;
		}
	}
}
