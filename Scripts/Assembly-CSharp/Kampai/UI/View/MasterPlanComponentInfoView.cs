using System;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class MasterPlanComponentInfoView : KampaiView
	{
		public enum ItemType
		{
			Requires = 0,
			Rewards = 1
		}

		public bool enabledTextOnRequires;

		public LocalizeView titleText;

		public KampaiImage itemPanelBackground;

		public Transform itemPanel;

		[Header("Colors")]
		public Color requiresPanelBackgroundColor;

		public Color requiresTitleColor;

		public Color rewardPanelBackgroundColor;

		public Color rewardTitleColor;

		[Header("External Headers")]
		public GameObject itemViewPrefab;

		private readonly IList<MonoBehaviour> itemIconList = new List<MonoBehaviour>();

		internal readonly Signal<MasterPlanComponentTask, KampaiImage> setTaskIconSignal = new Signal<MasterPlanComponentTask, KampaiImage>();

		internal readonly Signal<QuantityItem, KampaiImage> setRewardIconSignal = new Signal<QuantityItem, KampaiImage>();

		internal readonly Signal updateItemsSignal = new Signal();

		public ItemType itemType { get; internal set; }

		public int index { get; private set; }

		internal void Init(ItemType itemType, int index)
		{
			this.itemType = itemType;
			this.index = index;
			switch (this.itemType)
			{
			case ItemType.Requires:
				titleText.LocKey = "MasterPlanRequires";
				titleText.color = requiresTitleColor;
				itemPanelBackground.color = requiresPanelBackgroundColor;
				break;
			case ItemType.Rewards:
				titleText.LocKey = "MasterPlanReward";
				titleText.color = rewardTitleColor;
				itemPanelBackground.color = rewardPanelBackgroundColor;
				break;
			}
			updateItemsSignal.Dispatch();
		}

		internal void Refresh(Type type, int componentIndex)
		{
			if (type == GetType())
			{
				index = componentIndex;
				updateItemsSignal.Dispatch();
			}
		}

		internal void SetupRewardIcons(IList<QuantityItem> rewardItems)
		{
			if (rewardItems == null)
			{
				return;
			}
			int count = rewardItems.Count;
			int num = UpdateImages(count);
			for (int i = 0; i < num; i++)
			{
				MasterPlanComponentItemView masterPlanComponentItemView = itemIconList[i] as MasterPlanComponentItemView;
				if (!(masterPlanComponentItemView == null))
				{
					if (i > count)
					{
						masterPlanComponentItemView.gameObject.SetActive(false);
						continue;
					}
					masterPlanComponentItemView.gameObject.SetActive(true);
					QuantityItem quantityItem = rewardItems[i];
					masterPlanComponentItemView.quantityText.text = UIUtils.FormatLargeNumber((int)quantityItem.Quantity);
					setRewardIconSignal.Dispatch(quantityItem, masterPlanComponentItemView.icon);
				}
			}
		}

		internal void SetupTasksIcons(IList<MasterPlanComponentTask> tasks)
		{
			if (tasks == null)
			{
				return;
			}
			int count = tasks.Count;
			int num = UpdateImages(count);
			for (int i = 0; i < num; i++)
			{
				MasterPlanComponentItemView masterPlanComponentItemView = itemIconList[i] as MasterPlanComponentItemView;
				if (masterPlanComponentItemView == null)
				{
					continue;
				}
				if (i >= count)
				{
					masterPlanComponentItemView.gameObject.SetActive(false);
					continue;
				}
				masterPlanComponentItemView.gameObject.SetActive(true);
				MasterPlanComponentTask masterPlanComponentTask = tasks[i];
				if (masterPlanComponentTask != null)
				{
					setTaskIconSignal.Dispatch(masterPlanComponentTask, masterPlanComponentItemView.icon);
					masterPlanComponentItemView.quantityText.gameObject.SetActive(enabledTextOnRequires);
				}
			}
		}

		private int UpdateImages(int taskCount)
		{
			int count = itemIconList.Count;
			if (count == taskCount)
			{
				return count;
			}
			UIUtils.SafeDestoryViews(itemIconList);
			for (int i = 0; i < taskCount; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(itemViewPrefab, Vector3.zero, Quaternion.identity) as GameObject;
				if (!(gameObject == null))
				{
					MasterPlanComponentItemView component = gameObject.GetComponent<MasterPlanComponentItemView>();
					if (!(component == null))
					{
						gameObject.transform.SetParent(itemPanel, false);
						itemIconList.Add(component);
					}
				}
			}
			return itemIconList.Count;
		}
	}
}
