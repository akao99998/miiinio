using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class LegalDocumentDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1004;
			}
		}

		public LegalDocuments.LegalType type { get; set; }

		public List<LegalDocumentURL> urls { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, type);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteLegalDocumentURL, urls);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			type = BinarySerializationUtil.ReadEnum<LegalDocuments.LegalType>(reader);
			urls = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadLegalDocumentURL, urls);
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
					urls = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadLegalDocumentURL, urls);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "TYPE":
				reader.Read();
				type = ReaderUtil.ReadEnum<LegalDocuments.LegalType>(reader);
				break;
			}
			return true;
		}
	}
}
