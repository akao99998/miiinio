using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class NamedCharacterAnimationDefinition : AnimationDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1074;
			}
		}

		public string StateMachine { get; set; }

		public float SpreadMin { get; set; }

		public float SpreadMax { get; set; }

		public int IdleCount { get; set; }

		public float AttentionDuration { get; set; }

		public CharacterUIAnimationDefinition characterUIAnimationDefinition { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, StateMachine);
			writer.Write(SpreadMin);
			writer.Write(SpreadMax);
			writer.Write(IdleCount);
			writer.Write(AttentionDuration);
			BinarySerializationUtil.WriteCharacterUIAnimationDefinition(writer, characterUIAnimationDefinition);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			StateMachine = BinarySerializationUtil.ReadString(reader);
			SpreadMin = reader.ReadSingle();
			SpreadMax = reader.ReadSingle();
			IdleCount = reader.ReadInt32();
			AttentionDuration = reader.ReadSingle();
			characterUIAnimationDefinition = BinarySerializationUtil.ReadCharacterUIAnimationDefinition(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STATEMACHINE":
				reader.Read();
				StateMachine = ReaderUtil.ReadString(reader, converters);
				break;
			case "SPREADMIN":
				reader.Read();
				SpreadMin = Convert.ToSingle(reader.Value);
				break;
			case "SPREADMAX":
				reader.Read();
				SpreadMax = Convert.ToSingle(reader.Value);
				break;
			case "IDLECOUNT":
				reader.Read();
				IdleCount = Convert.ToInt32(reader.Value);
				break;
			case "ATTENTIONDURATION":
				reader.Read();
				AttentionDuration = Convert.ToSingle(reader.Value);
				break;
			case "CHARACTERUIANIMATIONDEFINITION":
				reader.Read();
				characterUIAnimationDefinition = ReaderUtil.ReadCharacterUIAnimationDefinition(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
