using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class StickerDefinition : DisplayableDefinition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1141;
			}
		}

		public int CharacterID { get; set; }

		public bool IsLimitedTime { get; set; }

		public int EventDefinitionID { get; set; }

		public int UnlockLevel { get; set; }

		public bool deprecated { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(CharacterID);
			writer.Write(IsLimitedTime);
			writer.Write(EventDefinitionID);
			writer.Write(UnlockLevel);
			writer.Write(deprecated);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			CharacterID = reader.ReadInt32();
			IsLimitedTime = reader.ReadBoolean();
			EventDefinitionID = reader.ReadInt32();
			UnlockLevel = reader.ReadInt32();
			deprecated = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "CHARACTERID":
				reader.Read();
				CharacterID = Convert.ToInt32(reader.Value);
				break;
			case "ISLIMITEDTIME":
				reader.Read();
				IsLimitedTime = Convert.ToBoolean(reader.Value);
				break;
			case "EVENTDEFINITIONID":
				reader.Read();
				EventDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "UNLOCKLEVEL":
				reader.Read();
				UnlockLevel = Convert.ToInt32(reader.Value);
				break;
			case "DEPRECATED":
				reader.Read();
				deprecated = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public Instance Build()
		{
			return new Sticker(this);
		}
	}
}
