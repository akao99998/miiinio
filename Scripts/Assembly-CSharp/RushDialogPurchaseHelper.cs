using System;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using strange.extensions.signal.impl;

public class RushDialogPurchaseHelper
{
	internal Signal actionSuccessfulSignal = new Signal();

	private int transactionId;

	private Action transactionAction;

	private bool waitingForAdditionalGoods;

	private TransactionTarget transactionTarget;

	private TransactionArg transactionArg;

	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public IDefinitionService definitionService { get; set; }

	[Inject]
	public RushDialogConfirmationSignal confirmedSignal { get; set; }

	[Inject]
	public LoadRushDialogSignal loadRushDialogSignal { get; set; }

	[Inject]
	public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

	[Inject]
	public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

	public void Init(int transactionId, TransactionTarget target, TransactionArg args = null)
	{
		this.transactionId = transactionId;
		transactionTarget = target;
		transactionArg = args;
		transactionAction = RunTransaction;
		confirmedSignal.AddListener(ConfirmClicked);
	}

	public void TryAction(bool forceConfirmation = false)
	{
		TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(transactionId);
		if (!playerService.VerifyTransaction(transactionDefinition) || forceConfirmation)
		{
			loadRushDialogSignal.Dispatch(new PendingCurrencyTransaction(transactionDefinition, true, 0, null, null, PurchaseButtonCallback), (transactionTarget == TransactionTarget.STORAGEBUILDING) ? RushDialogView.RushDialogType.STORAGE_EXPAND : RushDialogView.RushDialogType.DEFAULT);
			waitingForAdditionalGoods = true;
		}
		else
		{
			transactionAction();
		}
	}

	private void ConfirmClicked()
	{
		if (waitingForAdditionalGoods)
		{
			transactionAction();
		}
	}

	private void PurchaseButtonCallback(PendingCurrencyTransaction pct)
	{
		if (pct.Success)
		{
			OnTransactionSuccess();
		}
	}

	public void StartTransaction()
	{
		playerService.StartTransaction(transactionId, transactionTarget, PurchaseButtonCallback, transactionArg);
	}

	public void RunTransaction()
	{
		playerService.RunEntireTransaction(transactionId, transactionTarget, PurchaseButtonCallback, transactionArg);
	}

	private void OnTransactionSuccess()
	{
		waitingForAdditionalGoods = false;
		actionSuccessfulSignal.Dispatch();
		setPremiumCurrencySignal.Dispatch();
		setGrindCurrencySignal.Dispatch();
	}

	public void SetTransactionAction(Action transactionAction)
	{
		this.transactionAction = transactionAction;
	}

	public void Detach()
	{
		confirmedSignal.RemoveListener(ConfirmClicked);
	}

	public void Cleanup()
	{
		Detach();
	}
}
