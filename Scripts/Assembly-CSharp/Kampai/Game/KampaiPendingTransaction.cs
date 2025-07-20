using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class KampaiPendingTransaction
	{
		public string ExternalIdentifier { get; set; }

		[Serializer("KampaiPendingTransaction.SerializeDefinition")]
		public TransactionDefinition Transaction { get; set; }

		public TransactionInstance TransactionInstance { get; set; }

		public int StoreItemDefinitionId { get; set; }

		public int UTCTimeCreated { get; set; }

		internal static void SerializeDefinition(JsonWriter writer, TransactionDefinition value)
		{
			writer.WriteValue(value.ID);
		}
	}
}
