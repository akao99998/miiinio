using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class TaxonomyDefinition : DisplayableDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1014;
			}
		}

		public string TaxonomyHighLevel { get; set; }

		public string TaxonomySpecific { get; set; }

		public string TaxonomyType { get; set; }

		public string TaxonomyOther { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, TaxonomyHighLevel);
			BinarySerializationUtil.WriteString(writer, TaxonomySpecific);
			BinarySerializationUtil.WriteString(writer, TaxonomyType);
			BinarySerializationUtil.WriteString(writer, TaxonomyOther);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			TaxonomyHighLevel = BinarySerializationUtil.ReadString(reader);
			TaxonomySpecific = BinarySerializationUtil.ReadString(reader);
			TaxonomyType = BinarySerializationUtil.ReadString(reader);
			TaxonomyOther = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TAXONOMYHIGHLEVEL":
				reader.Read();
				TaxonomyHighLevel = ReaderUtil.ReadString(reader, converters);
				break;
			case "TAXONOMYSPECIFIC":
				reader.Read();
				TaxonomySpecific = ReaderUtil.ReadString(reader, converters);
				break;
			case "TAXONOMYTYPE":
				reader.Read();
				TaxonomyType = ReaderUtil.ReadString(reader, converters);
				break;
			case "TAXONOMYOTHER":
				reader.Read();
				TaxonomyOther = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
