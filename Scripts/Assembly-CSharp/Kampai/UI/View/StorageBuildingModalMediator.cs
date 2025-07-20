using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class StorageBuildingModalMediator : UIStackMediator<StorageBuildingModalView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("StorageBuildingModalMediator") as IKampaiLogger;

		private int currentUpgradeTransactionId;

		private int currentStorageBuildingId;

		private bool marketplaceUnlocked;

		private StorageBuildingModalTypes currentMode;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public UpdateStorageItemsSignal updateStorageItemsSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllMenuSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public RushDialogPurchaseHelper rushDialogPurchaseHelper { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal stateChangeSignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal storageCapacitySignal { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public EnableStorageBuildingItemDescriptionSignal enableItemDescriptionSignal { get; set; }

		[Inject]
		public UpdateSaleSlotsStateSignal updateSaleSlotsStateSignal { get; set; }

		[Inject]
		public MarketplaceOpenSalePanelSignal openSalePanelSignal { get; set; }

		[Inject]
		public MarketplaceOpenBuyPanelSignal openBuyPanelSignal { get; set; }

		[Inject]
		public MarketplaceCloseSalePanelSignal closeSalePanelSignal { get; set; }

		[Inject]
		public MarketplaceCloseBuyPanelSignal closeBuyPanel { get; set; }

		[Inject]
		public MarketplaceCloseAllSalePanels closeAllSalePanels { get; set; }

		[Inject]
		public RemoveFloatingTextSignal removeFloatingTextSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistance { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public GenerateBuyItemsSignal generateBuyItemsSignal { get; set; }

		[Inject]
		public RemoveStorageBuildingItemDescriptionSignal removeItemDescriptionSignal { get; set; }

		[Inject]
		public SelectStorageBuildingItemSignal selectStorageBuildingItemSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeOtherMenusSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal sfxSignal { get; set; }

		[Inject]
		public GoToResourceButtonClickedSignal gotoSignal { get; set; }

		[Inject]
		public IGoToService gotoService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.Init();
			base.view.UpgradeButtonView.ClickedSignal.AddListener(UpgradeButtonClicked);
			base.view.SellButtonView.ClickedSignal.AddListener(OnSellPanelClick);
			base.view.BuyButtonView.ClickedSignal.AddListener(OnBuyPanelClick);
			base.view.ScrollListButtonView.ClickedSignal.AddListener(CloseSalePanel);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			closeSalePanelSignal.AddListener(OnSellPanelClosed);
			closeBuyPanel.AddListener(OnBuyPanelClosed);
			updateStorageItemsSignal.AddListener(UpdateItems);
			closeAllMenuSignal.AddListener(CloseDialog);
			rushDialogPurchaseHelper.actionSuccessfulSignal.AddListener(OnTransactionSuccess);
			marketplaceUnlocked = marketplaceService.IsUnlocked();
			UpdateSellButton();
			base.view.InfoLabel.gameObject.SetActive(!marketplaceUnlocked);
			if (!marketplaceUnlocked)
			{
				MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
				base.view.InfoLabel.text = localService.GetString("MarketplaceUnlock", marketplaceDefinition.LevelGate);
			}
			StorageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StorageBuilding>(3018);
			if (firstInstanceByDefinitionId != null)
			{
				firstInstanceByDefinitionId.MenuOpened = true;
				firstInstanceByDefinitionId.MenuOpening = false;
			}
			gotoSignal.AddListener(GotoResourceBuilding);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.UpgradeButtonView.ClickedSignal.RemoveListener(UpgradeButtonClicked);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.SellButtonView.ClickedSignal.RemoveListener(OnSellPanelClick);
			base.view.BuyButtonView.ClickedSignal.RemoveListener(OnBuyPanelClick);
			base.view.ScrollListButtonView.ClickedSignal.RemoveListener(CloseSalePanel);
			closeBuyPanel.RemoveListener(OnBuyPanelClosed);
			closeSalePanelSignal.RemoveListener(OnSellPanelClosed);
			updateStorageItemsSignal.RemoveListener(UpdateItems);
			closeAllMenuSignal.RemoveListener(CloseDialog);
			rushDialogPurchaseHelper.actionSuccessfulSignal.RemoveListener(OnTransactionSuccess);
			rushDialogPurchaseHelper.Cleanup();
			StorageBuilding byInstanceId = playerService.GetByInstanceId<StorageBuilding>(currentStorageBuildingId);
			if (byInstanceId != null)
			{
				byInstanceId.MenuOpened = false;
			}
			gotoSignal.RemoveListener(GotoResourceBuilding);
		}

		public override void Initialize(GUIArguments args)
		{
			StorageBuilding building = args.Get<StorageBuilding>();
			soundFXSignal.Dispatch("Play_menu_popUp_01");
			CheckToGenerateBuyItems();
			currentMode = args.Get<StorageBuildingModalTypes>();
			switch (currentMode)
			{
			case StorageBuildingModalTypes.STORAGE:
				LoadItems(building);
				break;
			case StorageBuildingModalTypes.BUY:
				OpenBuyPanel(true);
				break;
			case StorageBuildingModalTypes.SELL:
				OpenSellPanel(true);
				break;
			}
			CheckForMarketplaceSurfacing();
			closeOtherMenusSignal.Dispatch(base.gameObject);
		}

		private void CheckToGenerateBuyItems()
		{
			List<MarketplaceBuyItem> instancesByType = playerService.GetInstancesByType<MarketplaceBuyItem>();
			if (marketplaceUnlocked && instancesByType != null && instancesByType.Count == 0)
			{
				generateBuyItemsSignal.Dispatch();
			}
		}

		internal void UpdateSellButton()
		{
			base.view.EnableMarketplace(marketplaceUnlocked);
		}

		internal void UpdateItems()
		{
			removeItemDescriptionSignal.Dispatch();
			StorageBuilding byInstanceId = playerService.GetByInstanceId<StorageBuilding>(currentStorageBuildingId);
			LoadItems(byInstanceId);
			base.view.RearrangeItemView(true);
			if (currentMode == StorageBuildingModalTypes.SELL)
			{
				StartCoroutine(EnableItemDescriptionPopupDelay());
			}
		}

		private void OnTransactionSuccess()
		{
			UpdateItems();
			soundFXSignal.Dispatch("Play_expand_storage_01");
			storageCapacitySignal.Dispatch();
			StorageBuilding byInstanceId = playerService.GetByInstanceId<StorageBuilding>(currentStorageBuildingId);
			if (byInstanceId.CurrentStorageBuildingLevel == byInstanceId.Definition.StorageUpgrades.Count - 1)
			{
				string @string = localizationService.GetString("MaxStorageExpansionReached");
				popupMessageSignal.Dispatch(@string, PopupMessageType.NORMAL);
			}
		}

		internal void UpgradeButtonClicked()
		{
			rushDialogPurchaseHelper.TryAction(true);
		}

		internal void CloseDialog(GameObject sender)
		{
			if (sender != base.gameObject)
			{
				Close();
			}
		}

		protected override void Close()
		{
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			CloseView();
		}

		private void CloseView()
		{
			base.view.Close();
			removeItemDescriptionSignal.Dispatch();
		}

		private void OnMenuClose()
		{
			stateChangeSignal.Dispatch(currentStorageBuildingId, BuildingState.Idle);
			hideSignal.Dispatch("StorageSkrim");
			guiService.Execute(GUIOperation.Unload, "screen_StorageBuilding");
		}

		internal void LoadItems(StorageBuilding building)
		{
			removeItemDescriptionSignal.Dispatch();
			base.view.scrollView.ClearItems();
			uint totalStorableQuantity = 0u;
			StorageBuildingModalTypes storageBuildingModalTypes = currentMode;
			ICollection<Item> collection;
			if (storageBuildingModalTypes == StorageBuildingModalTypes.SELL)
			{
				uint totalSellableQuantity = 0u;
				collection = playerService.GetSellableItems(out totalStorableQuantity, out totalSellableQuantity);
			}
			else
			{
				collection = playerService.GetStorableItems(out totalStorableQuantity);
			}
			uint currentStorageCapacity = playerService.GetCurrentStorageCapacity();
			currentStorageBuildingId = building.ID;
			if (building.CurrentStorageBuildingLevel == building.Definition.StorageUpgrades.Count - 1)
			{
				base.view.DisableExpandButton();
			}
			currentUpgradeTransactionId = building.Definition.StorageUpgrades[building.CurrentStorageBuildingLevel].TransactionId;
			rushDialogPurchaseHelper.Init(currentUpgradeTransactionId, TransactionTarget.STORAGEBUILDING, new TransactionArg(currentStorageBuildingId));
			base.view.SetCap((int)currentStorageCapacity);
			base.view.SetCurrentItemCount((int)totalStorableQuantity);
			if (totalStorableQuantity >= currentStorageCapacity)
			{
				base.view.UpdateStorageStatus(true);
			}
			else
			{
				base.view.UpdateStorageStatus(false);
			}
			if (collection != null)
			{
				foreach (Item item in collection)
				{
					StorageBuildingItemView slotView = StorageBuildingItemBuilder.Build(item, item.Definition, (int)item.Quantity, logger);
					base.view.scrollView.AddItem(slotView);
				}
				base.view.scrollView.SetupScrollView();
			}
			updateSaleSlotsStateSignal.Dispatch();
		}

		private void OnSellPanelClick()
		{
			base.view.HighlightSellButton(false);
			OpenSellPanel();
		}

		private void OpenSellPanel(bool isInstant = false)
		{
			if (base.view.SellPanel == null)
			{
				if (base.view.SellGrayImage.gameObject.activeSelf)
				{
					OnMarketplaceDisableClicked();
					return;
				}
				base.view.LoadSellMarketplacePanel();
			}
			if (base.view.BuyPanel != null && base.view.BuyPanel.IsOpen)
			{
				base.view.BuyPanel.SetOpen(false);
			}
			currentMode = StorageBuildingModalTypes.SELL;
			StorageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StorageBuilding>(3018);
			if (firstInstanceByDefinitionId != null)
			{
				LoadItems(firstInstanceByDefinitionId);
			}
			StartCoroutine(EnableItemDescriptionPopupDelay());
			if (base.view.SellPanel != null)
			{
				openSalePanelSignal.Dispatch(isInstant);
				base.view.SellButtonView.gameObject.SetActive(false);
				base.view.RearrangeItemView();
				CheckForMarketplaceSurfacing();
			}
			selectStorageBuildingItemSignal.Dispatch(0);
		}

		private IEnumerator EnableItemDescriptionPopupDelay()
		{
			yield return new WaitForEndOfFrame();
			enableItemDescriptionSignal.Dispatch(true);
		}

		private void OnSellPanelClosed()
		{
			base.view.SellButtonView.gameObject.SetActive(true);
			enableItemDescriptionSignal.Dispatch(false);
			currentMode = StorageBuildingModalTypes.STORAGE;
			UpdateItems();
		}

		private void OnBuyPanelClick()
		{
			base.view.HighlightBuyButton(false);
			OpenBuyPanel();
		}

		private void OpenBuyPanel(bool isInstant = false)
		{
			if (base.view.BuyPanel == null)
			{
				if (base.view.BuyGrayImage.gameObject.activeSelf)
				{
					OnMarketplaceDisableClicked();
					return;
				}
				base.view.LoadBuyMarketplacePanel();
			}
			if (base.view.BuyPanel != null)
			{
				openBuyPanelSignal.Dispatch(isInstant);
				base.view.BuyButtonView.gameObject.SetActive(false);
				base.view.SellButtonView.gameObject.SetActive(true);
				CheckForMarketplaceSurfacing();
			}
			if (!(base.view.SellPanel == null) && base.view.SellPanel.isOpen)
			{
				currentMode = StorageBuildingModalTypes.BUY;
				selectStorageBuildingItemSignal.Dispatch(0);
				closeAllSalePanels.Dispatch();
			}
		}

		private void OnBuyPanelClosed()
		{
			base.view.BuyButtonView.gameObject.SetActive(true);
			currentMode = StorageBuildingModalTypes.STORAGE;
			UpdateItems();
		}

		private void CloseSalePanel()
		{
			if (!(base.view.SellPanel == null) && base.view.SellPanel.isOpen)
			{
				closeAllSalePanels.Dispatch();
			}
		}

		private void CheckForMarketplaceSurfacing()
		{
			if (localPersistance.HasKeyPlayer("MarketSurfacing"))
			{
				localPersistance.DeleteKeyPlayer("MarketSurfacing");
				if (!localPersistance.HasKeyPlayer("MarketSurfacingButtonPulse"))
				{
					localPersistance.PutDataPlayer("MarketSurfacingButtonPulse", bool.FalseString);
					base.view.HighlightSellButton(true);
					base.view.HighlightBuyButton(true);
				}
				removeFloatingTextSignal.Dispatch(currentStorageBuildingId);
			}
		}

		private void OnMarketplaceDisableClicked()
		{
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			if (playerService.GetQuantity(StaticItem.LEVEL_ID) < marketplaceDefinition.LevelGate)
			{
				string @string = localService.GetString("MarketplaceUnlock", marketplaceDefinition.LevelGate);
				popupMessageSignal.Dispatch(@string, PopupMessageType.NORMAL);
				sfxSignal.Dispatch("Play_action_locked_01");
			}
		}

		private void GotoResourceBuilding(int itemDefinitionId)
		{
			Close();
			gotoService.GoToBuildingFromItem(itemDefinitionId);
		}
	}
}
