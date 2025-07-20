using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CheckResourceBuildingSlotsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CheckResourceBuildingSlotsCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			foreach (ResourceBuilding item in playerService.GetInstancesByType<ResourceBuilding>())
			{
				if (item.MinionSlotsOwned == 3)
				{
					continue;
				}
				int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
				int num = item.BuildingNumber - 1;
				if (num < 0)
				{
					num = playerService.GetInstancesByDefinitionID(item.Definition.ID).Count;
				}
				IList<SlotUnlock> slotUnlocks = item.Definition.SlotUnlocks;
				if (num >= slotUnlocks.Count)
				{
					logger.Error("CheckResourceBuildingsSlotsCommand: Data issue: Building number {0} is outside of slot unlocks range.", num);
					num = slotUnlocks.Count - 1;
				}
				IList<int> slotUnlockLevels = slotUnlocks[num].SlotUnlockLevels;
				if (slotUnlockLevels[slotUnlockLevels.Count - 1] < quantity)
				{
					continue;
				}
				foreach (int item2 in slotUnlockLevels)
				{
					if (item2 == quantity)
					{
						playerService.PurchaseSlotForBuilding(item.ID, item2);
					}
				}
			}
		}
	}
}
