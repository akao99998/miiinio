using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class BuildingAnimationDefinition : AnimationDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1031;
			}
		}

		public int CostumeId { get; set; }

		public float GagWeight { get; set; }

		public string BuildingController { get; set; }

		public string MinionController { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(CostumeId);
			writer.Write(GagWeight);
			BinarySerializationUtil.WriteString(writer, BuildingController);
			BinarySerializationUtil.WriteString(writer, MinionController);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			CostumeId = reader.ReadInt32();
			GagWeight = reader.ReadSingle();
			BuildingController = BinarySerializationUtil.ReadString(reader);
			MinionController = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "COSTUMEID":
				reader.Read();
				CostumeId = Convert.ToInt32(reader.Value);
				break;
			case "GAGWEIGHT":
				reader.Read();
				GagWeight = Convert.ToSingle(reader.Value);
				break;
			case "BUILDINGCONTROLLER":
				reader.Read();
				BuildingController = ReaderUtil.ReadString(reader, converters);
				break;
			case "MINIONCONTROLLER":
				reader.Read();
				MinionController = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
