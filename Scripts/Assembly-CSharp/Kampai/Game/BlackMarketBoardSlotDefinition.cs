using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class BlackMarketBoardSlotDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1037;
			}
		}

		public int SlotIndex { get; set; }

		public int MinQuantity { get; set; }

		public int MaxQuantity { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(SlotIndex);
			writer.Write(MinQuantity);
			writer.Write(MaxQuantity);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			SlotIndex = reader.ReadInt32();
			MinQuantity = reader.ReadInt32();
			MaxQuantity = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SLOTINDEX":
				reader.Read();
				SlotIndex = Convert.ToInt32(reader.Value);
				break;
			case "MINQUANTITY":
				reader.Read();
				MinQuantity = Convert.ToInt32(reader.Value);
				break;
			case "MAXQUANTITY":
				reader.Read();
				MaxQuantity = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
