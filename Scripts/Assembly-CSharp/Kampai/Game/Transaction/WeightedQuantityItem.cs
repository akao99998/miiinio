using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game.Transaction
{
	public class WeightedQuantityItem : QuantityItem
	{
		public override int TypeCode
		{
			get
			{
				return 1023;
			}
		}

		public uint Weight { get; set; }

		public WeightedQuantityItem()
		{
			base.Quantity = 1u;
		}

		public WeightedQuantityItem(int id, uint quantity, uint weight)
			: base(id, quantity)
		{
			Weight = weight;
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Weight);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Weight = reader.ReadUInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "WEIGHT":
				reader.Read();
				Weight = Convert.ToUInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
