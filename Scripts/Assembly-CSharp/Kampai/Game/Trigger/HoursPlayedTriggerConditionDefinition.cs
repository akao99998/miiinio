using System;
using System.IO;
using Newtonsoft.Json;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class HoursPlayedTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1158;
			}
		}

		public float Hours { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.HoursPlayed;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Hours);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Hours = reader.ReadSingle();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "HOURS":
				reader.Read();
				Hours = Convert.ToSingle(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, Seconds: {3}", GetType(), base.conditionOp, type, Hours);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			IPlayerDurationService instance = injectionBinder.GetInstance<IPlayerDurationService>();
			return TestOperator(Hours, (float)instance.TotalGamePlaySeconds / 3600f);
		}
	}
}
