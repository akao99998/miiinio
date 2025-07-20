using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class VillainDefinition : NamedCharacterDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1187;
			}
		}

		public int LoopCountMin { get; set; }

		public int LoopCountMax { get; set; }

		public string AsmCabana { get; set; }

		public string AsmFarewell { get; set; }

		public string AsmBoat { get; set; }

		public string WelcomeDialogKey { get; set; }

		public string FarewellDialogKey { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(LoopCountMin);
			writer.Write(LoopCountMax);
			BinarySerializationUtil.WriteString(writer, AsmCabana);
			BinarySerializationUtil.WriteString(writer, AsmFarewell);
			BinarySerializationUtil.WriteString(writer, AsmBoat);
			BinarySerializationUtil.WriteString(writer, WelcomeDialogKey);
			BinarySerializationUtil.WriteString(writer, FarewellDialogKey);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			LoopCountMin = reader.ReadInt32();
			LoopCountMax = reader.ReadInt32();
			AsmCabana = BinarySerializationUtil.ReadString(reader);
			AsmFarewell = BinarySerializationUtil.ReadString(reader);
			AsmBoat = BinarySerializationUtil.ReadString(reader);
			WelcomeDialogKey = BinarySerializationUtil.ReadString(reader);
			FarewellDialogKey = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "LOOPCOUNTMIN":
				reader.Read();
				LoopCountMin = Convert.ToInt32(reader.Value);
				break;
			case "LOOPCOUNTMAX":
				reader.Read();
				LoopCountMax = Convert.ToInt32(reader.Value);
				break;
			case "ASMCABANA":
				reader.Read();
				AsmCabana = ReaderUtil.ReadString(reader, converters);
				break;
			case "ASMFAREWELL":
				reader.Read();
				AsmFarewell = ReaderUtil.ReadString(reader, converters);
				break;
			case "ASMBOAT":
				reader.Read();
				AsmBoat = ReaderUtil.ReadString(reader, converters);
				break;
			case "WELCOMEDIALOGKEY":
				reader.Read();
				WelcomeDialogKey = ReaderUtil.ReadString(reader, converters);
				break;
			case "FAREWELLDIALOGKEY":
				reader.Read();
				FarewellDialogKey = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Instance Build()
		{
			return new Villain(this);
		}
	}
}
