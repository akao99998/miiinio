using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class OrderBoardDeleteOrderCommand : Command
	{
		[Inject]
		public OrderBoard building { get; set; }

		[Inject]
		public int TicketIndex { get; set; }

		[Inject]
		public TransactionDefinition def { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public OrderBoardStartRefillTicketSignal startRefillTicketSignal { get; set; }

		[Inject]
		public OrderBoardRefillTicketSignal refillTicketSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			int startTime = timeService.CurrentTime();
			DeleteTicket(startTime);
		}

		private void DeleteTicket(int startTime)
		{
			foreach (OrderBoardTicket ticket in building.tickets)
			{
				if (ticket.BoardIndex == TicketIndex)
				{
					ticket.StartTime = startTime;
					break;
				}
			}
			int refillTime = building.Definition.RefillTime;
			timeEventService.AddEvent(-TicketIndex, timeService.CurrentTime(), refillTime, refillTicketSignal);
			startRefillTicketSignal.Dispatch(new Tuple<int, int, float>(TicketIndex, refillTime, building.Definition.TicketRepopTime));
			OrderBoardTicket orderBoardTicket = building.tickets[TicketIndex];
			telemetryService.Send_TelemetryOrderBoard(false, def, orderBoardTicket.CharacterDefinitionId);
		}
	}
}
