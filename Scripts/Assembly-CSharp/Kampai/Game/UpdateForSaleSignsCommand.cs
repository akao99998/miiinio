using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class UpdateForSaleSignsCommand : Command
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
				if (byInstanceId.IsUnpurchasedAdjacentExpansion(item) && landExpansionService.ShouldUnlockThislevel(item, quantity))
				{
					createSignal.Dispatch(item, true);
				}
			}
		}
	}
}
