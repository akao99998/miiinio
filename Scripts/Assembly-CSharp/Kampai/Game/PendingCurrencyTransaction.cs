using System;
using System.Collections.Generic;
using Kampai.Game.Transaction;
using Kampai.Util;

namespace Kampai.Game
{
	public class PendingCurrencyTransaction
	{
		private TransactionDefinition pendingTransaction;

		private int rushCost;

		private bool rushing;

		private IList<QuantityItem> rushOutputItems;

		private IList<Instance> outputs;

		private Action<PendingCurrencyTransaction> callback;

		private TransactionTarget target;

		private TransactionArg transactionArg;

		public CurrencyTransactionFailReason FailReason;

		public bool Success { get; set; }

		public bool ParentSuccess { get; set; }

		public PendingCurrencyTransaction(TransactionDefinition pendingTransaction, bool isRush, int rushCost, IList<QuantityItem> rushOutputItems, IList<Instance> outputs, Action<PendingCurrencyTransaction> callback = null, TransactionTarget target = TransactionTarget.NO_VISUAL, TransactionArg transactionArg = null)
		{
			this.target = target;
			this.pendingTransaction = pendingTransaction;
			rushing = isRush;
			this.rushCost = rushCost;
			this.rushOutputItems = rushOutputItems;
			this.outputs = outputs;
			this.callback = callback;
			this.transactionArg = transactionArg;
		}

		public TransactionArg GetTransactionArg()
		{
			return transactionArg;
		}

		public TransactionTarget GetTransactionTarget()
		{
			return target;
		}

		public IList<QuantityItem> GetInputs()
		{
			return pendingTransaction.Inputs;
		}

		public TransactionDefinition GetPendingTransaction()
		{
			return pendingTransaction;
		}

		public bool IsRushing()
		{
			return rushing;
		}

		public int GetRushCost()
		{
			return rushCost;
		}

		public IList<QuantityItem> GetRushOutputItems()
		{
			return rushOutputItems;
		}

		public IList<Instance> GetOutputs()
		{
			return outputs;
		}

		public Action<PendingCurrencyTransaction> GetCallback()
		{
			return callback;
		}
	}
}
