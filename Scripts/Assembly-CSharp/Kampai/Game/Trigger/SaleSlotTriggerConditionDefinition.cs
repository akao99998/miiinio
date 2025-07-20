using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class SaleSlotTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1171;
			}
		}

		public int slotCount { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.MarketplaceSaleSlot;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(slotCount);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			slotCount = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SLOTCOUNT":
				reader.Read();
				slotCount = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, SlotCount: {3}", GetType(), base.conditionOp, type, slotCount);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IPlayerService instance = gameContext.injectionBinder.GetInstance<IPlayerService>();
			if (instance == null)
			{
				return false;
			}
			ICollection<MarketplaceSaleSlot> byDefinitionId = instance.GetByDefinitionId<MarketplaceSaleSlot>(1000008094);
			return CheckUserMarketplaceSlotCount(byDefinitionId);
		}

		public bool CheckUserMarketplaceSlotCount(ICollection<MarketplaceSaleSlot> slots)
		{
			if (slots == null || slots.Count == 0)
			{
				return false;
			}
			return TestOperator(slotCount, slots.Count);
		}
	}
}
