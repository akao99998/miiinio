using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class BlackMarketBoardUnlockedOrderSlotDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1036;
			}
		}

		public int UnlockLevel { get; set; }

		public int OrderSlots { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(UnlockLevel);
			writer.Write(OrderSlots);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			UnlockLevel = reader.ReadInt32();
			OrderSlots = reader.ReadInt32();
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
					OrderSlots = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "UNLOCKLEVEL":
				reader.Read();
				UnlockLevel = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}
	}
}
