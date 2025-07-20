using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public abstract class RushDialogMediator<T> : UIStackMediator<T> where T : RushDialogView
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("RushDialogMediator") as IKampaiLogger;

		protected int rushCost;

		protected IList<QuantityItem> requiredItems;

		protected List<int> requiredItemPremiumCosts;

		protected RushDialogView.RushDialogType dialogType;

		protected PendingCurrencyTransaction pendingCurrencyTransaction;

		protected bool purchaseInProgress;

		protected bool purchaseSuccess;

		protected bool isClosing;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public RushDialogConfirmationSignal confirmedSignal { get; set; }

		[Inject]
		public UpdateStorageItemsSignal updateStorageItemsSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public OpenStorageBuildingSignal openStorageBuildingSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public GoToResourceButtonClickedSignal gotoSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			requiredItems = null;
			openStorageBuildingSignal.AddListener(StorageBuildingOpened);
			base.view.PurchaseButtonView.ClickedSignal.AddListener(PurchaseButtonClicked);
			base.view.UpgradeButton.ClickedSignal.AddListener(PurchaseOrUpgradeButtonClicked);
			base.view.OnMenuClose.AddListener(OnMenuClose);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			openStorageBuildingSignal.RemoveListener(StorageBuildingOpened);
			base.view.PurchaseButtonView.ClickedSignal.RemoveListener(PurchaseButtonClicked);
			base.view.UpgradeButton.ClickedSignal.RemoveListener(PurchaseOrUpgradeButtonClicked);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
		}

		public override void Initialize(GUIArguments args)
		{
			T val = base.view;
			val.Init(localService);
			dialogType = args.Get<RushDialogView.RushDialogType>();
			PendingCurrencyTransaction pendingCurrencyTransaction = (this.pendingCurrencyTransaction = args.Get<PendingCurrencyTransaction>());
			purchaseInProgress = false;
			isClosing = false;
			LoadItems(pendingCurrencyTransaction.GetPendingTransaction(), dialogType);
			SetHeadline(pendingCurrencyTransaction);
		}

		protected virtual void SetHeadline(PendingCurrencyTransaction pct)
		{
		}

		private void StorageBuildingOpened(StorageBuilding storageBuilding, bool directOpen)
		{
			purchaseInProgress = false;
		}

		protected override void Close()
		{
			isClosing = true;
			if (pendingCurrencyTransaction != null)
			{
				Action<PendingCurrencyTransaction> callback = pendingCurrencyTransaction.GetCallback();
				if (callback != null)
				{
					pendingCurrencyTransaction.Success = false;
					callback(pendingCurrencyTransaction);
				}
			}
			if (base.view != null)
			{
				T val = base.view;
				val.Close();
			}
		}

		protected virtual void OnMenuClose()
		{
			if (requiredItems != null)
			{
				SendRushTelemetry(pendingCurrencyTransaction, requiredItems, purchaseSuccess | purchaseInProgress);
			}
			isClosing = true;
			if (dialogType == RushDialogView.RushDialogType.STORAGE_EXPAND)
			{
				hideSkrim.Dispatch("RushStorageSkrim");
				guiService.Execute(GUIOperation.Unload, "popup_OutOfResourceForStorage");
			}
			else
			{
				hideSkrim.Dispatch("RushSkrim");
				guiService.Execute(GUIOperation.Unload, "popup_MissingResources");
			}
		}

		internal virtual RequiredItemView BuildItem(Definition definition, uint itemsInInventory, int itemsLack, bool isAvailable, ILocalizationService localService)
		{
			if (definition == null)
			{
				throw new ArgumentNullException("definition", "RequiredItemBuilder: You are passing in null definitions!");
			}
			GameObject original = KampaiResources.Load("cmp_RequiredItem_ForShow") as GameObject;
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			RequiredItemView component = gameObject.GetComponent<RequiredItemView>();
			ItemDefinition itemDefinition = definition as ItemDefinition;
			Sprite sprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			Sprite maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
			component.ItemIcon.sprite = sprite;
			component.ItemIcon.maskSprite = maskSprite;
			int num = (int)itemsInInventory + itemsLack;
			int num2 = ((itemsLack >= 0) ? itemsLack : 0);
			component.ItemNeeded = num;
			component.ItemQuantity.text = string.Format("{0}/{1}", itemsInInventory, num);
			component.PurchasePanel.SetActive(!isAvailable);
			int num3 = Mathf.FloorToInt(itemDefinition.BasePremiumCost * (float)num2);
			num3 = ((num3 == 0 && num2 > 0) ? 1 : num3);
			component.ItemCost.text = UIUtils.FormatLargeNumber(num3);
			component.ItemDefinitionID = itemDefinition.ID;
			if (!isAvailable)
			{
				QuantityItem item = new QuantityItem(itemDefinition.ID, (uint)num2);
				component.RushBtn.RushCost = num3;
				component.RushBtn.Item = item;
			}
			return component;
		}

		protected virtual void LoadItems(TransactionDefinition transactionDefinition, RushDialogView.RushDialogType type)
		{
			IList<QuantityItem> inputs = transactionDefinition.Inputs;
			if (inputs != null)
			{
				int count = inputs.Count;
				bool showPurchaseButton = false;
				requiredItems = new List<QuantityItem>();
				requiredItemPremiumCosts = new List<int>();
				for (int i = 0; i < count; i++)
				{
					ItemDefinition definition = definitionService.Get<ItemDefinition>(inputs[i].ID);
					QuantityItem quantityItem = null;
					uint quantityByDefinitionId = playerService.GetQuantityByDefinitionId(inputs[i].ID);
					int num = (int)(inputs[i].Quantity - quantityByDefinitionId);
					bool flag = false;
					if (num <= 0)
					{
						flag = true;
					}
					else
					{
						flag = false;
						quantityItem = new QuantityItem(inputs[i].ID, (uint)num);
						requiredItems.Add(quantityItem);
					}
					RequiredItemView requiredItemView = BuildItem(definition, quantityByDefinitionId, num, flag, localService);
					if (!flag)
					{
						requiredItemView.RushBtn.RushButtonClickedSignal.AddListener(IndividualRushButtonClicked);
					}
					int intFromFormattedLargeNumber = UIUtils.GetIntFromFormattedLargeNumber(requiredItemView.ItemCost.text);
					if (intFromFormattedLargeNumber != 0)
					{
						requiredItemPremiumCosts.Add(intFromFormattedLargeNumber);
					}
					T val = base.view;
					val.AddRequiredItem(requiredItemView, i, base.view.ScrollViewParent);
				}
				if (requiredItems.Count != 0)
				{
					rushCost = playerService.CalculateRushCost(requiredItems);
					T val2 = base.view;
					val2.SetupItemCost(rushCost);
					showPurchaseButton = true;
				}
				T val3 = base.view;
				val3.SetupItemCount(count);
				T val4 = base.view;
				val4.SetupDialog(type, showPurchaseButton);
				base.gameObject.SetActive(true);
			}
			else
			{
				logger.Debug("Showing rush dialog without require items");
			}
		}

		protected virtual void PurchaseSuccess()
		{
			purchaseSuccess = true;
			confirmedSignal.Dispatch();
		}

		protected virtual void IndividualPurchaseSuccess()
		{
			updateStorageItemsSignal.Dispatch();
		}

		protected virtual void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			purchaseInProgress = false;
			if (pct.Success)
			{
				PurchaseSuccess();
				soundFXSignal.Dispatch("Play_button_premium_01");
				OnMenuClose();
				setPremiumCurrencySignal.Dispatch();
				setGrindCurrencySignal.Dispatch();
			}
			else if (pct.ParentSuccess)
			{
				PurchaseButtonClicked();
			}
		}

		protected void IndividualRushButtonClicked(int myRushCost, QuantityItem item, bool proceedTransaction)
		{
			if (!purchaseInProgress && !isClosing)
			{
				T val = base.view;
				val.resetAllExceptRequiredItemTapState(item.ID);
				if (proceedTransaction)
				{
					IList<QuantityItem> list = new List<QuantityItem>();
					list.Add(item);
					playerService.ProcessItemPurchase(myRushCost, list, true, IndividualRushTransactionCallback(myRushCost, item));
				}
			}
		}

		protected void PurchaseButtonClicked()
		{
			T val = base.view;
			val.resetAllRequiredItemsTapState();
			if (base.view.PurchaseButtonView.isDoubleConfirmed())
			{
				ExecuteRushTransaction();
			}
		}

		private void PurchaseOrUpgradeButtonClicked()
		{
			ExecuteRushTransaction();
		}

		protected void ExecuteRushTransaction()
		{
			if (!purchaseInProgress && !isClosing)
			{
				if (requiredItems != null)
				{
					purchaseInProgress = true;
					if (requiredItems.Count == 0)
					{
						PurchaseSuccess();
						OnMenuClose();
					}
					else
					{
						bool byPassStorageCheck = SkipStorageCheckOnRushTransaction();
						playerService.ProcessItemPurchase(requiredItemPremiumCosts, requiredItems, true, RushTransactionCallback, byPassStorageCheck);
					}
				}
				else
				{
					logger.Debug("no required items found");
				}
			}
			else
			{
				logger.Debug("Purchase is already in progress");
			}
		}

		protected bool SkipStorageCheckOnRushTransaction()
		{
			return dialogType == RushDialogView.RushDialogType.STORAGE_EXPAND || dialogType == RushDialogView.RushDialogType.BRIDGE_QUEST || dialogType == RushDialogView.RushDialogType.VILLAIN_LAIR_PORTAL_REPAIR || dialogType == RushDialogView.RushDialogType.VILLAIN_LAIR_RESOURCE_PLOT;
		}

		private Action<PendingCurrencyTransaction> IndividualRushTransactionCallback(int myRushCost, QuantityItem item)
		{
			return delegate(PendingCurrencyTransaction pct)
			{
				purchaseInProgress = false;
				if (pct.Success)
				{
					RemovingRequiredItems(item);
					T val = base.view;
					val.DeleteItem(item.ID);
					soundFXSignal.Dispatch("Play_button_premium_01");
					setPremiumCurrencySignal.Dispatch();
					rushCost -= myRushCost;
					if (rushCost == 0)
					{
						PurchaseSuccess();
						OnMenuClose();
					}
					else
					{
						T val2 = base.view;
						val2.SetupItemCost(rushCost);
						IndividualPurchaseSuccess();
					}
				}
			};
		}

		private void RemovingRequiredItems(QuantityItem item)
		{
			foreach (QuantityItem requiredItem in requiredItems)
			{
				if (requiredItem.ID == item.ID)
				{
					requiredItem.Quantity -= item.Quantity;
					if (requiredItem.Quantity == 0)
					{
						requiredItems.Remove(requiredItem);
						break;
					}
				}
			}
		}

		private void SendRushTelemetry(PendingCurrencyTransaction pct, IList<QuantityItem> requiredItems, bool purchaseSuccess)
		{
			string sourceName = "unknown";
			if (dialogType == RushDialogView.RushDialogType.STORAGE_EXPAND)
			{
				sourceName = "StorageExpand";
			}
			telemetryService.Send_Telemetry_EVT_PINCH_PROMPT(sourceName, pct, requiredItems, purchaseSuccess.ToString());
		}

		protected void gotoButtonHandler(int itemDefinitionId)
		{
			Close();
			gotoSignal.Dispatch(itemDefinitionId);
		}
	}
}
