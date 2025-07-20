using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class CurrencyStoreMediator : UIStackMediator<CurrencyStoreView>
	{
		[Inject]
		public PremiumCurrencyCatalogUpdatedSignal premiumCurrencyCatalogUpdatedSignal { get; set; }

		[Inject]
		public CurrencyDialogClosedSignal currencyDialogClosedSignal { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IQuestScriptService questScriptService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public FTUELevelChangedSignal ftueLevelChangedSignal { get; set; }

		[Inject]
		public RefreshMTXStoreSignal refreshMTXStoreSignal { get; set; }

		[Inject]
		public CancelPurchaseSignal cancelPurchaseSignal { get; set; }

		[Inject]
		public CloseHUDSignal closeSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ICurrencyStoreService currencyStoreService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			refreshMTXStoreSignal.AddListener(OnRefreshStore);
			premiumCurrencyCatalogUpdatedSignal.AddListener(OnPremiumCatalogUpdated);
			closeSignal.AddListener(CloseMenu);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			ftueLevelChangedSignal.AddListener(FTUELevelChanged);
			base.view.backgroundButton.ClickedSignal.AddListener(Close);
			base.view.Init(localService);
			if (playerService.GetHighestFtueCompleted() < 9)
			{
				questScriptService.PauseQuestScripts();
			}
		}

		public override void OnRemove()
		{
			base.OnRemove();
			refreshMTXStoreSignal.RemoveListener(OnRefreshStore);
			premiumCurrencyCatalogUpdatedSignal.RemoveListener(OnPremiumCatalogUpdated);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.backgroundButton.ClickedSignal.RemoveListener(Close);
			ftueLevelChangedSignal.RemoveListener(FTUELevelChanged);
			if (playerService.GetHighestFtueCompleted() < 9)
			{
				questScriptService.ResumeQuestScripts();
			}
		}

		protected override void Close()
		{
			OnClickBackgroundButton();
		}

		public override void Initialize(GUIArguments args)
		{
			Tuple<int, int> tuple = args.Get<Tuple<int, int>>();
			int item = tuple.Item1;
			OnStoreDefinitionLoaded(item);
			int item2 = tuple.Item2;
			base.view.ShowCategory(item, item2, true);
			if (!AllItemsAreLocked())
			{
				currencyStoreService.MarkCategoryAsViewed(item);
			}
			base.view.Open();
		}

		private void CloseMenu(bool closeCurrency)
		{
			closeSignal.RemoveListener(CloseMenu);
			base.view.Cleanup();
		}

		private void OnClickBackgroundButton()
		{
			cancelPurchaseSignal.Dispatch(false);
			closeSignal.RemoveListener(CloseMenu);
			closeSignal.Dispatch(false);
			gameContext.injectionBinder.GetInstance<CancelBuildingMovementSignal>().Dispatch(false);
			base.view.Cleanup();
		}

		private void GetStrings(StoreItemDefinition storeItemDef, PremiumCurrencyItemDefinition premium, TransactionDefinition transaction, ref string inputStr, ref string outputStr)
		{
			if (premium != null)
			{
				inputStr = currencyService.GetPriceWithCurrencyAndFormat(premium.SKU);
				if (storeItemDef.Type == StoreItemType.PremiumCurrency)
				{
					outputStr = UIUtils.FormatLargeNumber(TransactionUtil.GetPremiumOutputForTransaction(transaction));
				}
				else if (storeItemDef.Type == StoreItemType.SalePack)
				{
					outputStr = localService.GetStringUpper("StarterPackMTXDiscountButton");
				}
				else if (transaction != null)
				{
					outputStr = UIUtils.FormatLargeNumber(TransactionUtil.GetGrindOutputForTransaction(transaction));
				}
			}
			else if (transaction != null)
			{
				inputStr = TransactionUtil.GetPremiumCostForTransaction(transaction).ToString();
				outputStr = UIUtils.FormatLargeNumber(TransactionUtil.GetGrindOutputForTransaction(transaction));
			}
		}

		private void OnRefreshStore()
		{
			bool forceLocked = AllItemsAreLocked();
			base.view.RefreshButtons(forceLocked, currencyStoreService, localService);
		}

		private void OnStoreDefinitionLoaded(int storeCategoryDefinitionID)
		{
			base.view.ClearViews();
			IList<CurrencyStoreCategoryDefinition> currencyStoreCategoryDefinitions = definitionService.GetCurrencyStoreCategoryDefinitions();
			for (int i = 0; i < currencyStoreCategoryDefinitions.Count; i++)
			{
				CurrencyStoreCategoryDefinition currencyStoreCategoryDefinition = currencyStoreCategoryDefinitions[i];
				base.view.viewCounts.Add(currencyStoreCategoryDefinition.StoreItemDefinitionIDs.Count);
				List<StoreItemDefinition> list = new List<StoreItemDefinition>();
				for (int j = 0; j < currencyStoreCategoryDefinition.StoreItemDefinitionIDs.Count; j++)
				{
					int num = currencyStoreCategoryDefinition.StoreItemDefinitionIDs[j];
					if (currencyStoreService.IsValidCurrencyItem(num, currencyStoreCategoryDefinition.StoreCategoryType))
					{
						list.Add(definitionService.Get<StoreItemDefinition>(num));
					}
				}
				if (list.Count <= 0)
				{
					continue;
				}
				CurrencyStoreCategoryButtonView currencyStoreCategoryButtonView = base.view.BuildCategoryButton(currencyStoreCategoryDefinition);
				int badgeCount = 0;
				if (!AllItemsAreLocked() && currencyStoreCategoryDefinition.ID != storeCategoryDefinitionID)
				{
					badgeCount = currencyStoreService.GetBadgeCount(currencyStoreCategoryDefinition);
				}
				currencyStoreCategoryButtonView.SetBadgeCount(badgeCount);
				currencyStoreCategoryButtonView.ClickedSignal.AddListener(OnCategoryButtonClicked);
				CurrencyStoreCategoryView categoryView = base.view.BuildCategoryContainer(currencyStoreCategoryDefinition, currencyStoreCategoryButtonView);
				foreach (StoreItemDefinition item in list)
				{
					CurrencyItemDefinition currencyItemDefinition = definitionService.Get<CurrencyItemDefinition>(item.ReferencedDefID);
					PremiumCurrencyItemDefinition premium = currencyItemDefinition as PremiumCurrencyItemDefinition;
					TransactionDefinition definition;
					definitionService.TryGet<TransactionDefinition>(item.TransactionID, out definition);
					string inputStr = string.Empty;
					string outputStr = string.Empty;
					GetStrings(item, premium, definition, ref inputStr, ref outputStr);
					base.view.BuildCategoryItem(currencyItemDefinition, item, inputStr, outputStr, categoryView, true);
				}
			}
			OnRefreshStore();
		}

		private void OnCategoryButtonClicked(CurrencyStoreCategoryDefinition storeCategoryDefinition)
		{
			base.view.ShowCategory(storeCategoryDefinition.ID);
			currencyStoreService.MarkCategoryAsViewed(storeCategoryDefinition);
		}

		private void OnPremiumCatalogUpdated()
		{
			base.view.OnPremiumCatalogUpdated(currencyService, definitionService);
		}

		private void FTUELevelChanged()
		{
			bool forceLocked = AllItemsAreLocked();
			base.view.RefreshButtons(forceLocked, currencyStoreService, localService);
		}

		private bool AllItemsAreLocked()
		{
			return !currencyService.TransactionProcessingEnabled();
		}

		private void OnMenuClose()
		{
			currencyDialogClosedSignal.Dispatch();
			guiService.Execute(GUIOperation.Unload, "screen_Store");
		}
	}
}
