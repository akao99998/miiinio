using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class TabMenuMediator : EventMediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("TabMenuMediator") as IKampaiLogger;

		private StoreItemType lastTabClicked = StoreItemType.GrindCurrency;

		[Inject]
		public TabMenuView view { get; set; }

		[Inject]
		public AddStoreTabSignal addTabSignal { get; set; }

		[Inject]
		public OnTabClickedSignal tabClickSignal { get; set; }

		[Inject]
		public MoveTabMenuSignal moveTabSignal { get; set; }

		[Inject]
		public SetBadgeForStoreTabSignal setBadgeForTabSignal { get; set; }

		[Inject]
		public SetNewUnlockForStoreTabSignal setNewUnlockForTabSignal { get; set; }

		[Inject]
		public IBuildMenuService buildMenuService { get; set; }

		[Inject]
		public ClearNewBuildTabCount clearNewBuildTabCount { get; set; }

		[Inject]
		public ToggleStoreTabSignal toggleStoreTabSignal { get; set; }

		[Inject]
		public RemoveUnlockForBuildMenuSignal removeUnlockForBuildMenuSignal { get; set; }

		[Inject]
		public SetNewUnlockForBuildMenuSignal setNewUnlockForBuildMenuSignal { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		public override void OnRegister()
		{
			view.Init(setNewUnlockForBuildMenuSignal, removeUnlockForBuildMenuSignal);
			addTabSignal.AddListener(AddStoreTab);
			moveTabSignal.AddListener(ShowMenu);
			setBadgeForTabSignal.AddListener(SetBadgeForTab);
			setNewUnlockForTabSignal.AddListener(SetUnlockForTab);
			clearNewBuildTabCount.AddListener(SetClearForTab);
			toggleStoreTabSignal.AddListener(ToggleStoreTab);
		}

		public override void OnRemove()
		{
			addTabSignal.RemoveListener(AddStoreTab);
			moveTabSignal.RemoveListener(ShowMenu);
			setBadgeForTabSignal.RemoveListener(SetBadgeForTab);
			setNewUnlockForTabSignal.RemoveListener(SetUnlockForTab);
			clearNewBuildTabCount.RemoveListener(SetClearForTab);
			toggleStoreTabSignal.RemoveListener(ToggleStoreTab);
		}

		internal void AddStoreTab(StoreTab tab)
		{
			StoreTabView storeTabView = StoreTabBuilder.Build(tab, view.ScrollViewParent.transform, logger);
			storeTabView.ClickedSignal.AddListener(OnTabClicked);
			RectTransform rectTransform = storeTabView.transform as RectTransform;
			view.AddStoreTab(storeTabView, rectTransform.sizeDelta.y, storeTabView.PaddingInPixel);
		}

		private void ToggleStoreTab(StoreItemType type, bool show)
		{
			view.ToggleStoreTab(type, show);
		}

		internal void SetBadgeForTab(StoreItemType type, int badgeCount)
		{
			view.SetBadgeForStoreTab(type, badgeCount);
		}

		internal void SetUnlockForTab(StoreItemType type, int badgeCount)
		{
			view.SetUnlockForTab(type, badgeCount);
		}

		internal void SetClearForTab(StoreItemType type)
		{
			view.ClearUnlockForTab(type);
		}

		internal void ShowMenu(bool show)
		{
			if (show)
			{
				lastTabClicked = StoreItemType.GrindCurrency;
			}
			view.ShowMenu(show);
		}

		internal void OnTabClicked(StoreItemType type, string localizedTitle)
		{
			if (lastTabClicked != type)
			{
				lastTabClicked = type;
				tabClickSignal.Dispatch(type, localizedTitle);
				view.HideBadge(type);
				buildMenuService.ClearTab(type);
				uiModel.GoToInEffect = false;
			}
		}
	}
}
