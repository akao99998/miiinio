using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public class CurrencyItemDefinition : TaxonomyDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1139;
			}
		}

		public string VFX { get; set; }

		public Vector3Serialize VFXOffset { get; set; }

		public string Audio { get; set; }

		public bool COPPAGated { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, VFX);
			BinarySerializationUtil.WriteVector3Serialize(writer, VFXOffset);
			BinarySerializationUtil.WriteString(writer, Audio);
			writer.Write(COPPAGated);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			VFX = BinarySerializationUtil.ReadString(reader);
			VFXOffset = BinarySerializationUtil.ReadVector3Serialize(reader);
			Audio = BinarySerializationUtil.ReadString(reader);
			COPPAGated = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "VFX":
				reader.Read();
				VFX = ReaderUtil.ReadString(reader, converters);
				break;
			case "VFXOFFSET":
				reader.Read();
				VFXOffset = ReaderUtil.ReadVector3Serialize(reader, converters);
				break;
			case "AUDIO":
				reader.Read();
				Audio = ReaderUtil.ReadString(reader, converters);
				break;
			case "COPPAGATED":
				reader.Read();
				COPPAGated = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
