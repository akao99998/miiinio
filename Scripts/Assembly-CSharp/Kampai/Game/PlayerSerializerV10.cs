using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV10 : PlayerSerializerV9
	{
		public override int Version
		{
			get
			{
				return 10;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 10)
			{
				FixCorruptedQuestInstanceIDs(player);
				player.Version = 10;
			}
			return player;
		}

		private void FixCorruptedQuestInstanceIDs(Player player)
		{
			if (player.nextId < 3146)
			{
				player.nextId = 3146;
			}
			List<int> list = new List<int>();
			list.Add(3140);
			list.Add(3141);
			list.Add(3142);
			list.Add(3143);
			list.Add(3144);
			list.Add(3145);
			List<int> list2 = list;
			foreach (Quest item in player.GetInstancesByType<Quest>())
			{
				if (list2.Contains(item.ID))
				{
					player.AssignNextInstanceId(item);
				}
			}
		}
	}
}
