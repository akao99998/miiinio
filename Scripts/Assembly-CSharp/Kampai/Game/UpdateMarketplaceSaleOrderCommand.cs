using System.Collections.Generic;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class UpdateMarketplaceSaleOrderCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public UpdateSaleSlotSignal updateSaleSlot { get; set; }

		public override void Execute()
		{
			List<MarketplaceSaleItem> instancesByType = playerService.GetInstancesByType<MarketplaceSaleItem>();
			instancesByType.Sort();
			List<MarketplaceSaleSlot> instancesByType2 = playerService.GetInstancesByType<MarketplaceSaleSlot>();
			for (int i = 0; i < instancesByType2.Count; i++)
			{
				if (i < instancesByType.Count)
				{
					instancesByType2[i].itemId = instancesByType[i].ID;
				}
				else
				{
					instancesByType2[i].itemId = 0;
				}
				updateSaleSlot.Dispatch(instancesByType2[i].ID);
			}
		}
	}
}
