using System;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	[RequiresJsonConverter]
	public abstract class TriggerConditionDefinition : Definition, IIsTriggerable
	{
		public override int TypeCode
		{
			get
			{
				return 1020;
			}
		}

		public TriggerConditionOperator conditionOp { get; set; }

		public abstract TriggerConditionType.Identifier type { get; }

		public TriggerConditionDefinition()
		{
			conditionOp = TriggerConditionOperator.Invalid;
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, conditionOp);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			conditionOp = BinarySerializationUtil.ReadEnum<TriggerConditionOperator>(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "CONDITIONOP":
				reader.Read();
				conditionOp = ReaderUtil.ReadEnum<TriggerConditionOperator>(reader);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public abstract bool IsTriggered(ICrossContextCapable gameContext);

		public virtual TransactionDefinition GetDynamicTriggerTransaction(ICrossContextCapable gameContext)
		{
			return null;
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}", GetType(), conditionOp, type);
		}

		protected bool TestOperator<T>(T testingValue, T actualValue) where T : IComparable<T>
		{
			bool result = false;
			switch (conditionOp)
			{
			case TriggerConditionOperator.Equal:
				result = actualValue.CompareTo(testingValue) == 0;
				break;
			case TriggerConditionOperator.NotEqual:
				result = actualValue.CompareTo(testingValue) != 0;
				break;
			case TriggerConditionOperator.Greater:
				result = actualValue.CompareTo(testingValue) > 0;
				break;
			case TriggerConditionOperator.Less:
				result = actualValue.CompareTo(testingValue) < 0;
				break;
			case TriggerConditionOperator.LessEqual:
				result = actualValue.CompareTo(testingValue) <= 0;
				break;
			case TriggerConditionOperator.GreaterEqual:
				result = actualValue.CompareTo(testingValue) >= 0;
				break;
			}
			return result;
		}
	}
}
