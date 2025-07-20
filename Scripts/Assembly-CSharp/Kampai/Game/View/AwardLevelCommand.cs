using Kampai.Game.Transaction;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class AwardLevelCommand : Command
	{
		[Inject]
		public TransactionDefinition transaction { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public UpdateUIButtonsSignal updateStoreButtonsSignal { get; set; }

		[Inject]
		public CheckResourceBuildingSlotsSignal resourceBuildingSignal { get; set; }

		public override void Execute()
		{
			playerService.RunEntireTransaction(transaction, TransactionTarget.NO_VISUAL, null);
			updateStoreButtonsSignal.Dispatch(true);
			resourceBuildingSignal.Dispatch();
		}
	}
}
