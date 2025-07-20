using System;
using System.IO;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class ChurnTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1153;
			}
		}

		public float Value { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.Churn;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Value);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Value = reader.ReadSingle();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "VALUE":
				reader.Read();
				Value = Convert.ToSingle(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, ChurnValue: {3}", GetType(), base.conditionOp, type, Value);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			float actualValue = gameContext.injectionBinder.GetInstance<IPlayerService>().Churn();
			return TestOperator(Value, actualValue);
		}
	}
}
