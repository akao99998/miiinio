using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV12 : PlayerSerializerV11
	{
		public override int Version
		{
			get
			{
				return 12;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 12)
			{
				FixOldMasterPlan(player);
				player.Version = 12;
			}
			return player;
		}

		private void FixOldMasterPlan(Player player)
		{
			IList<MasterPlan> instancesByType = player.GetInstancesByType<MasterPlan>();
			if (instancesByType == null || instancesByType.Count != 1)
			{
				return;
			}
			int iD = instancesByType[0].ID;
			IList<MasterPlanComponent> instancesByType2 = player.GetInstancesByType<MasterPlanComponent>();
			foreach (MasterPlanComponent item in instancesByType2)
			{
				if (iD != item.planTrackingInstance)
				{
					player.Remove(item);
				}
			}
		}
	}
}
