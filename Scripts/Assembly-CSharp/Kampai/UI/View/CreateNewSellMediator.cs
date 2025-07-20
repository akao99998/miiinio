using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class CreateNewSellMediator : Mediator
	{
		private int m_slotId;

		public IKampaiLogger logger = LogManager.GetClassLogger("CreateNewSellMediator") as IKampaiLogger;

		private bool m_isSalePanelOpened;

		[Inject]
		public CreateNewSellView view { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public SetNewSellItemSignal setNewSellItemSignal { get; set; }

		[Inject]
		public OpenCreateNewSalePanelSignal openSalePanelSignal { get; set; }

		[Inject]
		public CloseCreateNewSalePanelSignal CloseCreateNewSalePanelSignal { get; set; }

		[Inject]
		public MarketplaceOpenSalePanelSignal openSaleSignal { get; set; }

		[Inject]
		public MarketplaceCloseSalePanelSignal closeSaleSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public SelectStorageBuildingItemSignal selectStorageBuildingItemSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displaySignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			FillInColors();
			view.Init();
			closeSaleSignal.AddListener(SalePanelClosed);
			openSaleSignal.AddListener(SalePanelOpened);
			openSalePanelSignal.AddListener(OpenPanel);
			setNewSellItemSignal.AddListener(SetInitialItem);
			view.PutOnSaleButton.ClickedSignal.AddListener(CreateSaleOnClick);
			view.AddItemCountButton.heldDownSignal.AddListener(OnAddItemCountClick);
			view.MinusItemCountButton.heldDownSignal.AddListener(OnMinusItemCountClick);
			view.AddPriceButton.heldDownSignal.AddListener(OnAddPriceClick);
			view.MinusPriceButton.heldDownSignal.AddListener(OnMinusPriceClick);
			view.ResetPriceButton.ClickedSignal.AddListener(OnPriceResetClick);
			view.gameObject.SetActive(false);
			SalePanelOpened(true);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			closeSaleSignal.RemoveListener(SalePanelClosed);
			openSaleSignal.RemoveListener(SalePanelOpened);
			openSalePanelSignal.RemoveListener(OpenPanel);
			setNewSellItemSignal.RemoveListener(SetInitialItem);
			view.PutOnSaleButton.ClickedSignal.RemoveListener(CreateSaleOnClick);
			view.AddItemCountButton.heldDownSignal.RemoveListener(OnAddItemCountClick);
			view.MinusItemCountButton.heldDownSignal.RemoveListener(OnMinusItemCountClick);
			view.AddPriceButton.heldDownSignal.RemoveListener(OnAddPriceClick);
			view.MinusPriceButton.heldDownSignal.RemoveListener(OnMinusPriceClick);
			view.ResetPriceButton.ClickedSignal.RemoveListener(OnPriceResetClick);
		}

		private void FillInColors()
		{
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			if (marketplaceDefinition != null)
			{
				view.sellPriceUpBackgroundColor = marketplaceDefinition.SellPriceUpBackgroundColor.GetColor();
				view.sellPriceUpTextColor = marketplaceDefinition.SellPriceUpTextColor.GetColor();
				view.sellPriceDownBackgroundColor = marketplaceDefinition.SellPriceDownBackgroundColor.GetColor();
				view.sellPriceDownTextColor = marketplaceDefinition.SellPriceDownTextColor.GetColor();
			}
		}

		private bool GetFirstItemInStorage()
		{
			foreach (Item sellableItem in playerService.GetSellableItems())
			{
				DynamicIngredientsDefinition dynamicIngredientsDefinition = sellableItem.Definition as DynamicIngredientsDefinition;
				if (dynamicIngredientsDefinition == null)
				{
					SetInitialItem(sellableItem.Definition.ID);
					if (view.marketplaceDef != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		private Item GetItem(int id)
		{
			Item result = null;
			foreach (Item sellableItem in playerService.GetSellableItems())
			{
				if (sellableItem.Definition.ID != id)
				{
					continue;
				}
				result = sellableItem;
				break;
			}
			return result;
		}

		private MarketplaceItemDefinition GetMarketplaceItemDef(int itemId)
		{
			MarketplaceItemDefinition itemDefinition;
			marketplaceService.GetItemDefinitionByItemID(itemId, out itemDefinition);
			return itemDefinition;
		}

		private bool HasOpenSlot()
		{
			return marketplaceService.GetNextAvailableSlot() != null;
		}

		private void OnPriceResetClick()
		{
			SetItem(view.item, true);
		}

		private void SetInitialItem(int id)
		{
			if (view == null)
			{
				return;
			}
			if (id <= 0)
			{
				CloseCreateNewSalePanelSignal.Dispatch();
				return;
			}
			Item item = null;
			if (m_isSalePanelOpened && HasOpenSlot())
			{
				item = GetItem(id);
				SetItem(item, false);
				openSalePanelSignal.Dispatch(id);
			}
			else
			{
				item = GetItem(id);
				SetItem(item, false);
			}
		}

		private void SetItem(Item item, bool reset)
		{
			if (item == null)
			{
				logger.Warning("Item is null when trying to set the item for the create new sale panel.");
				return;
			}
			MarketplaceItemDefinition marketplaceItemDef = GetMarketplaceItemDef(item.Definition.ID);
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			int maxSellQuantity = 0;
			if (marketplaceDefinition != null)
			{
				maxSellQuantity = marketplaceDefinition.MaxSellQuantity;
			}
			DynamicIngredientsDefinition dynamicIngredientsDefinition = item.Definition as DynamicIngredientsDefinition;
			if (view != null && dynamicIngredientsDefinition == null && marketplaceItemDef != null)
			{
				int itemCount = view.ItemCount;
				view.SetForSaleItem(marketplaceItemDef, item, maxSellQuantity, reset, coppaService.Restricted());
				if (reset)
				{
					view.ItemCount = itemCount;
				}
			}
			selectStorageBuildingItemSignal.Dispatch(item.ID);
			view.UpdateCountRange();
			view.UpdatePriceRange();
		}

		private void OpenPanel(int slotIndex)
		{
			if (!(view == null) && m_isSalePanelOpened)
			{
				m_slotId = slotIndex;
				if (view.item == null)
				{
					GetFirstItemInStorage();
				}
				soundFXSignal.Dispatch("Play_marketplace_newSale_01");
				view.OpenPanel();
			}
		}

		private void CreateSaleOnClick()
		{
			soundFXSignal.Dispatch("Play_marketplace_putOnSale_01");
			int second = Mathf.FloorToInt(view.Price);
			Tuple<int, int, int, int> type = new Tuple<int, int, int, int>(view.marketplaceDef.ID, second, view.ItemCount, m_slotId);
			gameContext.injectionBinder.GetInstance<SellToAISignal>().Dispatch(type);
			if (view.item.Definition.Storable)
			{
				setStorageSignal.Dispatch();
			}
			view.ClosePanel();
			CloseCreateNewSalePanelSignal.Dispatch();
			displaySignal.Dispatch(19000011, false, new Signal<bool>());
		}

		private void OnAddItemCountClick(int delta)
		{
			view.ItemCount += delta;
			view.CheckAmountMaxOutState(view.ItemCount + delta);
		}

		private void OnMinusItemCountClick(int delta)
		{
			view.ItemCount -= delta;
			view.CheckAmountMaxOutState(view.ItemCount - delta);
		}

		private void OnAddPriceClick(int delta)
		{
			view.Price += delta;
			view.CheckPriceMaxOutState(view.Price + delta);
		}

		private void OnMinusPriceClick(int delta)
		{
			view.Price -= delta;
			view.CheckPriceMaxOutState(view.Price - delta);
		}

		private void SalePanelClosed()
		{
			m_isSalePanelOpened = false;
		}

		private void SalePanelOpened(bool isInstant)
		{
			m_isSalePanelOpened = true;
		}
	}
}
