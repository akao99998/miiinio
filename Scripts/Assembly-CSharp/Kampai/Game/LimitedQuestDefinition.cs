using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class LimitedQuestDefinition : QuestDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1131;
			}
		}

		public int ServerStartTimeUTC { get; set; }

		public int ServerStopTimeUTC { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(ServerStartTimeUTC);
			writer.Write(ServerStopTimeUTC);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			ServerStartTimeUTC = reader.ReadInt32();
			ServerStopTimeUTC = reader.ReadInt32();
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
					ServerStopTimeUTC = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "SERVERSTARTTIMEUTC":
				reader.Read();
				ServerStartTimeUTC = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}
	}
}
