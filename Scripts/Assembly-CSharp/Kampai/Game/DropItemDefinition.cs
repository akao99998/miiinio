using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DropItemDefinition : ItemDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1091;
			}
		}

		public float Rarity { get; set; }

		public DropType dropType { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Rarity);
			BinarySerializationUtil.WriteEnum(writer, dropType);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Rarity = reader.ReadSingle();
			dropType = BinarySerializationUtil.ReadEnum<DropType>(reader);
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
					dropType = ReaderUtil.ReadEnum<DropType>(reader);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "RARITY":
				reader.Read();
				Rarity = Convert.ToSingle(reader.Value);
				break;
			}
			return true;
		}
	}
}
