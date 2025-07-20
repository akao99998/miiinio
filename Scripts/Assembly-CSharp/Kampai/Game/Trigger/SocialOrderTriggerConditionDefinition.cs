using System;
using System.IO;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class SocialOrderTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1174;
			}
		}

		public int PercentComplete { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.SocialOrder;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(PercentComplete);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			PercentComplete = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PERCENTCOMPLETE":
				reader.Read();
				PercentComplete = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			ITimedSocialEventService instance = injectionBinder.GetInstance<ITimedSocialEventService>();
			IKampaiLogger kampaiLogger = LogManager.GetClassLogger("SocialOrderTriggerConditionDefinition") as IKampaiLogger;
			TimedSocialEventDefinition currentSocialEvent = instance.GetCurrentSocialEvent();
			if (currentSocialEvent == null || currentSocialEvent.Orders == null || currentSocialEvent.Orders.Count == 0)
			{
				kampaiLogger.Info("No social order available.");
				return false;
			}
			SocialTeamResponse socialEventStateCached = instance.GetSocialEventStateCached(currentSocialEvent.ID);
			if (socialEventStateCached == null || socialEventStateCached.Team == null)
			{
				kampaiLogger.Info("No social team available.");
				return false;
			}
			return TestOperator(PercentComplete, CalculateCompletion(currentSocialEvent, socialEventStateCached));
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, PercentComplete: {3}", GetType(), base.conditionOp, type, PercentComplete);
		}

		private int CalculateCompletion(TimedSocialEventDefinition current, SocialTeamResponse team)
		{
			float num = 0f;
			foreach (SocialEventOrderDefinition order in current.Orders)
			{
				string value = null;
				foreach (SocialOrderProgress item in team.Team.OrderProgress)
				{
					if (item.OrderId == order.OrderID)
					{
						value = item.CompletedByUserId;
						break;
					}
				}
				if (!string.IsNullOrEmpty(value))
				{
					num += 1f;
				}
			}
			return (int)(num / (float)current.Orders.Count * 100f);
		}
	}
}
