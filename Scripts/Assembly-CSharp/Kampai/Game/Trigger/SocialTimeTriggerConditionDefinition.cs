using System;
using System.IO;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class SocialTimeTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1175;
			}
		}

		public int Seconds { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.SocialTime;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Seconds);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Seconds = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SECONDS":
				reader.Read();
				Seconds = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, Seconds: {3}", GetType(), base.conditionOp, type, Seconds);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			ITimedSocialEventService instance = injectionBinder.GetInstance<ITimedSocialEventService>();
			IKampaiLogger kampaiLogger = LogManager.GetClassLogger("SocialTimeTriggerConditionDefinition") as IKampaiLogger;
			ITimeService instance2 = injectionBinder.GetInstance<ITimeService>();
			TimedSocialEventDefinition currentSocialEvent = instance.GetCurrentSocialEvent();
			if (currentSocialEvent == null)
			{
				kampaiLogger.Info("No social order available.");
				return false;
			}
			long num = instance.GetCurrentSocialEvent().FinishTime;
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(num).ToLocalTime();
			DateTime dateTime2 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(instance2.CurrentTime()).ToLocalTime();
			TimeSpan timeSpan = dateTime.Subtract(dateTime2);
			if (dateTime2 >= dateTime)
			{
				kampaiLogger.Info("Event is already over.");
				return false;
			}
			return TestOperator(Seconds, timeSpan.TotalSeconds);
		}
	}
}
