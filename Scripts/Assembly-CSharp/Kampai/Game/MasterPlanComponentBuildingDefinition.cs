using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MasterPlanComponentBuildingDefinition : BuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1054;
			}
		}

		public Location placementLocation { get; set; }

		public string animationController { get; set; }

		public string environmentalAudio { get; set; }

		public string dropAnimationController { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteLocation(writer, placementLocation);
			BinarySerializationUtil.WriteString(writer, animationController);
			BinarySerializationUtil.WriteString(writer, environmentalAudio);
			BinarySerializationUtil.WriteString(writer, dropAnimationController);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			placementLocation = BinarySerializationUtil.ReadLocation(reader);
			animationController = BinarySerializationUtil.ReadString(reader);
			environmentalAudio = BinarySerializationUtil.ReadString(reader);
			dropAnimationController = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PLACEMENTLOCATION":
				reader.Read();
				placementLocation = ReaderUtil.ReadLocation(reader, converters);
				break;
			case "ANIMATIONCONTROLLER":
				reader.Read();
				animationController = ReaderUtil.ReadString(reader, converters);
				break;
			case "ENVIRONMENTALAUDIO":
				reader.Read();
				environmentalAudio = ReaderUtil.ReadString(reader, converters);
				break;
			case "DROPANIMATIONCONTROLLER":
				reader.Read();
				dropAnimationController = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new MasterPlanComponentBuilding(this);
		}
	}
}
