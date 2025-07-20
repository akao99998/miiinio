using System.Collections;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class StorageBuildingItemMediator : Mediator
	{
		private bool m_isPointerDown;

		private bool m_isBuyPanelOpened;

		private bool m_isDescriptionDelayed;

		private bool m_canShowSelectAnimation;

		private float itemInfoPopupOffset = -0.2f;

		private IEnumerator showInfoCoroutine;

		private IEnumerator autoCloseCoroutine;

		[Inject]
		public StorageBuildingItemView view { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public SetNewSellItemSignal sellItemSignal { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public AppPauseSignal pauseSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public RemoveStorageBuildingItemDescriptionSignal removeItemDescriptionSignal { get; set; }

		[Inject]
		public SelectStorageBuildingItemSignal selectStorageBuildingItemSignal { get; set; }

		[Inject]
		public EnableStorageBuildingItemDescriptionSignal enableItemDescriptionSignal { get; set; }

		[Inject]
		public OpenCreateNewSalePanelSignal openCreateNewSalePanelSignal { get; set; }

		[Inject]
		public CloseCreateNewSalePanelSignal closeCreateNewSalePanelSignal { get; set; }

		[Inject]
		public MarketplaceOpenSalePanelSignal openSalePanelSignal { get; set; }

		[Inject]
		public MarketplaceCloseSalePanelSignal closeSalePanelSignal { get; set; }

		[Inject]
		public MarketplaceOpenBuyPanelSignal openBuyPanelSignal { get; set; }

		[Inject]
		public MarketplaceCloseBuyPanelSignal closeBuyPanelSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			m_canShowSelectAnimation = false;
			openBuyPanelSignal.AddListener(BuyPanelOpened);
			closeBuyPanelSignal.AddListener(BuyPanelClosed);
			openSalePanelSignal.AddListener(SalePanelOpened);
			openCreateNewSalePanelSignal.AddListener(SalePanelOpened);
			closeSalePanelSignal.AddListener(SalePanelClosed);
			closeCreateNewSalePanelSignal.AddListener(CreateNewSalePanelClose);
			view.InfoButtonView.ClickedSignal.AddListener(OnItemClick);
			view.InfoButtonView.pointerDownSignal.AddListener(PointerDown);
			view.InfoButtonView.pointerUpSignal.AddListener(PointerUp);
			pauseSignal.AddListener(OnPause);
			removeItemDescriptionSignal.AddListener(OnItemInfoRemoved);
			selectStorageBuildingItemSignal.AddListener(IsItemSelected);
			enableItemDescriptionSignal.AddListener(EnableDelayOnInfoShow);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			openBuyPanelSignal.RemoveListener(BuyPanelOpened);
			closeBuyPanelSignal.RemoveListener(BuyPanelClosed);
			openSalePanelSignal.RemoveListener(SalePanelOpened);
			openCreateNewSalePanelSignal.RemoveListener(SalePanelOpened);
			closeSalePanelSignal.RemoveListener(SalePanelClosed);
			closeCreateNewSalePanelSignal.RemoveListener(CreateNewSalePanelClose);
			view.InfoButtonView.ClickedSignal.RemoveListener(OnItemClick);
			view.InfoButtonView.pointerDownSignal.RemoveListener(PointerDown);
			view.InfoButtonView.pointerUpSignal.RemoveListener(PointerUp);
			pauseSignal.RemoveListener(OnPause);
			removeItemDescriptionSignal.RemoveListener(OnItemInfoRemoved);
			selectStorageBuildingItemSignal.RemoveListener(IsItemSelected);
			enableItemDescriptionSignal.RemoveListener(EnableDelayOnInfoShow);
		}

		private void OnItemInfoRemoved()
		{
			if (autoCloseCoroutine != null)
			{
				StopCoroutine(autoCloseCoroutine);
				autoCloseCoroutine = null;
			}
		}

		private void EnableDelayOnInfoShow(bool isDelayed)
		{
			m_isDescriptionDelayed = isDelayed;
		}

		private void OnItemClick()
		{
			if (view.StorageItem != null || m_canShowSelectAnimation)
			{
				DynamicIngredientsDefinition dynamicIngredientsDefinition = view.StorageItem.Definition as DynamicIngredientsDefinition;
				if (dynamicIngredientsDefinition == null)
				{
					sellItemSignal.Dispatch(view.StorageItem.Definition.ID);
				}
			}
		}

		private void PointerDown()
		{
			if (!m_isBuyPanelOpened)
			{
				if (autoCloseCoroutine != null)
				{
					StopCoroutine(autoCloseCoroutine);
					autoCloseCoroutine = null;
				}
				m_isPointerDown = true;
				showInfoCoroutine = WaitToShowInfoView();
				StartCoroutine(showInfoCoroutine);
			}
		}

		private void IsItemSelected(int itemId)
		{
			if (!(view == null) && view.StorageItem != null)
			{
				view.SelectItem(m_canShowSelectAnimation && view.StorageItem.ID == itemId && HasOpenSlot());
			}
		}

		private void PointerUp()
		{
			if (m_isPointerDown)
			{
				if (showInfoCoroutine != null)
				{
					StopCoroutine(showInfoCoroutine);
					showInfoCoroutine = null;
				}
				m_isPointerDown = false;
				autoCloseCoroutine = CloseInfoView();
				StartCoroutine(autoCloseCoroutine);
			}
		}

		private IEnumerator CloseInfoView()
		{
			yield return new WaitForSeconds(1f);
			removeItemDescriptionSignal.Dispatch();
			autoCloseCoroutine = null;
		}

		private IEnumerator WaitToShowInfoView()
		{
			RectTransform rt = view.transform as RectTransform;
			if (!(rt == null))
			{
				Vector3[] corners = new Vector3[4];
				rt.GetWorldCorners(corners);
				Vector3 center = default(Vector3);
				Vector3[] array = corners;
				foreach (Vector3 corner in array)
				{
					center += corner;
				}
				center /= 4f;
				center += new Vector3(itemInfoPopupOffset, 0f, 0f);
				IGUICommand command = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "cmp_StorageItemInfo");
				GUIArguments args = command.Args;
				args.Add(typeof(ItemDefinition), view.StorageItem.Definition);
				args.Add(typeof(RectTransform), view.transform);
				args.Add(uiCamera.WorldToViewportPoint(center));
				if (m_isDescriptionDelayed)
				{
					yield return new WaitForSeconds(0.2f);
				}
				removeItemDescriptionSignal.Dispatch();
				if (m_isPointerDown)
				{
					soundFXSignal.Dispatch("Play_menu_popUp_02");
					guiService.Execute(command);
				}
			}
		}

		private void OnPause()
		{
			removeItemDescriptionSignal.Dispatch();
		}

		private bool HasOpenSlot()
		{
			return marketplaceService.GetNextAvailableSlot() != null;
		}

		private void CreateNewSalePanelClose()
		{
			IsItemSelected(0);
		}

		private void SalePanelClosed()
		{
			removeItemDescriptionSignal.Dispatch();
			m_canShowSelectAnimation = false;
			IsItemSelected(0);
		}

		private void SalePanelOpened(bool isInstant)
		{
			m_canShowSelectAnimation = true;
		}

		private void SalePanelOpened(int value)
		{
			m_canShowSelectAnimation = true;
		}

		private void BuyPanelOpened(bool isInstant)
		{
			m_isBuyPanelOpened = true;
		}

		private void BuyPanelClosed()
		{
			m_isBuyPanelOpened = false;
		}
	}
}
