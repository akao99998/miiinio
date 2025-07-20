using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.UI.View
{
	public class ItemListMediator : UIStackMediator<ItemListView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ItemListMediator") as IKampaiLogger;

		private GameObject dragIcon;

		private bool placingNonIconBuilding;

		private bool placingIconBuilding;

		private bool dragingLockedItem = true;

		private ScrollRect scrollRect;

		private bool isDragging;

		private Dictionary<StoreItemType, StoreTab> storeTabs;

		private bool isSubMenuOpen;

		private Queue<Vector2> HorizontalDrag;

		private PurchaseNewBuildingSignal purchaseNewBuildingSignal;

		private CreateInventoryBuildingSignal createInventoryBuildingSignal;

		private PostMinionPartyStartSignal postMinionPartyStartSignal;

		private PostMinionPartyEndSignal postMinionPartyEndSignal;

		private StoreItemType lastPickedType;

		[Inject]
		public BuildMenuDefinitionLoadedSignal defLoadedSignal { get; set; }

		[Inject]
		public AddStoreTabSignal addTabSignal { get; set; }

		[Inject]
		public OnTabClickedSignal tabClickSignal { get; set; }

		[Inject]
		public MoveTabMenuSignal moveTabSignal { get; set; }

		[Inject]
		public MoveBuildMenuSignal moveBaseMenuSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal audioSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public BuildMenuOpenedSignal buildMenuOpened { get; set; }

		[Inject]
		public HighlightStoreItemSignal highlightStoreItemSignal { get; set; }

		[Inject]
		public DragFromStoreSignal dragFromStoreSignal { get; set; }

		[Inject]
		public HideStoreHighlightSignal hideHightlightSignal { get; set; }

		[Inject]
		public UpdateUIButtonsSignal updateStoreButtonsSignal { get; set; }

		[Inject]
		public UpdatePartyPointButtonsSignal updatePartyButtonsSignal { get; set; }

		[Inject]
		public IBuildMenuService buildMenuService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public MessageDialogClosed messageDialogClosed { get; set; }

		[Inject]
		public CancelPurchaseSignal cancelPurchaseSignal { get; set; }

		[Inject]
		public AppPauseSignal pauseSignal { get; set; }

		[Inject]
		public HighlightTabSignal highlightTabSignal { get; set; }

		[Inject]
		public UIModel uimodel { get; set; }

		[Inject]
		public SendBuildingToInventorySignal sendBuildingToInventorySignal { get; set; }

		[Inject]
		public ToggleStoreTabSignal toggleStoreTabSignal { get; set; }

		public override void OnRegister()
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			base.OnRegister();
			base.view.Init();
			defLoadedSignal.AddListener(OnDefinitionLoaded);
			updateStoreButtonsSignal.AddListener(UpdateStoreButtons);
			updatePartyButtonsSignal.AddListener(UpdatePartyPointButtons);
			addTabSignal.AddListener(AddStoreTab);
			highlightTabSignal.AddListener(HighlightStoreTab);
			tabClickSignal.AddListener(OnTabClicked);
			buildMenuOpened.AddListener(OnBuildMenuOpened);
			highlightStoreItemSignal.AddListener(HighlightStoreItem);
			hideHightlightSignal.AddListener(OnHideHighlight);
			base.view.Title.ClickedSignal.AddListener(OnItemMenuTitleClicked);
			postMinionPartyStartSignal = injectionBinder.GetInstance<PostMinionPartyStartSignal>();
			postMinionPartyStartSignal.AddListener(UpdatePartyPointButtons);
			postMinionPartyEndSignal = injectionBinder.GetInstance<PostMinionPartyEndSignal>();
			postMinionPartyEndSignal.AddListener(UpdatePartyPointButtons);
			purchaseNewBuildingSignal = injectionBinder.GetInstance<PurchaseNewBuildingSignal>();
			purchaseNewBuildingSignal.AddListener(NewBuildingPurchased);
			createInventoryBuildingSignal = injectionBinder.GetInstance<CreateInventoryBuildingSignal>();
			createInventoryBuildingSignal.AddListener(BuildingDraggedFromInventory);
			sendBuildingToInventorySignal.AddListener(BuildingSentToInventory);
			scrollRect = base.view.ScrollViewParent.parent.GetComponent<ScrollRect>();
			storeTabs = new Dictionary<StoreItemType, StoreTab>();
			HorizontalDrag = new Queue<Vector2>();
			moveBaseMenuSignal.AddListener(BaseMenuMoved);
			pauseSignal.AddListener(ResetIconDragState);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			defLoadedSignal.RemoveListener(OnDefinitionLoaded);
			updateStoreButtonsSignal.RemoveListener(UpdateStoreButtons);
			updatePartyButtonsSignal.RemoveListener(UpdatePartyPointButtons);
			addTabSignal.RemoveListener(AddStoreTab);
			tabClickSignal.RemoveListener(OnTabClicked);
			highlightTabSignal.RemoveListener(HighlightStoreTab);
			buildMenuOpened.RemoveListener(OnBuildMenuOpened);
			highlightStoreItemSignal.RemoveListener(HighlightStoreItem);
			hideHightlightSignal.RemoveListener(OnHideHighlight);
			base.view.Title.ClickedSignal.RemoveListener(OnItemMenuTitleClicked);
			postMinionPartyStartSignal.RemoveListener(UpdatePartyPointButtons);
			postMinionPartyEndSignal.RemoveListener(UpdatePartyPointButtons);
			purchaseNewBuildingSignal.RemoveListener(NewBuildingPurchased);
			createInventoryBuildingSignal.RemoveListener(BuildingDraggedFromInventory);
			sendBuildingToInventorySignal.RemoveListener(BuildingSentToInventory);
			moveBaseMenuSignal.RemoveListener(BaseMenuMoved);
			pauseSignal.RemoveListener(ResetIconDragState);
		}

		public void BaseMenuMoved(bool show)
		{
			uimodel.AllowMultiTouch = show;
			if (!show)
			{
				Close();
				ResetIconDragState();
			}
		}

		internal void OnDefinitionLoaded(Dictionary<StoreItemType, List<Definition>> storeMenuDefs)
		{
			StoreButtonView storeButtonView = null;
			foreach (KeyValuePair<StoreItemType, List<Definition>> storeMenuDef in storeMenuDefs)
			{
				StoreItemType key = storeMenuDef.Key;
				storeMenuDef.Value.Sort(delegate(Definition x, Definition y)
				{
					StoreItemDefinition storeItemDefinition2 = x as StoreItemDefinition;
					StoreItemDefinition storeItemDefinition3 = y as StoreItemDefinition;
					if (storeItemDefinition2 == null)
					{
						return 1;
					}
					if (storeItemDefinition3 == null)
					{
						return -1;
					}
					int levelItemUnlocksAt = definitionService.GetLevelItemUnlocksAt(storeItemDefinition2.ReferencedDefID);
					int levelItemUnlocksAt2 = definitionService.GetLevelItemUnlocksAt(storeItemDefinition3.ReferencedDefID);
					if (levelItemUnlocksAt < levelItemUnlocksAt2)
					{
						return -1;
					}
					if (levelItemUnlocksAt > levelItemUnlocksAt2)
					{
						return 1;
					}
					if (storeItemDefinition2.PriorityDefinition < storeItemDefinition3.PriorityDefinition)
					{
						return -1;
					}
					return (storeItemDefinition2.PriorityDefinition > storeItemDefinition3.PriorityDefinition) ? 1 : 0;
				});
				foreach (Definition item in storeMenuDef.Value)
				{
					StoreItemDefinition storeItemDefinition = item as StoreItemDefinition;
					if (storeItemDefinition == null)
					{
						continue;
					}
					Definition definition = definitionService.Get(storeItemDefinition.ReferencedDefID);
					if (definition != null)
					{
						TransactionDefinition transaction = definitionService.Get(storeItemDefinition.TransactionID) as TransactionDefinition;
						storeButtonView = base.view.GetStoreButtonViewByID(storeItemDefinition.ID);
						if (storeButtonView == null)
						{
							storeButtonView = StoreButtonBuilder.Build(definition, transaction, storeItemDefinition, base.view.ScrollViewParent, localService, definitionService, logger, playerService);
							storeButtonView.pointerDownSignal.AddListener(OnPointerDown);
							storeButtonView.pointerDragSignal.AddListener(OnPointerDrag);
							storeButtonView.pointerUpSignal.AddListener(OnPointerUp);
						}
						base.view.AddStoreButton(key, storeButtonView);
					}
				}
			}
			RectTransform rectTransform = storeButtonView.transform as RectTransform;
			float y2 = rectTransform.sizeDelta.y;
			float paddingInPixels = storeButtonView.PaddingInPixels;
			base.view.SetupButtonHeight(y2, paddingInPixels);
			buildMenuService.RetoreBuidMenuState(base.view.GetAllButtonViews());
		}

		private void SetDisplayOnStoreButtonViewCost(List<StoreButtonView> views, int buildingDefID)
		{
			foreach (StoreButtonView view in views)
			{
				if (view.definition.ID == buildingDefID)
				{
					view.DisplayOrHideUnlockedCostIcons();
					break;
				}
			}
		}

		private void NewBuildingPurchased(Building building)
		{
			int iD = building.Definition.ID;
			StoreItemType type = base.view.UpdateStoreButtonState(iD, true);
			if (buildMenuService.RemoveNewUnlockedItem(type, iD))
			{
				buildMenuService.RemoveUncheckedInventoryItem(type, iD);
			}
		}

		private void BuildingDraggedFromInventory(Building building, Location location)
		{
			int iD = building.Definition.ID;
			StoreItemType type = base.view.UpdateStoreButtonState(iD, true);
			if (buildMenuService.RemoveNewUnlockedItem(type, iD))
			{
				buildMenuService.RemoveUncheckedInventoryItem(type, iD);
			}
			List<StoreButtonView> storeButtonViews = base.view.GetStoreButtonViews(type);
			if (storeButtonViews != null)
			{
				SetDisplayOnStoreButtonViewCost(storeButtonViews, iD);
			}
		}

		private void BuildingSentToInventory(int buildingInstanceID)
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingInstanceID);
			int iD = byInstanceId.Definition.ID;
			StoreItemType storeItemType = base.view.UpdateStoreButtonState(iD, false);
			SetBadgeCountForStoreItemType(storeItemType);
			buildMenuService.AddUncheckedInventoryItem(storeItemType, iD);
			List<StoreButtonView> storeButtonViews = base.view.GetStoreButtonViews(storeItemType);
			if (storeButtonViews != null)
			{
				SetDisplayOnStoreButtonViewCost(storeButtonViews, iD);
			}
		}

		private void UpdateStoreButtons(bool clearUnlock)
		{
			if (clearUnlock)
			{
				buildMenuService.ClearAllNewUnlockItems();
			}
			IBuildMenuService obj = buildMenuService;
			bool updateBadge = clearUnlock;
			obj.UpdateNewUnlockList(base.view.GetAllButtonViews(), true, updateBadge);
			UpdatePartyPointButtons();
		}

		private void UpdatePartyPointButtons()
		{
			List<StoreButtonView> storeButtonViews = base.view.GetStoreButtonViews(StoreItemType.Decoration);
			List<StoreButtonView> storeButtonViews2 = base.view.GetStoreButtonViews(StoreItemType.Leisure);
			if (storeButtonViews != null)
			{
				foreach (StoreButtonView item in storeButtonViews)
				{
					item.UpdatePartyPointText(localService);
				}
			}
			if (storeButtonViews2 == null)
			{
				return;
			}
			foreach (StoreButtonView item2 in storeButtonViews2)
			{
				item2.UpdatePartyPointText(localService);
			}
		}

		internal void OnHideHighlight()
		{
			foreach (StoreItemType key in storeTabs.Keys)
			{
				List<StoreButtonView> storeButtonViews = base.view.GetStoreButtonViews(key);
				foreach (StoreButtonView item in storeButtonViews)
				{
					item.SetHighlight(false);
				}
			}
		}

		internal void OnItemMenuTitleClicked()
		{
			moveTabSignal.Dispatch(true);
			if (isSubMenuOpen)
			{
				audioSignal.Dispatch("Play_shop_pane_out_01");
			}
			isSubMenuOpen = false;
			base.view.MoveSubMenu(false);
		}

		protected override void Close()
		{
			OnItemMenuTitleClicked();
		}

		internal void AddStoreTab(StoreTab tab)
		{
			storeTabs.Add(tab.Type, tab);
		}

		internal void OnTabClicked(StoreItemType type, string localizedTitle)
		{
			base.view.TabIcon.maskSprite = StoreTabBuilder.SetTabIcon(type, logger);
			RectTransform rectTransform = scrollRect.content.transform as RectTransform;
			rectTransform.anchoredPosition = Vector2.zero;
			RefreshWhatButtonsShouldBeVisible(type);
			if (base.view.SetupItemMenu(type, localizedTitle))
			{
				moveTabSignal.Dispatch(false);
				audioSignal.Dispatch("Play_shop_pane_in_01");
				base.view.MoveSubMenu(true);
				isSubMenuOpen = true;
			}
			else
			{
				audioSignal.Dispatch("Play_action_locked_01");
			}
			lastPickedType = type;
			CheckPinataQuest();
		}

		private void CheckPinataQuest()
		{
			if (lastPickedType != StoreItemType.Leisure || !questService.GetQuestMap().ContainsKey(101120) || questService.GetQuestMap()[101120].State != QuestState.RunningTasks || playerService.GetInstancesByDefinitionID(3123).Count != 0)
			{
				return;
			}
			cancelPurchaseSignal.AddListener(OnCancelBuildingPlacement);
			popupMessageSignal.Dispatch(localService.GetString("BuildingHelperDialog"), PopupMessageType.AUTO_CLOSE_OVERRIDE);
			List<StoreButtonView> storeButtonViews = base.view.GetStoreButtonViews(StoreItemType.Leisure);
			foreach (StoreButtonView item in storeButtonViews)
			{
				if (item.definition.ID == 3123)
				{
					item.SetHighlight(true);
					break;
				}
			}
		}

		private void OnCancelBuildingPlacement(bool invalid)
		{
			cancelPurchaseSignal.RemoveListener(OnCancelBuildingPlacement);
			if (isSubMenuOpen && invalid)
			{
				messageDialogClosed.AddListener(OnMessageDialogClosed);
			}
		}

		private void OnMessageDialogClosed()
		{
			messageDialogClosed.RemoveListener(OnMessageDialogClosed);
			if (isSubMenuOpen)
			{
				CheckPinataQuest();
			}
		}

		private void RefreshWhatButtonsShouldBeVisible(StoreItemType type)
		{
			List<StoreButtonView> storeButtonViews = base.view.GetStoreButtonViews(type);
			toggleStoreTabSignal.Dispatch(type, buildMenuService.ShowingAChild(storeButtonViews));
		}

		private void OnBuildMenuOpened()
		{
			foreach (StoreItemType key in storeTabs.Keys)
			{
				SetBadgeCountForStoreItemType(key);
				RefreshWhatButtonsShouldBeVisible(key);
			}
			if (isSubMenuOpen)
			{
				base.view.RefreshStoreButtonLayout();
			}
		}

		internal void SetBadgeCountForStoreItemType(StoreItemType type)
		{
			List<StoreButtonView> storeButtonViews = base.view.GetStoreButtonViews(type);
			foreach (StoreButtonView item in storeButtonViews)
			{
				int iD = item.definition.ID;
				int inventoryCountByDefinitionID = playerService.GetInventoryCountByDefinitionID(iD);
				item.SetBadge(inventoryCountByDefinitionID);
			}
		}

		protected override void OnCloseAllMenu(GameObject exception)
		{
			ResetIconDragState();
		}

		private void ResetIconDragState()
		{
			if (dragIcon != null)
			{
				base.view.Title.ClickedSignal.AddListener(OnItemMenuTitleClicked);
				isDragging = false;
				UnityEngine.Object.Destroy(dragIcon);
				dragIcon = null;
				OnHideHighlight();
			}
		}

		internal void HighlightStoreItem(StoreItemDefinition definition, HighlightType type)
		{
			if (!storeTabs.ContainsKey(definition.Type))
			{
				return;
			}
			StoreTab storeTab = storeTabs[definition.Type];
			OnTabClicked(storeTab.Type, storeTab.LocalizedName);
			List<StoreButtonView> storeButtonViews = base.view.GetStoreButtonViews(definition.Type);
			float num = 0f;
			foreach (StoreButtonView item in storeButtonViews)
			{
				if (item.gameObject.activeSelf)
				{
					RectTransform rectTransform = item.transform as RectTransform;
					if (item.definition.ID == definition.ReferencedDefID)
					{
						item.SetHighlight(item.IsUnlocked(), type);
						RectTransform rectTransform2 = scrollRect.content.transform as RectTransform;
						rectTransform2.anchoredPosition = new Vector2(0f, num);
						break;
					}
					item.SetHighlight(false, type);
					num += rectTransform.rect.height + item.PaddingInPixels;
				}
			}
		}

		internal void HighlightStoreTab(StoreItemType type)
		{
			if (storeTabs.ContainsKey(type))
			{
				moveBaseMenuSignal.Dispatch(true);
				StoreTab storeTab = storeTabs[type];
				OnTabClicked(storeTab.Type, storeTab.LocalizedName);
			}
		}

		private void OnPointerDown(PointerEventData eventData, Definition definition, TransactionDefinition transactionDef, bool canPurchase)
		{
			placingNonIconBuilding = false;
			placingIconBuilding = false;
			if (Input.touchCount > 1)
			{
				if (dragIcon != null)
				{
					UnityEngine.Object.Destroy(dragIcon);
					dragIcon = null;
					base.view.Title.ClickedSignal.AddListener(OnItemMenuTitleClicked);
				}
				return;
			}
			isDragging = true;
			if (!canPurchase)
			{
				audioSignal.Dispatch("Play_action_locked_01");
				dragIcon = null;
				scrollRect.OnBeginDrag(eventData);
				dragingLockedItem = true;
				return;
			}
			audioSignal.Dispatch("Play_button_click_01");
			dragingLockedItem = false;
			if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.name.CompareTo("img_ItemIcon") == 0 && dragIcon == null)
			{
				popoutIcon(definition, eventData);
			}
			else
			{
				scrollRect.OnBeginDrag(eventData);
			}
		}

		private void popoutIcon(Definition definition, PointerEventData eventData)
		{
			DisplayableDefinition displayableDefinition = definition as DisplayableDefinition;
			base.view.Title.ClickedSignal.RemoveListener(OnItemMenuTitleClicked);
			dragIcon = new GameObject("DragIcon");
			dragIcon.transform.SetParent(glassCanvas.transform);
			dragIcon.layer = 5;
			KampaiIngoreRaycastImage kampaiIngoreRaycastImage = dragIcon.AddComponent<KampaiIngoreRaycastImage>();
			kampaiIngoreRaycastImage.sprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Image);
			kampaiIngoreRaycastImage.material = KampaiResources.Load<Material>("CircleIconAlphaMaskMat");
			kampaiIngoreRaycastImage.maskSprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Mask);
			RectTransform component = dragIcon.GetComponent<RectTransform>();
			component.localPosition = new Vector3(0f, 0f, 0f);
			KampaiImage component2 = dragIcon.GetComponent<KampaiImage>();
			component.anchoredPosition = new Vector2(eventData.position.x / UIUtils.GetHeightScale(), eventData.position.y / UIUtils.GetHeightScale() + component2.sprite.rect.height * component2.pixelsPerUnit / 2f);
			component.localScale = Vector3.one;
			component.anchorMin = new Vector2(0f, 0f);
			component.anchorMax = new Vector2(0f, 0f);
			component.pivot = new Vector2(0.5f, 0.5f);
			float num = Mathf.Max(component2.sprite.rect.width, component2.sprite.rect.height);
			component.sizeDelta = new Vector2(component2.sprite.rect.width / num * 100f, component2.sprite.rect.height / num * 100f);
		}

		private void OnPointerDrag(PointerEventData eventData, Definition definition, TransactionDefinition transactionDef, int badgeCount)
		{
			if (isDragging && dragIcon == null)
			{
				scrollRect.OnDrag(eventData);
			}
			if (dragingLockedItem)
			{
				return;
			}
			int num = 10;
			float num2 = 0.3f;
			float num3 = 3f;
			if (dragIcon == null)
			{
				if (HorizontalDrag.Count >= num)
				{
					HorizontalDrag.Dequeue();
				}
				HorizontalDrag.Enqueue(eventData.position);
				float num4 = 0f;
				float num5 = 0f;
				Vector2[] array = HorizontalDrag.ToArray();
				for (int i = 0; i < HorizontalDrag.Count; i++)
				{
					if (i != 0)
					{
						num5 += array[i].x - array[i - 1].x;
						num4 += array[i].y - array[i - 1].y;
					}
				}
				float value = ((!(num5 <= 0.001f)) ? (num4 / num5) : 100f);
				if (Math.Abs(value) <= num2 && num5 >= num3 && !placingIconBuilding && !placingNonIconBuilding)
				{
					popoutIcon(definition, eventData);
				}
				if (eventData.pointerCurrentRaycast.gameObject == null && !placingNonIconBuilding && !dragingLockedItem && dragIcon == null && !placingIconBuilding)
				{
					isDragging = false;
					scrollRect.OnEndDrag(eventData);
					dragFromStoreSignal.Dispatch(definition, transactionDef, eventData.position, true);
					moveBaseMenuSignal.Dispatch(false);
					placingNonIconBuilding = true;
				}
			}
			else if (eventData.pointerCurrentRaycast.gameObject == null)
			{
				if (dragIcon != null && !placingIconBuilding && !placingNonIconBuilding)
				{
					base.view.Title.ClickedSignal.AddListener(OnItemMenuTitleClicked);
					isDragging = false;
					UnityEngine.Object.Destroy(dragIcon);
					dragIcon = null;
					placingIconBuilding = true;
					scrollRect.OnEndDrag(eventData);
					moveBaseMenuSignal.Dispatch(false);
					dragFromStoreSignal.Dispatch(definition, transactionDef, eventData.position, true);
				}
			}
			else if (dragIcon != null)
			{
				setDragIconPosition(eventData);
			}
		}

		private void setDragIconPosition(PointerEventData eventData)
		{
			RectTransform component = dragIcon.GetComponent<RectTransform>();
			component.anchoredPosition = eventData.position / UIUtils.GetHeightScale();
			KampaiImage component2 = dragIcon.GetComponent<KampaiImage>();
			component.anchoredPosition = new Vector2(component.anchoredPosition.x, component.anchoredPosition.y + component2.sprite.rect.height * component2.pixelsPerUnit / 2f);
		}

		private void OnPointerUp(PointerEventData eventData, Definition definition, TransactionDefinition transactionDef)
		{
			dragingLockedItem = true;
			placingIconBuilding = false;
			placingNonIconBuilding = false;
			HorizontalDrag = new Queue<Vector2>();
			if (dragIcon != null)
			{
				base.view.Title.ClickedSignal.AddListener(OnItemMenuTitleClicked);
				isDragging = false;
				UnityEngine.Object.Destroy(dragIcon);
				dragIcon = null;
			}
			else
			{
				scrollRect.OnEndDrag(eventData);
			}
		}
	}
}
