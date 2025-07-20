using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game.Transaction
{
	public class WeightedDefinition : Definition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1022;
			}
		}

		public IList<WeightedQuantityItem> Entities { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, Entities);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Entities = BinarySerializationUtil.ReadList(reader, Entities);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ENTITIES":
				reader.Read();
				Entities = ReaderUtil.PopulateList(reader, converters, Entities);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public Instance Build()
		{
			return new WeightedInstance(this);
		}
	}
}
