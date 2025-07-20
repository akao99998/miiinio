using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PlayerTrainingCardDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1123;
			}
		}

		public string cardTitleLocalizedKey { get; set; }

		public string cardDescriptionLocalizedKey { get; set; }

		public List<ImageMaskCombo> cardImages { get; set; }

		public int prestigeDefinitionID { get; set; }

		public int buildingDefinitionID { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, cardTitleLocalizedKey);
			BinarySerializationUtil.WriteString(writer, cardDescriptionLocalizedKey);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteImageMaskCombo, cardImages);
			writer.Write(prestigeDefinitionID);
			writer.Write(buildingDefinitionID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			cardTitleLocalizedKey = BinarySerializationUtil.ReadString(reader);
			cardDescriptionLocalizedKey = BinarySerializationUtil.ReadString(reader);
			cardImages = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadImageMaskCombo, cardImages);
			prestigeDefinitionID = reader.ReadInt32();
			buildingDefinitionID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "CARDTITLELOCALIZEDKEY":
				reader.Read();
				cardTitleLocalizedKey = ReaderUtil.ReadString(reader, converters);
				break;
			case "CARDDESCRIPTIONLOCALIZEDKEY":
				reader.Read();
				cardDescriptionLocalizedKey = ReaderUtil.ReadString(reader, converters);
				break;
			case "CARDIMAGES":
				reader.Read();
				cardImages = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadImageMaskCombo, cardImages);
				break;
			case "PRESTIGEDEFINITIONID":
				reader.Read();
				prestigeDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "BUILDINGDEFINITIONID":
				reader.Read();
				buildingDefinitionID = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
