using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class TabMenuView : KampaiView
	{
		public Text StoreTitle;

		public RectTransform ScrollViewParent;

		private int count;

		private List<StoreTabView> tabViews;

		private Animator animator;

		private Dictionary<StoreItemType, int> oldBadgeCount;

		private RemoveUnlockForBuildMenuSignal removeUnlockForBuildMenuSignal;

		private SetNewUnlockForBuildMenuSignal setNewUnlockForBuildMenuSignal;

		public void Init(SetNewUnlockForBuildMenuSignal setNewUnlockForBuildMenuSignal, RemoveUnlockForBuildMenuSignal removeUnlockForBuildMenuSignal)
		{
			this.removeUnlockForBuildMenuSignal = removeUnlockForBuildMenuSignal;
			this.setNewUnlockForBuildMenuSignal = setNewUnlockForBuildMenuSignal;
			animator = base.transform.GetComponentInParent<Animator>();
			StoreTitle.rectTransform.offsetMin = Vector2.zero;
			StoreTitle.rectTransform.offsetMax = Vector2.zero;
			tabViews = new List<StoreTabView>();
			oldBadgeCount = new Dictionary<StoreItemType, int>();
		}

		private StoreTabView GetStoreTabView(StoreItemType type)
		{
			foreach (StoreTabView tabView in tabViews)
			{
				if (tabView.Type == type)
				{
					return tabView;
				}
			}
			return null;
		}

		internal void SetBadgeForStoreTab(StoreItemType type, int badgeCount)
		{
			StoreTabView storeTabView = GetStoreTabView(type);
			if (storeTabView != null)
			{
				storeTabView.SetBadgeCount(badgeCount);
				oldBadgeCount[type] = badgeCount;
			}
		}

		internal void SetUnlockForTab(StoreItemType type, int badgeCount)
		{
			StoreTabView storeTabView = GetStoreTabView(type);
			if (storeTabView != null)
			{
				storeTabView.SetNewUnlockState(badgeCount);
				oldBadgeCount[type] = badgeCount;
			}
		}

		internal void ClearUnlockForTab(StoreItemType type)
		{
			StoreTabView storeTabView = GetStoreTabView(type);
			if (storeTabView != null)
			{
				storeTabView.SetNewUnlockState(0);
				oldBadgeCount[type] = 0;
				removeUnlockForBuildMenuSignal.Dispatch(oldBadgeCount[type]);
			}
		}

		public GameObject GetStoreTabObject(StoreItemType type)
		{
			StoreTabView storeTabView = GetStoreTabView(type);
			if (storeTabView != null)
			{
				return storeTabView.gameObject;
			}
			return null;
		}

		internal void AddStoreTab(StoreTabView tabView, float buttonHeight, float padding)
		{
			tabViews.Add(tabView);
			count = tabViews.Count;
			ScrollViewParent.offsetMin = new Vector2(0f, (float)(-count) * buttonHeight + ScrollViewParent.offsetMax.y);
			ScrollViewParent.offsetMax = Vector2.zero;
			RectTransform rectTransform = tabViews[count - 1].transform as RectTransform;
			rectTransform.offsetMin = new Vector2(padding, (0f - buttonHeight - padding) * (float)count);
			rectTransform.offsetMax = new Vector2(0f - padding, (0f - buttonHeight - padding) * (float)(count - 1) - padding);
			tabViews[count - 1].gameObject.SetActive(true);
		}

		internal void ToggleStoreTab(StoreItemType type, bool show)
		{
			foreach (StoreTabView tabView in tabViews)
			{
				if (tabView.Type == type)
				{
					tabView.gameObject.SetActive(show);
					break;
				}
			}
		}

		internal void HideBadge(StoreItemType type)
		{
			SetBadgeForStoreTab(type, 0);
			if (oldBadgeCount.ContainsKey(type))
			{
				removeUnlockForBuildMenuSignal.Dispatch(oldBadgeCount[type]);
				oldBadgeCount[type] = 0;
			}
			SetUnlockForTab(type, 0);
		}

		internal void ShowMenu(bool show)
		{
			if (show)
			{
				int num = 0;
				foreach (KeyValuePair<StoreItemType, int> item in oldBadgeCount)
				{
					num += item.Value;
				}
				setNewUnlockForBuildMenuSignal.Dispatch(num);
			}
			animator.SetBool("OnOpenSubMenu", !show);
		}
	}
}
