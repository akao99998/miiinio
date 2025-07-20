using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV14 : PlayerSerializerV13
	{
		public override int Version
		{
			get
			{
				return 14;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 14)
			{
				FixPartyPauseItems(player);
				FixOverflowPrestigePoints(player);
				UpdateStagePosition(player);
				player.Version = 14;
			}
			return player;
		}

		private void FixPartyPauseItems(Player player)
		{
			TryToRemoveItem(player, 366);
			TryToRemoveItem(player, 367);
			TryToRemoveItem(player, 368);
			TryToRemoveItem(player, 708);
		}

		private void TryToRemoveItem(Player player, int definitionID)
		{
			int quantityByDefinitionId = (int)player.GetQuantityByDefinitionId(definitionID);
			if (quantityByDefinitionId > 0)
			{
				player.AlterQuantity(definitionID, -quantityByDefinitionId);
			}
		}

		private void FixOverflowPrestigePoints(Player player)
		{
			List<Prestige> instancesByType = player.GetInstancesByType<Prestige>();
			foreach (Prestige item in instancesByType)
			{
				int neededPrestigePoints = item.NeededPrestigePoints;
				if (neededPrestigePoints != 0 && item.CurrentPrestigePoints >= neededPrestigePoints)
				{
					item.CurrentPrestigePoints = item.NeededPrestigePoints - 1;
				}
			}
		}

		private void UpdateStagePosition(Player player)
		{
			StageBuilding byInstanceId = player.GetByInstanceId<StageBuilding>(370);
			if (byInstanceId != null)
			{
				byInstanceId.Location.x = 107;
				byInstanceId.Location.y = 171;
			}
		}
	}
}
