using System;
using System.IO;
using Newtonsoft.Json;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class ConsecutiveDaysConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1154;
			}
		}

		public int Days { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.ConsecutiveDays;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Days);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Days = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "DAYS":
				reader.Read();
				Days = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, Seconds: {3}", GetType(), base.conditionOp, type, Days);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			IPlayerService instance = injectionBinder.GetInstance<IPlayerService>();
			return TestOperator(Days, (int)instance.GetQuantity(StaticItem.CONSECUTIVE_DAYS_COUNT));
		}
	}
}
