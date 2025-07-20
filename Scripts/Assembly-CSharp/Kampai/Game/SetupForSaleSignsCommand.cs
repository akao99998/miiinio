using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetupForSaleSignsCommand : Command
	{
		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CreateForSaleSignSignal createSignal { get; set; }

		public override void Execute()
		{
			List<int> list = landExpansionService.GetAllExpansionIDs() as List<int>;
			PurchasedLandExpansion byInstanceId = playerService.GetByInstanceId<PurchasedLandExpansion>(354);
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			foreach (int item in list)
			{
				bool type = false;
				if (byInstanceId.IsUnpurchasedAdjacentExpansion(item) && !landExpansionService.IsLevelGated(item, quantity))
				{
					type = true;
				}
				createSignal.Dispatch(item, type);
			}
		}
	}
}
