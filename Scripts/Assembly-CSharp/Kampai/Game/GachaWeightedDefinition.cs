using System;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class GachaWeightedDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1111;
			}
		}

		public int Minions { get; set; }

		public WeightedDefinition WeightedDefinition { get; set; }

		public bool PartyOnly { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Minions);
			BinarySerializationUtil.WriteObject(writer, WeightedDefinition);
			writer.Write(PartyOnly);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Minions = reader.ReadInt32();
			WeightedDefinition = BinarySerializationUtil.ReadObject<WeightedDefinition>(reader);
			PartyOnly = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MINIONS":
				reader.Read();
				Minions = Convert.ToInt32(reader.Value);
				break;
			case "WEIGHTEDDEFINITION":
				reader.Read();
				WeightedDefinition = FastJSONDeserializer.Deserialize<WeightedDefinition>(reader, converters);
				break;
			case "PARTYONLY":
				reader.Read();
				PartyOnly = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
