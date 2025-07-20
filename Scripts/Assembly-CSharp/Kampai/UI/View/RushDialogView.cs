using System.Collections.Generic;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class RushDialogView : PopupMenuView
	{
		public enum RushDialogType
		{
			DEFAULT = 0,
			STORAGE_EXPAND = 1,
			SOCIAL = 2,
			DEBRIS = 3,
			LAND_EXPANSION = 4,
			BRIDGE_QUEST = 5,
			VILLAIN_LAIR_PORTAL_REPAIR = 6,
			VILLAIN_LAIR_RESOURCE_PLOT = 7
		}

		public DoubleConfirmButtonView PurchaseButtonView;

		public ButtonView UpgradeButton;

		public RectTransform ScrollViewParent;

		public GameObject PurchasePanel;

		public GameObject UpgradePanel;

		public Text RushCost;

		public Text Title;

		protected IList<RequiredItemView> items;

		protected ILocalizationService localService;

		protected float requiredItemWidth;

		protected float requiredItemPadding;

		internal void Init(ILocalizationService localService)
		{
			base.Init();
			PostInit(localService);
		}

		internal void InitProgrammatic(BuildingPopupPositionData buildingPopupPositionData, ILocalizationService localService)
		{
			InitProgrammatic(buildingPopupPositionData);
			PostInit(localService);
		}

		private void PostInit(ILocalizationService localService)
		{
			this.localService = localService;
			items = new List<RequiredItemView>();
			base.Open();
		}

		internal virtual void SetupDialog(RushDialogType type, bool showPurchaseButton)
		{
			if (showPurchaseButton)
			{
				PurchasePanel.SetActive(true);
				UpgradePanel.SetActive(false);
				return;
			}
			PurchasePanel.SetActive(false);
			UpgradePanel.SetActive(true);
			RushButtonView rushButtonView = UpgradeButton as RushButtonView;
			if (rushButtonView != null)
			{
				rushButtonView.SkipDoubleConfirm = true;
			}
		}

		internal virtual void SetupItemCount(int count)
		{
			float num = (requiredItemWidth + requiredItemPadding) * (float)count;
			RectTransform rectTransform = ScrollViewParent.parent.transform as RectTransform;
			ScrollViewParent.sizeDelta = new Vector2(num, 0f);
			if (num < rectTransform.rect.width)
			{
				rectTransform.GetComponent<ScrollRect>().enabled = false;
				ScrollViewParent.anchoredPosition = Vector2.zero;
			}
			else
			{
				ScrollViewParent.pivot = new Vector2(0.5f, 0.5f);
			}
		}

		internal void AddRequiredItem(RequiredItemView view, int index, RectTransform parent)
		{
			requiredItemWidth = (view.transform as RectTransform).sizeDelta.x;
			requiredItemPadding = view.PaddingInPixels;
			RectTransform rectTransform = view.transform as RectTransform;
			rectTransform.SetParent(parent, false);
			if (index == -1)
			{
				rectTransform.offsetMin = new Vector2((0f - requiredItemWidth) / 2f, 0f);
				rectTransform.offsetMax = new Vector2(requiredItemWidth / 2f, 0f);
			}
			else
			{
				rectTransform.offsetMin = new Vector2((float)index * (requiredItemWidth + requiredItemPadding), 0f);
				rectTransform.offsetMax = new Vector2((float)index * (requiredItemWidth + requiredItemPadding) + requiredItemWidth, 0f);
			}
			items.Add(view);
		}

		internal void MoveRequiredItem()
		{
			int num = 0;
			for (int i = 0; i < items.Count; i++)
			{
				RequiredItemView requiredItemView = items[i];
				if (!(requiredItemView == null) && requiredItemView.ItemDefinitionID != 0)
				{
					RectTransform rectTransform = requiredItemView.transform as RectTransform;
					if (!(rectTransform == null))
					{
						requiredItemWidth = rectTransform.sizeDelta.x;
						requiredItemPadding = requiredItemView.PaddingInPixels;
						rectTransform.offsetMin = new Vector2((float)num * (requiredItemWidth + requiredItemPadding), 0f);
						rectTransform.offsetMax = new Vector2((float)num * (requiredItemWidth + requiredItemPadding) + requiredItemWidth, 0f);
						num++;
					}
				}
			}
		}

		internal IList<RequiredItemView> GetItemList()
		{
			return items;
		}

		internal void SetupItemCost(int cost)
		{
			RushCost.text = UIUtils.FormatLargeNumber(cost);
		}

		internal void resetAllExceptRequiredItemTapState(int itemDefID)
		{
			foreach (RequiredItemView item in items)
			{
				if (item.RushBtn.Item == null || item.RushBtn.Item.ID != itemDefID)
				{
					item.RushBtn.ResetTapState();
				}
			}
			PurchaseButtonView.ResetTapState();
		}

		internal void resetAllRequiredItemsTapState()
		{
			foreach (RequiredItemView item in items)
			{
				if (item.RushBtn != null)
				{
					item.RushBtn.ResetTapState();
					item.RushBtn.ResetAnim();
				}
			}
		}

		internal void DeleteItem(int itemDefID)
		{
			foreach (RequiredItemView item in items)
			{
				markDelete(item, itemDefID);
			}
		}

		private void markDelete(RequiredItemView riv, int itemDefID)
		{
			if (riv.RushBtn.Item != null && riv.RushBtn.Item.ID == itemDefID)
			{
				riv.PurchasePanel.SetActive(false);
				riv.ItemQuantity.text = string.Format("{0}/{1}", riv.ItemNeeded, riv.ItemNeeded);
				riv.ItemQuantity.color = Color.black;
				riv.ItemCost.gameObject.SetActive(false);
				riv.RushBtn.gameObject.SetActive(false);
			}
		}

		internal void DeleteAllItems()
		{
			foreach (RequiredItemView item in items)
			{
				if (item.IsInvoking())
				{
					item.CancelInvoke();
				}
				Object.Destroy(item.gameObject);
			}
			items.Clear();
		}
	}
}
