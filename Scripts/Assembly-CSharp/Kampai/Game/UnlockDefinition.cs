using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class UnlockDefinition : ItemDefinition
	{
		public int ReferencedDefinitionID;

		public int UnlockedQuantity;

		public bool Delta;

		public override int TypeCode
		{
			get
			{
				return 1148;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(ReferencedDefinitionID);
			writer.Write(UnlockedQuantity);
			writer.Write(Delta);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			ReferencedDefinitionID = reader.ReadInt32();
			UnlockedQuantity = reader.ReadInt32();
			Delta = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "REFERENCEDDEFINITIONID":
				reader.Read();
				ReferencedDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "UNLOCKEDQUANTITY":
				reader.Read();
				UnlockedQuantity = Convert.ToInt32(reader.Value);
				break;
			case "DELTA":
				reader.Read();
				Delta = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
