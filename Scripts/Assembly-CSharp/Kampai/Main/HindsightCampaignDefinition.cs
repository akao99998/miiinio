using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Game;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Main
{
	public class HindsightCampaignDefinition : Definition, IBuilder<Instance>, IUTCRangeable
	{
		public Dictionary<string, object> Content;

		public Dictionary<string, object> URI;

		public override int TypeCode
		{
			get
			{
				return 1193;
			}
		}

		public string Name { get; set; }

		public string Scope { get; set; }

		public string Platform { get; set; }

		public int Limit { get; set; }

		public int UTCStartDate { get; set; }

		public int UTCEndDate { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Name);
			BinarySerializationUtil.WriteString(writer, Scope);
			BinarySerializationUtil.WriteString(writer, Platform);
			writer.Write(Limit);
			writer.Write(UTCStartDate);
			writer.Write(UTCEndDate);
			BinarySerializationUtil.WriteDictionary(writer, Content);
			BinarySerializationUtil.WriteDictionary(writer, URI);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Name = BinarySerializationUtil.ReadString(reader);
			Scope = BinarySerializationUtil.ReadString(reader);
			Platform = BinarySerializationUtil.ReadString(reader);
			Limit = reader.ReadInt32();
			UTCStartDate = reader.ReadInt32();
			UTCEndDate = reader.ReadInt32();
			Content = BinarySerializationUtil.ReadDictionary(reader);
			URI = BinarySerializationUtil.ReadDictionary(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "NAME":
				reader.Read();
				Name = ReaderUtil.ReadString(reader, converters);
				break;
			case "SCOPE":
				reader.Read();
				Scope = ReaderUtil.ReadString(reader, converters);
				break;
			case "PLATFORM":
				reader.Read();
				Platform = ReaderUtil.ReadString(reader, converters);
				break;
			case "LIMIT":
				reader.Read();
				Limit = Convert.ToInt32(reader.Value);
				break;
			case "UTCSTARTDATE":
				reader.Read();
				UTCStartDate = Convert.ToInt32(reader.Value);
				break;
			case "UTCENDDATE":
				reader.Read();
				UTCEndDate = Convert.ToInt32(reader.Value);
				break;
			case "CONTENT":
				reader.Read();
				Content = ReaderUtil.ReadDictionary(reader);
				break;
			case "URI":
				reader.Read();
				URI = ReaderUtil.ReadDictionary(reader);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public Instance Build()
		{
			return new HindsightCampaign(this);
		}
	}
}
