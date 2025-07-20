using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreateBuildingInInventoryCommand : Command
	{
		[Inject]
		public int transactionId { get; set; }

		[Inject]
		public SendBuildingToInventorySignal sendBuildingToInventorySignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			Location value = new Location();
			TransactionArg transactionArg = new TransactionArg();
			transactionArg.Add(value);
			IList<Instance> outputs = null;
			if (!playerService.FinishTransaction(transactionId, TransactionTarget.REWARD_BUILDING, out outputs, transactionArg))
			{
				return;
			}
			foreach (Instance item in outputs)
			{
				if (item is Building)
				{
					buildingChangeStateSignal.Dispatch(item.ID, BuildingState.Inventory);
					sendBuildingToInventorySignal.Dispatch(item.ID);
					break;
				}
			}
		}
	}
}
