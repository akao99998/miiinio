using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MasterPlanOnboardDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1109;
			}
		}

		public int nextOnboardDefinitionId { get; set; }

		public int CustomCameraPosID { get; set; }

		public GhostFunctionDefinition ghostFunction { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(nextOnboardDefinitionId);
			writer.Write(CustomCameraPosID);
			BinarySerializationUtil.WriteGhostFunctionDefinition(writer, ghostFunction);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			nextOnboardDefinitionId = reader.ReadInt32();
			CustomCameraPosID = reader.ReadInt32();
			ghostFunction = BinarySerializationUtil.ReadGhostFunctionDefinition(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "NEXTONBOARDDEFINITIONID":
				reader.Read();
				nextOnboardDefinitionId = Convert.ToInt32(reader.Value);
				break;
			case "CUSTOMCAMERAPOSID":
				reader.Read();
				CustomCameraPosID = Convert.ToInt32(reader.Value);
				break;
			case "GHOSTFUNCTION":
				reader.Read();
				ghostFunction = ReaderUtil.ReadGhostFunctionDefinition(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
