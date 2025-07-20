using System;
using System.IO;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class SocialOrderTriggerRewardDefinition : TriggerRewardDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1184;
			}
		}

		public int OrderId { get; set; }

		public override TriggerRewardType.Identifier type
		{
			get
			{
				return TriggerRewardType.Identifier.SocialOrder;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(OrderId);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			OrderId = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ORDERID":
				reader.Read();
				OrderId = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override void RewardPlayer(ICrossContextCapable context)
		{
			ICrossContextInjectionBinder injectionBinder = context.injectionBinder;
			ITimedSocialEventService instance = injectionBinder.GetInstance<ITimedSocialEventService>();
			IKampaiLogger kampaiLogger = LogManager.GetClassLogger("SocialOrderTriggerRewardDefinition") as IKampaiLogger;
			TimedSocialEventDefinition currentSocialEvent = instance.GetCurrentSocialEvent();
			if (currentSocialEvent == null)
			{
				kampaiLogger.Error("No social order available.");
				return;
			}
			SocialTeamResponse socialEventStateCached = instance.GetSocialEventStateCached(currentSocialEvent.ID);
			if (socialEventStateCached == null || socialEventStateCached.Team == null)
			{
				kampaiLogger.Error("No social team available.");
			}
			else
			{
				foreach (SocialEventOrderDefinition order in currentSocialEvent.Orders)
				{
					string value = null;
					foreach (SocialOrderProgress item in socialEventStateCached.Team.OrderProgress)
					{
						if (item.OrderId == order.OrderID)
						{
							value = item.CompletedByUserId;
							break;
						}
					}
					if (string.IsNullOrEmpty(value) && (order.OrderID == OrderId || OrderId < 1))
					{
						TransactionDefinition transactionDefinition = injectionBinder.GetInstance<IDefinitionService>().Get<TransactionDefinition>(order.Transaction);
						injectionBinder.GetInstance<IPlayerService>().GrantInputs(transactionDefinition);
						injectionBinder.GetInstance<ShowSocialPartyFillOrderSignal>().Dispatch(order.OrderID);
						return;
					}
				}
			}
			kampaiLogger.Error("No such order: {0}", OrderId);
		}
	}
}
