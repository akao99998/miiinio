using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game.Trigger
{
	public class PrestigeTriggerConditionDefinition : PrestigeTriggerConditionDefinitionBase
	{
		public override int TypeCode
		{
			get
			{
				return 1166;
			}
		}

		public int duration { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.Prestige;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(duration);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			duration = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "DURATION":
				reader.Read();
				duration = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, duration: {3}", GetType(), base.conditionOp, type, duration);
		}

		protected override bool IsTriggered(IPrestigeService prestigeService, Prestige prestigeCharacter)
		{
			int idlePrestigeDuration = prestigeService.GetIdlePrestigeDuration(base.prestigeDefinitionID);
			return TestOperator(duration, idlePrestigeDuration);
		}
	}
}
