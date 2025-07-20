using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class BuffDefinition : DisplayableDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1114;
			}
		}

		public BuffType buffType { get; set; }

		public List<float> starMultiplierValue { get; set; }

		public string buffSimpleMask { get; set; }

		public string buffDetailLocalizedKey { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, buffType);
			BinarySerializationUtil.WriteListSingle(writer, starMultiplierValue);
			BinarySerializationUtil.WriteString(writer, buffSimpleMask);
			BinarySerializationUtil.WriteString(writer, buffDetailLocalizedKey);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			buffType = BinarySerializationUtil.ReadEnum<BuffType>(reader);
			starMultiplierValue = BinarySerializationUtil.ReadListSingle(reader, starMultiplierValue);
			buffSimpleMask = BinarySerializationUtil.ReadString(reader);
			buffDetailLocalizedKey = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BUFFTYPE":
				reader.Read();
				buffType = ReaderUtil.ReadEnum<BuffType>(reader);
				break;
			case "STARMULTIPLIERVALUE":
				reader.Read();
				starMultiplierValue = ReaderUtil.PopulateListSingle(reader, starMultiplierValue);
				break;
			case "BUFFSIMPLEMASK":
				reader.Read();
				buffSimpleMask = ReaderUtil.ReadString(reader, converters);
				break;
			case "BUFFDETAILLOCALIZEDKEY":
				reader.Read();
				buffDetailLocalizedKey = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
