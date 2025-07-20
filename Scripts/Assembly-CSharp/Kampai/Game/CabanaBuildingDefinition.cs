using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class CabanaBuildingDefinition : AnimatingBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1040;
			}
		}

		public int CharacterID { get; set; }

		public string InactivePrefab { get; set; }

		public string DisheveledScene { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(CharacterID);
			BinarySerializationUtil.WriteString(writer, InactivePrefab);
			BinarySerializationUtil.WriteString(writer, DisheveledScene);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			CharacterID = reader.ReadInt32();
			InactivePrefab = BinarySerializationUtil.ReadString(reader);
			DisheveledScene = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "CHARACTERID":
				reader.Read();
				CharacterID = Convert.ToInt32(reader.Value);
				break;
			case "INACTIVEPREFAB":
				reader.Read();
				InactivePrefab = ReaderUtil.ReadString(reader, converters);
				break;
			case "DISHEVELEDSCENE":
				reader.Read();
				DisheveledScene = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new CabanaBuilding(this);
		}
	}
}
