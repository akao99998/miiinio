using System;
using System.IO;
using Kampai.Game.Transaction;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class OrderBoardTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1161;
			}
		}

		public int duration { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.OrderBoard;
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

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IOrderBoardService instance = gameContext.injectionBinder.GetInstance<IOrderBoardService>();
			if (instance == null)
			{
				return false;
			}
			int longestIdleOrderDuration = instance.GetLongestIdleOrderDuration();
			return TestOperator(duration, longestIdleOrderDuration);
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, duration: {3}", GetType(), base.conditionOp, type, duration);
		}

		public override TransactionDefinition GetDynamicTriggerTransaction(ICrossContextCapable gameContext)
		{
			IOrderBoardService instance = gameContext.injectionBinder.GetInstance<IOrderBoardService>();
			if (instance == null)
			{
				return null;
			}
			return instance.GetLongestIdleOrderTransaction();
		}
	}
}
