using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class CurrencyStoreCategoryView : strange.extensions.mediation.impl.View
	{
		private List<CurrencyButtonView> currencyButtonViews;

		private CurrencyStoreCategoryDefinition definition;

		internal void Init(CurrencyStoreCategoryDefinition definition)
		{
			currencyButtonViews = new List<CurrencyButtonView>();
			this.definition = definition;
		}

		internal int GetStoreCategoryDefinitionID()
		{
			return definition.ID;
		}

		internal string GetCategoryTitle()
		{
			return definition.Description;
		}

		internal void RefreshButtons(bool forceLocked, ICurrencyStoreService currencyStoreService, ILocalizationService localizationService)
		{
			foreach (CurrencyButtonView currencyButtonView in currencyButtonViews)
			{
				StoreItemType type = currencyButtonView.Definition.Type;
				if (type == StoreItemType.SalePack)
				{
					UpdateSalePackButton(currencyButtonView, forceLocked, currencyStoreService, localizationService);
					continue;
				}
				bool flag = !forceLocked;
				currencyButtonView.UnlockButton(flag);
				if (!flag)
				{
					currencyButtonView.ItemWorth.text = localizationService.GetStringUpper("StarterPackMTXLockedBanner");
				}
			}
		}

		private void UpdateSalePackButton(CurrencyButtonView btnView, bool forceLocked, ICurrencyStoreService currencyStoreService, ILocalizationService localizationService)
		{
			CurrencyStorePackDefinition currencyStorePackDefinition = currencyStoreService.GetCurrencyStorePackDefinition(btnView.Definition.ReferencedDefID);
			if (currencyStorePackDefinition != null)
			{
				if (currencyStoreService.HasPurchasedEnough(currencyStorePackDefinition))
				{
					btnView.gameObject.SetActive(false);
					return;
				}
				bool flag = forceLocked || currencyStoreService.ShouldPackBeVisuallyLocked(currencyStorePackDefinition);
				UpdateButtonUnlock(btnView, !flag, currencyStorePackDefinition, localizationService);
			}
		}

		private void UpdateButtonUnlock(CurrencyButtonView btnView, bool unlocked, CurrencyStorePackDefinition packDef, ILocalizationService localService)
		{
			if (btnView != null)
			{
				if (unlocked)
				{
					btnView.UnlockButton(true);
					string text = ((!string.IsNullOrEmpty(packDef.SaleBanner)) ? localService.GetString(packDef.SaleBanner) : localService.GetStringUpper("StarterPackMTXDiscountButton"));
					btnView.ItemWorth.text = text;
				}
				else
				{
					btnView.UnlockButton(false);
					btnView.ItemWorth.text = localService.GetStringUpper("StarterPackMTXLockedBanner");
				}
			}
		}

		internal Transform GetClosestValueView(int amount)
		{
			foreach (CurrencyButtonView currencyButtonView in currencyButtonViews)
			{
				int result;
				bool flag = int.TryParse(currencyButtonView.ItemWorth.text, out result);
				if (currencyButtonView.gameObject.activeSelf && flag && result > amount)
				{
					return currencyButtonView.transform;
				}
			}
			return null;
		}

		internal bool CanDisplay()
		{
			foreach (CurrencyButtonView currencyButtonView in currencyButtonViews)
			{
				if (currencyButtonView.gameObject.activeSelf)
				{
					return true;
				}
			}
			return false;
		}

		internal void AddCurrencyButtonView(CurrencyButtonView currencyButtonView)
		{
			currencyButtonViews.Add(currencyButtonView);
		}

		internal void OnPremiumCatalogUpdated(ICurrencyService currencyService, IDefinitionService definitionService)
		{
			for (int i = 0; i < currencyButtonViews.Count; i++)
			{
				CurrencyButtonView currencyButtonView = currencyButtonViews[i];
				CurrencyItemDefinition currencyItemDefinition;
				if (definitionService.TryGet<CurrencyItemDefinition>(currencyButtonView.Definition.ReferencedDefID, out currencyItemDefinition))
				{
					PremiumCurrencyItemDefinition premiumCurrencyItemDefinition = currencyItemDefinition as PremiumCurrencyItemDefinition;
					if (premiumCurrencyItemDefinition != null)
					{
						currencyButtonView.ItemPrice.text = currencyService.GetPriceWithCurrencyAndFormat(premiumCurrencyItemDefinition.SKU);
					}
				}
			}
		}
	}
}
