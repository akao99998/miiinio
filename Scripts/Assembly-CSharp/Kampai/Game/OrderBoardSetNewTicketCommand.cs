using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class OrderBoardSetNewTicketCommand : Command
	{
		[Inject]
		public int inverseTicketIndex { get; set; }

		[Inject]
		public bool makeItSelected { get; set; }

		[Inject]
		public IOrderBoardService orderBoardService { get; set; }

		[Inject]
		public OrderBoardFillOrderCompleteSignal fillOrderCompleteSignal { get; set; }

		public override void Execute()
		{
			orderBoardService.GetNewTicket(-inverseTicketIndex);
			if (makeItSelected)
			{
				fillOrderCompleteSignal.Dispatch(-inverseTicketIndex);
			}
		}
	}
}
