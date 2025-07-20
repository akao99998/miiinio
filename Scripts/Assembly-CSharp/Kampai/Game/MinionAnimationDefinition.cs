using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MinionAnimationDefinition : AnimationDefinition
	{
		public Dictionary<string, object> arguments;

		public override int TypeCode
		{
			get
			{
				return 1076;
			}
		}

		public string StateMachine { get; set; }

		public bool FaceCamera { get; set; }

		public float AnimationSeconds { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, StateMachine);
			writer.Write(FaceCamera);
			writer.Write(AnimationSeconds);
			BinarySerializationUtil.WriteDictionary(writer, arguments);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			StateMachine = BinarySerializationUtil.ReadString(reader);
			FaceCamera = reader.ReadBoolean();
			AnimationSeconds = reader.ReadSingle();
			arguments = BinarySerializationUtil.ReadDictionary(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STATEMACHINE":
				reader.Read();
				StateMachine = ReaderUtil.ReadString(reader, converters);
				break;
			case "FACECAMERA":
				reader.Read();
				FaceCamera = Convert.ToBoolean(reader.Value);
				break;
			case "ANIMATIONSECONDS":
				reader.Read();
				AnimationSeconds = Convert.ToSingle(reader.Value);
				break;
			case "ARGUMENTS":
				reader.Read();
				arguments = ReaderUtil.ReadDictionary(reader);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
