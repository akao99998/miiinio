using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	[RequireComponent(typeof(Animator))]
	public class CurrencyStoreView : PopupMenuView
	{
		private const int minimumAnimItemCount = 4;

		public RectTransform CategoryGridParent;

		public RectTransform CategoryParent;

		public Text title;

		public ButtonView backgroundButton;

		internal bool isOpen;

		internal List<int> viewCounts = new List<int>();

		private ILocalizationService localService;

		private List<CurrencyStoreCategoryView> currencyStoreCategoryViews = new List<CurrencyStoreCategoryView>();

		private List<CurrencyStoreCategoryButtonView> categoryButtonViews = new List<CurrencyStoreCategoryButtonView>();

		private Transform pulsing;

		private bool openAnimFinished;

		private GameObject storeCategoryButtonPrefab;

		private GameObject storeCategoryGridPrefab;

		private int currentCategoryIndex;

		internal void Init(ILocalizationService localization)
		{
			base.Init();
			localService = localization;
			storeCategoryButtonPrefab = KampaiResources.Load("cmp_StoreButton") as GameObject;
			storeCategoryGridPrefab = KampaiResources.Load("cmp_currencyStoreCategoryGrid") as GameObject;
		}

		internal void ClearViews()
		{
			StopPulsing();
			UIUtils.SafeDestoryViews(currencyStoreCategoryViews);
			UIUtils.SafeDestoryViews(categoryButtonViews);
		}

		internal CurrencyStoreCategoryButtonView BuildCategoryButton(CurrencyStoreCategoryDefinition categoryDefinition)
		{
			GameObject gameObject = Object.Instantiate(storeCategoryButtonPrefab);
			gameObject.name = string.Format("Category: {0}", categoryDefinition.StoreCategoryType);
			gameObject.transform.SetParent(CategoryParent, false);
			CurrencyStoreCategoryButtonView component = gameObject.GetComponent<CurrencyStoreCategoryButtonView>();
			component.Init(categoryDefinition, localService);
			return component;
		}

		internal CurrencyStoreCategoryView BuildCategoryContainer(CurrencyStoreCategoryDefinition categoryDefinition, CurrencyStoreCategoryButtonView buttonView)
		{
			GameObject gameObject = Object.Instantiate(storeCategoryGridPrefab);
			gameObject.name = string.Format("Category Grid: {0}", categoryDefinition.StoreCategoryType);
			gameObject.transform.SetParent(CategoryGridParent, false);
			gameObject.SetActive(false);
			CurrencyStoreCategoryView component = gameObject.GetComponent<CurrencyStoreCategoryView>();
			component.Init(categoryDefinition);
			currencyStoreCategoryViews.Add(component);
			categoryButtonViews.Add(buttonView);
			return component;
		}

		internal CurrencyButtonView BuildCategoryItem(CurrencyItemDefinition currencyItemDef, StoreItemDefinition storeItemDef, string inputStr, string outputStr, CurrencyStoreCategoryView categoryView, bool hasVFX)
		{
			CurrencyButtonView currencyButtonView = CurrencyButtonBuilder.Build(localService, currencyItemDef, storeItemDef, inputStr, outputStr, categoryView.transform, hasVFX);
			currencyButtonView.Definition = storeItemDef;
			categoryView.AddCurrencyButtonView(currencyButtonView);
			return currencyButtonView;
		}

		internal void OnPremiumCatalogUpdated(ICurrencyService currencyService, IDefinitionService definitionService)
		{
			for (int i = 0; i < currencyStoreCategoryViews.Count; i++)
			{
				currencyStoreCategoryViews[i].OnPremiumCatalogUpdated(currencyService, definitionService);
			}
		}

		internal void RefreshButtons(bool forceLocked, ICurrencyStoreService currencyStoreService, ILocalizationService localizationService)
		{
			foreach (CurrencyStoreCategoryView currencyStoreCategoryView in currencyStoreCategoryViews)
			{
				currencyStoreCategoryView.RefreshButtons(forceLocked, currencyStoreService, localizationService);
			}
		}

		internal void StopPulsing()
		{
			if (pulsing != null)
			{
				Go.killAllTweensWithTarget(pulsing);
				pulsing.transform.localScale = new Vector3(1f, 1f, 1f);
				pulsing = null;
			}
		}

		private void TrySlideInItems()
		{
			if (openAnimFinished && viewCounts[currentCategoryIndex] > 4)
			{
				base.animator.StopPlayback();
				base.animator.Play("SlideInItems", 0, 0f);
			}
		}

		internal void Cleanup()
		{
			StopPulsing();
			SetScrollableTransform(null);
			isOpen = false;
			base.Close();
		}

		private void DisplayCategory(CurrencyStoreCategoryView categoryView, bool repositionToCenter = false)
		{
			GameObject gameObject = categoryView.gameObject;
			RectTransform rectTransform = gameObject.transform as RectTransform;
			SetScrollableTransform(rectTransform);
			if (repositionToCenter)
			{
				rectTransform.anchoredPosition = Vector2.zero;
			}
			TrySlideInItems();
			gameObject.SetActive(true);
			title.text = localService.GetString(categoryView.GetCategoryTitle());
		}

		private void SetScrollableTransform(RectTransform rectTransform)
		{
			if (CategoryGridParent != null)
			{
				ScrollRect component = CategoryGridParent.GetComponent<ScrollRect>();
				if (component != null)
				{
					component.content = rectTransform;
				}
			}
		}

		private void UpdateNextCategoryIndex(bool next)
		{
			if (next)
			{
				currentCategoryIndex++;
				if (currentCategoryIndex >= currencyStoreCategoryViews.Count)
				{
					currentCategoryIndex = 0;
				}
			}
			else
			{
				currentCategoryIndex--;
				if (currentCategoryIndex < 0)
				{
					currentCategoryIndex = currencyStoreCategoryViews.Count - 1;
				}
			}
		}

		internal void ShowCategory(int categoryDefinitionID, int amountNeeded = 0, bool firstTime = false)
		{
			StopPulsing();
			int num = currentCategoryIndex;
			for (int i = 0; i < currencyStoreCategoryViews.Count; i++)
			{
				CurrencyStoreCategoryView currencyStoreCategoryView = currencyStoreCategoryViews[i];
				CurrencyStoreCategoryButtonView currencyStoreCategoryButtonView = categoryButtonViews[i];
				if (currencyStoreCategoryView.GetStoreCategoryDefinitionID() == categoryDefinitionID)
				{
					currentCategoryIndex = i;
					currencyStoreCategoryView.gameObject.SetActive(true);
					if (firstTime)
					{
						currencyStoreCategoryButtonView.MarkAsSelected();
					}
				}
				else
				{
					currencyStoreCategoryView.gameObject.SetActive(false);
					currencyStoreCategoryButtonView.MarkAsDeselected();
				}
			}
			if (currentCategoryIndex == num && !firstTime)
			{
				return;
			}
			CurrencyStoreCategoryView currencyStoreCategoryView2 = currencyStoreCategoryViews[currentCategoryIndex];
			int num2 = currencyStoreCategoryViews.Count;
			while (!currencyStoreCategoryView2.CanDisplay() && num2 > 0)
			{
				UpdateNextCategoryIndex(true);
				currencyStoreCategoryView2 = currencyStoreCategoryViews[currentCategoryIndex];
				num2--;
			}
			if (num2 == 0)
			{
				logger.Warning("Failed to find a suitable mtx category view to display.");
				return;
			}
			DisplayCategory(currencyStoreCategoryView2, true);
			if (amountNeeded > 0)
			{
				pulsing = currencyStoreCategoryView2.GetClosestValueView(amountNeeded);
				if (pulsing != null)
				{
					Vector3 originalScale = Vector3.one;
					TweenUtil.Throb(pulsing, 0.98f, 0.5f, out originalScale);
				}
			}
		}

		public override void FinishedOpening()
		{
			base.FinishedOpening();
			openAnimFinished = true;
			TrySlideInItems();
		}
	}
}
