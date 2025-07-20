using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class CompositeBuildingPieceDefinition : DisplayableDefinition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1042;
			}
		}

		public string PrefabName { get; set; }

		public int BuildingDefinitionID { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, PrefabName);
			writer.Write(BuildingDefinitionID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			PrefabName = BinarySerializationUtil.ReadString(reader);
			BuildingDefinitionID = reader.ReadInt32();
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
					BuildingDefinitionID = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "PREFABNAME":
				reader.Read();
				PrefabName = ReaderUtil.ReadString(reader, converters);
				break;
			}
			return true;
		}

		public Instance Build()
		{
			return new CompositeBuildingPiece(this);
		}
	}
}
