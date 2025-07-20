using System;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DynamicQuestDefinition : QuestDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1129;
			}
		}

		public TransactionInstance RewardTransactionInstance { get; set; }

		public int DropStep { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteTransactionInstance(writer, RewardTransactionInstance);
			writer.Write(DropStep);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			RewardTransactionInstance = BinarySerializationUtil.ReadTransactionInstance(reader);
			DropStep = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					DropStep = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "REWARDTRANSACTIONINSTANCE":
				reader.Read();
				RewardTransactionInstance = ReaderUtil.ReadTransactionInstance(reader, converters);
				break;
			}
			return true;
		}

		public override TransactionDefinition GetReward(IDefinitionService definitionService)
		{
			if (RewardTransactionInstance != null)
			{
				return RewardTransactionInstance.ToDefinition();
			}
			return base.GetReward(definitionService);
		}
	}
}
