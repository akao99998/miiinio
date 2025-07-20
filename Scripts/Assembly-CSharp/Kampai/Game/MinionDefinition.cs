using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MinionDefinition : Definition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1112;
			}
		}

		public uint Eyes { get; set; }

		public MinionBody Body { get; set; }

		public MinionHair Hair { get; set; }

		public int LeisureCooldownTime { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Eyes);
			BinarySerializationUtil.WriteEnum(writer, Body);
			BinarySerializationUtil.WriteEnum(writer, Hair);
			writer.Write(LeisureCooldownTime);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Eyes = reader.ReadUInt32();
			Body = BinarySerializationUtil.ReadEnum<MinionBody>(reader);
			Hair = BinarySerializationUtil.ReadEnum<MinionHair>(reader);
			LeisureCooldownTime = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "EYES":
				reader.Read();
				Eyes = Convert.ToUInt32(reader.Value);
				break;
			case "BODY":
				reader.Read();
				Body = ReaderUtil.ReadEnum<MinionBody>(reader);
				break;
			case "HAIR":
				reader.Read();
				Hair = ReaderUtil.ReadEnum<MinionHair>(reader);
				break;
			case "LEISURECOOLDOWNTIME":
				reader.Read();
				LeisureCooldownTime = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public Instance Build()
		{
			return new Minion(this);
		}
	}
}
