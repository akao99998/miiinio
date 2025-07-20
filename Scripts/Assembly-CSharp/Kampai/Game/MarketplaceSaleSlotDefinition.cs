using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MarketplaceSaleSlotDefinition : Definition, IBuilder<Instance>
	{
		public enum SlotType
		{
			DEFAULT = 0,
			FACEBOOK_UNLOCKABLE = 1,
			PREMIUM_UNLOCKABLE = 2
		}

		public override int TypeCode
		{
			get
			{
				return 1104;
			}
		}

		public SlotType type { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, type);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			type = BinarySerializationUtil.ReadEnum<SlotType>(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TYPE":
				reader.Read();
				type = ReaderUtil.ReadEnum<SlotType>(reader);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public Instance Build()
		{
			return new MarketplaceSaleSlot(this);
		}
	}
}
