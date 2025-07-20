using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game.Mtx
{
	internal sealed class ProcessNextPendingTransactionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ProcessNextPendingTransactionCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		public override void Execute()
		{
			IList<KampaiPendingTransaction> pendingTransactions = playerService.GetPendingTransactions();
			if (pendingTransactions.Count > 0)
			{
				logger.Debug("ProcessNextPendingTransactionCommand: A pending transaction found, repurchasing");
				currencyService.RequestPurchase(pendingTransactions[0]);
			}
		}
	}
}
