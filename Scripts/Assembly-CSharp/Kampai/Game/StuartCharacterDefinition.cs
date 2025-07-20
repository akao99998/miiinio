using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class StuartCharacterDefinition : FrolicCharacterDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1082;
			}
		}

		public string StageStateMachine { get; set; }

		public MinionAnimationDefinition OnStageAnimation { get; set; }

		public int OnStageIdleAnimationCount { get; set; }

		public int OnStageTicketFilledAnimationCount { get; set; }

		public int OnStagePerformAnimationCount { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, StageStateMachine);
			BinarySerializationUtil.WriteObject(writer, OnStageAnimation);
			writer.Write(OnStageIdleAnimationCount);
			writer.Write(OnStageTicketFilledAnimationCount);
			writer.Write(OnStagePerformAnimationCount);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			StageStateMachine = BinarySerializationUtil.ReadString(reader);
			OnStageAnimation = BinarySerializationUtil.ReadObject<MinionAnimationDefinition>(reader);
			OnStageIdleAnimationCount = reader.ReadInt32();
			OnStageTicketFilledAnimationCount = reader.ReadInt32();
			OnStagePerformAnimationCount = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STAGESTATEMACHINE":
				reader.Read();
				StageStateMachine = ReaderUtil.ReadString(reader, converters);
				break;
			case "ONSTAGEANIMATION":
				reader.Read();
				OnStageAnimation = FastJSONDeserializer.Deserialize<MinionAnimationDefinition>(reader, converters);
				break;
			case "ONSTAGEIDLEANIMATIONCOUNT":
				reader.Read();
				OnStageIdleAnimationCount = Convert.ToInt32(reader.Value);
				break;
			case "ONSTAGETICKETFILLEDANIMATIONCOUNT":
				reader.Read();
				OnStageTicketFilledAnimationCount = Convert.ToInt32(reader.Value);
				break;
			case "ONSTAGEPERFORMANIMATIONCOUNT":
				reader.Read();
				OnStagePerformAnimationCount = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Instance Build()
		{
			return new StuartCharacter(this);
		}
	}
}
