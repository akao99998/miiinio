using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class ItemListView : KampaiView
	{
		public ButtonView Title;

		public Text TitleText;

		public RectTransform ScrollViewParent;

		public KampaiImage TabIcon;

		private Dictionary<StoreItemType, List<StoreButtonView>> buttonViews;

		private float itemButtonHeight;

		private float itemPadding;

		private StoreItemType currentType;

		private Animator animator;

		public void Init()
		{
			animator = base.transform.GetComponentInParent<Animator>();
			buttonViews = new Dictionary<StoreItemType, List<StoreButtonView>>();
		}

		internal void SetupButtonHeight(float buttonHeight, float buttonPadding)
		{
			itemButtonHeight = buttonHeight;
			itemPadding = buttonPadding;
		}

		internal Dictionary<StoreItemType, List<StoreButtonView>> GetAllButtonViews()
		{
			return buttonViews;
		}

		internal void AddStoreButton(StoreItemType type, StoreButtonView buttonView)
		{
			if (!buttonViews.ContainsKey(type))
			{
				buttonViews[type] = new List<StoreButtonView>();
			}
			buttonViews[type].Add(buttonView);
		}

		internal List<StoreButtonView> GetStoreButtonViews(StoreItemType type)
		{
			if (buttonViews.ContainsKey(type))
			{
				return buttonViews[type];
			}
			return null;
		}

		internal StoreButtonView GetStoreButtonViewByID(int ID)
		{
			foreach (StoreItemType key in buttonViews.Keys)
			{
				List<StoreButtonView> list = buttonViews[key];
				foreach (StoreButtonView item in list)
				{
					if (item.storeItemDefinition.ID == ID)
					{
						return item;
					}
				}
			}
			return null;
		}

		internal StoreItemType UpdateStoreButtonState(int buildingDefinitionID, bool isAddingBuilding)
		{
			foreach (KeyValuePair<StoreItemType, List<StoreButtonView>> buttonView in buttonViews)
			{
				foreach (StoreButtonView item in buttonView.Value)
				{
					if (item.definition.ID == buildingDefinitionID)
					{
						item.SetNewUnlockState(false);
						item.ChangeBuildingCount(isAddingBuilding);
						item.AdjustIncrementalCost();
						return buttonView.Key;
					}
				}
			}
			return StoreItemType.BaseResource;
		}

		internal bool SetupItemMenu(StoreItemType type, string localizedTitle)
		{
			if (buttonViews.ContainsKey(type))
			{
				if (buttonViews[type].Count == 0)
				{
					return false;
				}
				TitleText.text = localizedTitle;
				ShowAndPositionMenuItems(type);
				return true;
			}
			return false;
		}

		internal void RefreshStoreButtonLayout()
		{
			ShowAndPositionMenuItems(currentType);
		}

		internal void ShowAndPositionMenuItems(StoreItemType type)
		{
			int count = buttonViews[type].Count;
			if (count == 0)
			{
				return;
			}
			foreach (StoreButtonView item in buttonViews[currentType])
			{
				item.gameObject.SetActive(false);
			}
			currentType = type;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				StoreButtonView storeButtonView = buttonViews[type][i];
				if (storeButtonView.ShouldBeRendered() || currentType == StoreItemType.Featured)
				{
					RectTransform rectTransform = storeButtonView.transform as RectTransform;
					rectTransform.offsetMin = new Vector2(0f, (0f - (itemButtonHeight + itemPadding)) * (float)num - itemButtonHeight);
					rectTransform.offsetMax = new Vector2(0f, (0f - (itemButtonHeight + itemPadding)) * (float)num);
					storeButtonView.gameObject.SetActive(true);
					num++;
				}
			}
			ScrollViewParent.offsetMin = new Vector2(0f, (float)(-num) * (itemButtonHeight + itemPadding) + ScrollViewParent.offsetMax.y);
		}

		internal void MoveSubMenu(bool show)
		{
			animator.SetBool("OnOpenSubMenu", show);
		}
	}
}
