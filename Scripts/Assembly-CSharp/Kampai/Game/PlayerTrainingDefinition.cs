using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PlayerTrainingDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1125;
			}
		}

		public string trainingTitleLocalizedKey { get; set; }

		public int cardOneDefinitionID { get; set; }

		public int cardTwoDefinitionID { get; set; }

		public int cardThreeDefinitionID { get; set; }

		public TransitionType transitionOne { get; set; }

		public TransitionType transitionTwo { get; set; }

		public bool disableAutomaticDisplay { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, trainingTitleLocalizedKey);
			writer.Write(cardOneDefinitionID);
			writer.Write(cardTwoDefinitionID);
			writer.Write(cardThreeDefinitionID);
			BinarySerializationUtil.WriteEnum(writer, transitionOne);
			BinarySerializationUtil.WriteEnum(writer, transitionTwo);
			writer.Write(disableAutomaticDisplay);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			trainingTitleLocalizedKey = BinarySerializationUtil.ReadString(reader);
			cardOneDefinitionID = reader.ReadInt32();
			cardTwoDefinitionID = reader.ReadInt32();
			cardThreeDefinitionID = reader.ReadInt32();
			transitionOne = BinarySerializationUtil.ReadEnum<TransitionType>(reader);
			transitionTwo = BinarySerializationUtil.ReadEnum<TransitionType>(reader);
			disableAutomaticDisplay = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TRAININGTITLELOCALIZEDKEY":
				reader.Read();
				trainingTitleLocalizedKey = ReaderUtil.ReadString(reader, converters);
				break;
			case "CARDONEDEFINITIONID":
				reader.Read();
				cardOneDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "CARDTWODEFINITIONID":
				reader.Read();
				cardTwoDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "CARDTHREEDEFINITIONID":
				reader.Read();
				cardThreeDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "TRANSITIONONE":
				reader.Read();
				transitionOne = ReaderUtil.ReadEnum<TransitionType>(reader);
				break;
			case "TRANSITIONTWO":
				reader.Read();
				transitionTwo = ReaderUtil.ReadEnum<TransitionType>(reader);
				break;
			case "DISABLEAUTOMATICDISPLAY":
				reader.Read();
				disableAutomaticDisplay = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
