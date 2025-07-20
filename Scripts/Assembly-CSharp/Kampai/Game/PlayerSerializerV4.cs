using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV4 : PlayerSerializerV3
	{
		public override int Version
		{
			get
			{
				return 4;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 4)
			{
				foreach (KampaiPendingTransaction pendingTransaction in player.GetPendingTransactions())
				{
					if (pendingTransaction.Transaction != null)
					{
						pendingTransaction.TransactionInstance = pendingTransaction.Transaction.ToInstance();
					}
				}
				player.Version = 4;
			}
			return player;
		}
	}
}
