using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game.Trigger
{
	public class PrestigeLevelTriggerConditionDefinition : PrestigeTriggerConditionDefinitionBase
	{
		public override int TypeCode
		{
			get
			{
				return 1163;
			}
		}

		public int level { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.PrestigeLevel;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(level);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			level = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "LEVEL":
				reader.Read();
				level = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, level: {3}", GetType(), base.conditionOp, type, level);
		}

		protected override bool IsTriggered(IPrestigeService prestigeService, Prestige prestigeCharacter)
		{
			return prestigeCharacter != null && TestOperator(level, prestigeCharacter.CurrentPrestigeLevel);
		}
	}
}
