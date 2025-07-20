using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class BridgeBuildingDefinition : AnimatingBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1039;
			}
		}

		public string AspirationalMessage { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, AspirationalMessage);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			AspirationalMessage = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ASPIRATIONALMESSAGE":
				reader.Read();
				AspirationalMessage = ReaderUtil.ReadString(reader, converters);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override Building BuildBuilding()
		{
			return new BridgeBuilding(this);
		}
	}
}
