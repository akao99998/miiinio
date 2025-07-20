using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PartyUpDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1006;
			}
		}

		public float Multiplier { get; set; }

		public List<int> PointsNeeded { get; set; }

		public TransactionDefinition PartyTransaction { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Multiplier);
			BinarySerializationUtil.WriteListInt32(writer, PointsNeeded);
			BinarySerializationUtil.WriteObject(writer, PartyTransaction);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Multiplier = reader.ReadSingle();
			PointsNeeded = BinarySerializationUtil.ReadListInt32(reader, PointsNeeded);
			PartyTransaction = BinarySerializationUtil.ReadObject<TransactionDefinition>(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MULTIPLIER":
				reader.Read();
				Multiplier = Convert.ToSingle(reader.Value);
				break;
			case "POINTSNEEDED":
				reader.Read();
				PointsNeeded = ReaderUtil.PopulateListInt32(reader, PointsNeeded);
				break;
			case "PARTYTRANSACTION":
				reader.Read();
				PartyTransaction = ((converters.transactionDefinitionConverter == null) ? FastJSONDeserializer.Deserialize<TransactionDefinition>(reader, converters) : converters.transactionDefinitionConverter.ReadJson(reader, converters));
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
