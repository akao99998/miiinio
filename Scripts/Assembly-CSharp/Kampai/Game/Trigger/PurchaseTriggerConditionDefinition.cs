using System;
using System.IO;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class PurchaseTriggerConditionDefinition : TriggerConditionDefinition
	{
		public enum PurchaseTriggerMode
		{
			GAME_SECONDS = 0,
			CAL_SECONDS = 1,
			TRANSACTIONS = 2,
			SKU = 3,
			USD = 4
		}

		public override int TypeCode
		{
			get
			{
				return 1167;
			}
		}

		public PurchaseTriggerMode mode { get; set; }

		public uint quantity { get; set; }

		public string sku { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.Purchase;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, mode);
			writer.Write(quantity);
			BinarySerializationUtil.WriteString(writer, sku);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			mode = BinarySerializationUtil.ReadEnum<PurchaseTriggerMode>(reader);
			quantity = reader.ReadUInt32();
			sku = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MODE":
				reader.Read();
				mode = ReaderUtil.ReadEnum<PurchaseTriggerMode>(reader);
				break;
			case "QUANTITY":
				reader.Read();
				quantity = Convert.ToUInt32(reader.Value);
				break;
			case "SKU":
				reader.Read();
				sku = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, mode: {3}, quantity: {4}, sku: {5}", GetType(), base.conditionOp, type, mode, quantity, sku);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IInjectionBinder injectionBinder = gameContext.injectionBinder;
			IPlayerService instance = injectionBinder.GetInstance<IPlayerService>();
			if (instance == null)
			{
				return false;
			}
			switch (mode)
			{
			case PurchaseTriggerMode.CAL_SECONDS:
			{
				ITimeService instance3 = injectionBinder.GetInstance<ITimeService>();
				return TestOperator(quantity, instance3.CurrentTime() - instance.GetQuantity(StaticItem.LAST_CAL_TIME_PURCHASE));
			}
			case PurchaseTriggerMode.GAME_SECONDS:
			{
				IPlayerDurationService instance2 = injectionBinder.GetInstance<IPlayerDurationService>();
				return TestOperator(quantity, instance2.TotalGamePlaySeconds - instance.GetQuantity(StaticItem.LAST_GAME_TIME_PURCHASE));
			}
			case PurchaseTriggerMode.TRANSACTIONS:
				return TestOperator(quantity, instance.GetQuantity(StaticItem.TRANSACTIONS_LIFETIME_COUNT_ID));
			case PurchaseTriggerMode.SKU:
				return TestOperator(quantity, Convert.ToUInt32(instance.MTXPurchaseCount(sku)));
			case PurchaseTriggerMode.USD:
				return false;
			default:
			{
				IKampaiLogger kampaiLogger = LogManager.GetClassLogger("PurchaseTriggerConditionDefinition") as IKampaiLogger;
				kampaiLogger.Fatal(FatalCode.TR_INVALID_PURCHASE_MODE, (int)mode);
				return false;
			}
			}
		}
	}
}
