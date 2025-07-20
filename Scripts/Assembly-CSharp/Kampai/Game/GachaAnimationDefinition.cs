using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class GachaAnimationDefinition : AnimationDefinition
	{
		public const int INFINITE_MINIONS = 4;

		public override int TypeCode
		{
			get
			{
				return 1110;
			}
		}

		public int AnimationID { get; set; }

		public int Minions { get; set; }

		public string Prefab { get; set; }

		public TargetPerformance MinPerformance { get; set; }

		public bool SoloExit { get; set; }

		public KnuckleheadednessInfo knuckleheadednessInfo { get; set; }

		public AnimationAlternate AnimationAlternate { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(AnimationID);
			writer.Write(Minions);
			BinarySerializationUtil.WriteString(writer, Prefab);
			BinarySerializationUtil.WriteEnum(writer, MinPerformance);
			writer.Write(SoloExit);
			BinarySerializationUtil.WriteKnuckleheadednessInfo(writer, knuckleheadednessInfo);
			BinarySerializationUtil.WriteAnimationAlternate(writer, AnimationAlternate);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			AnimationID = reader.ReadInt32();
			Minions = reader.ReadInt32();
			Prefab = BinarySerializationUtil.ReadString(reader);
			MinPerformance = BinarySerializationUtil.ReadEnum<TargetPerformance>(reader);
			SoloExit = reader.ReadBoolean();
			knuckleheadednessInfo = BinarySerializationUtil.ReadKnuckleheadednessInfo(reader);
			AnimationAlternate = BinarySerializationUtil.ReadAnimationAlternate(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ANIMATIONID":
				reader.Read();
				AnimationID = Convert.ToInt32(reader.Value);
				break;
			case "MINIONS":
				reader.Read();
				Minions = Convert.ToInt32(reader.Value);
				break;
			case "PREFAB":
				reader.Read();
				Prefab = ReaderUtil.ReadString(reader, converters);
				break;
			case "MINPERFORMANCE":
				reader.Read();
				MinPerformance = ReaderUtil.ReadEnum<TargetPerformance>(reader);
				break;
			case "SOLOEXIT":
				reader.Read();
				SoloExit = Convert.ToBoolean(reader.Value);
				break;
			case "KNUCKLEHEADEDNESSINFO":
				reader.Read();
				knuckleheadednessInfo = ReaderUtil.ReadKnuckleheadednessInfo(reader, converters);
				break;
			case "ANIMATIONALTERNATE":
				reader.Read();
				AnimationAlternate = ReaderUtil.ReadAnimationAlternate(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
