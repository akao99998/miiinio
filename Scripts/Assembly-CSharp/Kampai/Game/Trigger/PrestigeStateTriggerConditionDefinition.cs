using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game.Trigger
{
	public class PrestigeStateTriggerConditionDefinition : PrestigeTriggerConditionDefinitionBase
	{
		public override int TypeCode
		{
			get
			{
				return 1165;
			}
		}

		public PrestigeState state { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.PrestigeState;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, state);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			state = BinarySerializationUtil.ReadEnum<PrestigeState>(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STATE":
				reader.Read();
				state = ReaderUtil.ReadEnum<PrestigeState>(reader);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, state: {3}", GetType(), base.conditionOp, type, state);
		}

		protected override bool IsTriggered(IPrestigeService prestigeService, Prestige prestigeCharacter)
		{
			return prestigeCharacter != null && TestOperator((int)state, (int)prestigeCharacter.state);
		}
	}
}
