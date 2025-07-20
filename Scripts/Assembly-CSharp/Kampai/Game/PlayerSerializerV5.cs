using System;
using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV5 : PlayerSerializerV4
	{
		private const int FTUE_LEVEL_COMPLETED_V4_AND_UNDER = 7;

		private const int XP_TO_LEVEL_UP = 4;

		public override int Version
		{
			get
			{
				return 5;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 5)
			{
				if (player.HighestFtueLevel < 7)
				{
					long iD = player.ID;
					string country = player.country;
					logger.Warning("Old user has not completed ftue, let's reset their inventory.");
					json = definitionService.GetInitialPlayer();
					player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
					player.ID = iD;
					player.country = country;
				}
				else
				{
					Array values = Enum.GetValues(typeof(FtueLevel));
					player.HighestFtueLevel = (int)values.GetValue(values.Length - 1);
					player.AddUnlock(80000, 1);
					int quantity = (int)player.GetQuantity(StaticItem.LEVEL_ID);
					int quantity2 = (int)player.GetQuantity(StaticItem.XP_ID);
					Tuple<int, int> tuple = partyService.V4toV5UpdatePartyPointsAndIndex(quantity, quantity2);
					if (tuple != null)
					{
						player.SetQuantityByStaticItem(StaticItem.XP_ID, (uint)tuple.Item1);
						player.SetQuantityByStaticItem(StaticItem.LEVEL_PARTY_INDEX_ID, (uint)tuple.Item2);
					}
					Instance firstInstanceByDefinitionId = player.GetFirstInstanceByDefinitionId<Instance>(4);
					if (firstInstanceByDefinitionId != null)
					{
						player.Remove(firstInstanceByDefinitionId);
					}
					else
					{
						logger.Warning("Old user does not have XP To Level Up ID in their inventory");
					}
				}
				PurchasedLandExpansion byInstanceId = player.GetByInstanceId<PurchasedLandExpansion>(354);
				if (byInstanceId.HasPurchased(1579032) && !byInstanceId.HasPurchased(1579033))
				{
					byInstanceId.PurchasedExpansions.Add(1579033);
				}
				if (byInstanceId.HasPurchased(1973790) && !byInstanceId.HasPurchased(1973791))
				{
					byInstanceId.PurchasedExpansions.Add(1973791);
				}
				if (byInstanceId.HasPurchased(2960685) && !byInstanceId.HasPurchased(2960686))
				{
					byInstanceId.PurchasedExpansions.Add(2960686);
				}
				if (byInstanceId.HasPurchased(3355443) && !byInstanceId.HasPurchased(3355444))
				{
					byInstanceId.PurchasedExpansions.Add(3355444);
				}
				player.Version = 5;
			}
			return player;
		}
	}
}
