using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class CommonLandExpansionDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1097;
			}
		}

		public string MinionPrefab { get; set; }

		public string VFXGrassClearing { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, MinionPrefab);
			BinarySerializationUtil.WriteString(writer, VFXGrassClearing);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			MinionPrefab = BinarySerializationUtil.ReadString(reader);
			VFXGrassClearing = BinarySerializationUtil.ReadString(reader);
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
					VFXGrassClearing = ReaderUtil.ReadString(reader, converters);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "MINIONPREFAB":
				reader.Read();
				MinionPrefab = ReaderUtil.ReadString(reader, converters);
				break;
			}
			return true;
		}
	}
}
