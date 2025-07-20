using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class SaleItemTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1170;
			}
		}

		public int remainingTime { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.MarketplaceSaleItem;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(remainingTime);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			remainingTime = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "REMAININGTIME":
				reader.Read();
				remainingTime = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IPlayerService instance = gameContext.injectionBinder.GetInstance<IPlayerService>();
			if (instance == null)
			{
				return false;
			}
			PlayerService playerService = instance as PlayerService;
			if (playerService == null || playerService.timeService == null)
			{
				return false;
			}
			ICollection<MarketplaceSaleItem> byDefinitionId = playerService.GetByDefinitionId<MarketplaceSaleItem>(1000008094);
			return CheckUserMarketplaceItems(playerService.timeService, byDefinitionId);
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, remainingTime: {3}", GetType(), base.conditionOp, type, remainingTime);
		}

		public bool CheckUserMarketplaceItems(ITimeService timeService, ICollection<MarketplaceSaleItem> items)
		{
			if (items == null || items.Count == 0)
			{
				return false;
			}
			int saleRemaingTime = int.MaxValue;
			MarketplaceSaleItem nextForSaleItem = null;
			saleRemaingTime = GetClosestSaleItem(timeService, items, saleRemaingTime, ref nextForSaleItem);
			if (saleRemaingTime == int.MaxValue)
			{
				return false;
			}
			return TestOperator(remainingTime, saleRemaingTime);
		}

		public static int GetClosestSaleItem(ITimeService timeService, ICollection<MarketplaceSaleItem> items, int saleRemaingTime, ref MarketplaceSaleItem nextForSaleItem)
		{
			int num = timeService.CurrentTime();
			foreach (MarketplaceSaleItem item in items)
			{
				if (item != null)
				{
					int num2 = item.LengthOfSale + item.SaleStartTime - num;
					saleRemaingTime = Mathf.Min(saleRemaingTime, num2);
					if (saleRemaingTime == num2)
					{
						nextForSaleItem = item;
					}
				}
			}
			return saleRemaingTime;
		}
	}
}
