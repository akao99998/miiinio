using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.UI.View
{
	public class CraftingModalMediator : UIStackMediator<CraftingModalView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CraftingModalMediator") as IKampaiLogger;

		private List<CraftingBuilding> validCraftingBuildingList = new List<CraftingBuilding>();

		private CameraAutoPanCompleteSignal cameraAutoPanCompleteSignal;

		private EndPartyBuffTimerSignal endPartyBuffTimerSignal;

		private ToggleVignetteSignal toggleVignetteSignal;

		private int currentIndex;

		private int craftingBuildingCount;

		private readonly AdPlacementName adPlacementName = AdPlacementName.CRAFTING;

		private AdPlacementInstance adPlacementInstance;

		private IEnumerator updateAdButtonCoroutine;

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public RefreshQueueSlotSignal refreshSignal { get; set; }

		[Inject]
		public CraftingModalParams craftingModalParams { get; set; }

		[Inject]
		public CraftingModalClosedSignal closedSignal { get; set; }

		[Inject]
		public CraftingQueuePositionUpdateSignal queuePositionSignal { get; set; }

		[Inject]
		public HideHUDAndIconsSignal hideHUDSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public HideItemPopupSignal closeItemPopupSignal { get; set; }

		[Inject]
		public ResetDoubleTapSignal resetDoubleTapSignal { get; set; }

		[Inject]
		public UIModel model { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllMessageDialogs { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public UpdateQueueIcon updateQueueSignal { get; set; }

		[Inject]
		public GoToResourceButtonClickedSignal gotoSignal { get; set; }

		[Inject]
		public IGoToService gotoService { get; set; }

		[Inject]
		public IRewardedAdService rewardedAdService { get; set; }

		[Inject]
		public RewardedAdRewardSignal rewardedAdRewardSignal { get; set; }

		[Inject]
		public AdPlacementActivityStateChangedSignal adPlacementActivityStateChangedSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		public override void OnRegister()
		{
			model.CraftingUIOpen = true;
			if (craftingModalParams.highlight)
			{
				craftingModalParams.highlight = false;
				base.view.highlightItem = true;
				base.view.higlightItemId = craftingModalParams.itemId;
			}
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.backArrow.ClickedSignal.AddListener(BackArrow);
			base.view.forwardArrow.ClickedSignal.AddListener(ForwardArrow);
			base.view.freeRush.ClickedSignal.AddListener(FreeRush);
			refreshSignal.AddListener(RefreshQueue);
			queuePositionSignal.AddListener(UpdateQueuePosition);
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			cameraAutoPanCompleteSignal = injectionBinder.GetInstance<CameraAutoPanCompleteSignal>();
			endPartyBuffTimerSignal = injectionBinder.GetInstance<EndPartyBuffTimerSignal>();
			toggleVignetteSignal = injectionBinder.GetInstance<ToggleVignetteSignal>();
			cameraAutoPanCompleteSignal.AddListener(PanComplete);
			endPartyBuffTimerSignal.AddListener(MinionPartyEnded);
			toggleVignetteSignal.Dispatch(true, null);
			resetDoubleTapSignal.AddListener(ResetDoubleTap);
			updateQueueSignal.AddListener(OnUpdateQueue);
			rewardedAdRewardSignal.AddListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.AddListener(OnAdPlacementActivityStateChanged);
			buildingChangeStateSignal.AddListener(OnBuildingChangeState);
			hideHUDSignal.Dispatch(false);
			gotoSignal.AddListener(GotoResourceBuilding);
		}

		public override void OnRemove()
		{
			model.CraftingUIOpen = false;
			base.OnRemove();
			if (base.view.buildingObject != null)
			{
				base.view.buildingObject.DisableHighLightBuilding();
			}
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.backArrow.ClickedSignal.RemoveListener(BackArrow);
			base.view.forwardArrow.ClickedSignal.RemoveListener(ForwardArrow);
			base.view.freeRush.ClickedSignal.RemoveListener(FreeRush);
			refreshSignal.RemoveListener(RefreshQueue);
			queuePositionSignal.RemoveListener(UpdateQueuePosition);
			cameraAutoPanCompleteSignal.RemoveListener(PanComplete);
			endPartyBuffTimerSignal.RemoveListener(MinionPartyEnded);
			resetDoubleTapSignal.RemoveListener(ResetDoubleTap);
			updateQueueSignal.RemoveListener(OnUpdateQueue);
			rewardedAdRewardSignal.RemoveListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.RemoveListener(OnAdPlacementActivityStateChanged);
			buildingChangeStateSignal.RemoveListener(OnBuildingChangeState);
			if (updateAdButtonCoroutine != null)
			{
				StopCoroutine(updateAdButtonCoroutine);
				updateAdButtonCoroutine = null;
			}
			gotoSignal.RemoveListener(GotoResourceBuilding);
		}

		public override void Initialize(GUIArguments args)
		{
			CraftingBuilding craftingBuilding = args.Get<CraftingBuilding>();
			GameObject gameObject = gameContext.injectionBinder.GetInstance(typeof(GameObject), GameElement.BUILDING_MANAGER) as GameObject;
			BuildingManagerView component = gameObject.GetComponent<BuildingManagerView>();
			CraftableBuildingObject buildingObject = component.GetBuildingObject(craftingBuilding.ID) as CraftableBuildingObject;
			if (craftingBuilding != null && craftingBuilding.State == BuildingState.Construction)
			{
				OnMenuClose();
			}
			else
			{
				int num = args.Get<int>();
				Init(num, buildingObject);
				InitCraftingBuildingList(num);
				CheckPartyState();
			}
			ShowCraftingInstruction();
		}

		private void ShowCraftingInstruction()
		{
			if (!localPersistService.HasKey("didyouknow_Crafting"))
			{
				popupMessageSignal.Dispatch(localService.GetString("CraftingDescription"), PopupMessageType.AUTO_CLOSE_OVERRIDE);
				localPersistService.PutDataInt("didyouknow_Crafting", 1);
			}
		}

		private void CheckPartyState()
		{
			float currentBuffMultiplierForBuffType = guestService.GetCurrentBuffMultiplierForBuffType(BuffType.PRODUCTION);
			base.view.SetPartyInfo(currentBuffMultiplierForBuffType, localService.GetString("partyBuffMultiplier", currentBuffMultiplierForBuffType), playerService.GetMinionPartyInstance().IsBuffHappening);
		}

		private void MinionPartyEnded(int buffDuration)
		{
			base.view.SetPartyInfo(1f, string.Empty, false);
		}

		protected override void Close()
		{
			closeItemPopupSignal.Dispatch();
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			base.view.Close();
			toggleVignetteSignal.Dispatch(false, null);
			hideHUDSignal.Dispatch(true);
			closeAllMessageDialogs.Dispatch();
		}

		private void RefreshQueue(bool purchasing)
		{
			if (purchasing)
			{
				playerService.BuyCraftingSlot(base.view.building.ID);
			}
			base.view.RefreshQueue();
			base.view.SetChildrenAsPartying();
		}

		private void UpdateQueuePosition()
		{
			base.view.UpdateQueuePosition();
			closeAllMessageDialogs.Dispatch();
		}

		private void BackArrow()
		{
			if (currentIndex <= 0)
			{
				currentIndex = craftingBuildingCount - 1;
			}
			else
			{
				currentIndex--;
			}
			OpenBuildingMenu();
		}

		private void PopulateBuilding(CraftingBuilding building)
		{
			GameObject gameObject = gameContext.injectionBinder.GetInstance(typeof(GameObject), GameElement.BUILDING_MANAGER) as GameObject;
			BuildingManagerView component = gameObject.GetComponent<BuildingManagerView>();
			CraftableBuildingObject buildingObject = component.GetBuildingObject(building.ID) as CraftableBuildingObject;
			base.view.RePopulateModal(building, buildingObject, HighlightType.DRAG);
			base.view.SetChildrenAsPartying();
			base.view.SetTitle(localService.GetString(building.Definition.LocalizedKey));
		}

		private void OpenBuildingMenu()
		{
			base.view.SetArrowButtonState(false);
			CraftingBuilding building = validCraftingBuildingList[currentIndex];
			PopulateBuilding(building);
			PanAndShowBuildingMenu(building);
			ScheduleAdButtonUpdate();
		}

		private void PanComplete()
		{
			base.view.SetArrowButtonState(true);
			if (base.view.highlightItem)
			{
				base.view.ShowDragTutorial();
			}
		}

		private void ForwardArrow()
		{
			if (currentIndex >= craftingBuildingCount - 1)
			{
				currentIndex = 0;
			}
			else
			{
				currentIndex++;
			}
			OpenBuildingMenu();
		}

		private void FreeRush()
		{
			if (adPlacementInstance != null)
			{
				rewardedAdService.ShowRewardedVideo(adPlacementInstance);
			}
		}

		private void UpdateAdButton()
		{
			if (base.view.building != null)
			{
				CraftingBuilding building = base.view.building;
				int iD = building.ID;
				bool flag = rewardedAdService.IsPlacementActive(adPlacementName, iD);
				if (!flag)
				{
					logger.Debug("Ads: placement '{0}' for crafting building {1} is disabled.", adPlacementName, iD);
				}
				AdPlacementInstance placementInstance = rewardedAdService.GetPlacementInstance(adPlacementName, iD);
				bool enable = flag && !playerService.isStorageFull() && IsBuildingInProduction() && placementInstance != null;
				base.view.EnableRewardedAdRushButton(enable);
				adPlacementInstance = placementInstance;
			}
		}

		private void ScheduleAdButtonUpdate()
		{
			if (updateAdButtonCoroutine == null)
			{
				updateAdButtonCoroutine = UpdateAdButtonOnNextFrame();
				StartCoroutine(updateAdButtonCoroutine);
			}
		}

		private IEnumerator UpdateAdButtonOnNextFrame()
		{
			yield return null;
			UpdateAdButton();
			updateAdButtonCoroutine = null;
		}

		private int GetBuildingId()
		{
			int result = -1;
			if (base.view != null && base.view.building != null)
			{
				result = base.view.building.ID;
			}
			return result;
		}

		private void OnRewardedAdReward(AdPlacementInstance placement)
		{
			if (placement.PlacementInstanceId != GetBuildingId())
			{
				return;
			}
			CraftingQueueView firstQueueItem = base.view.GetFirstQueueItem();
			if (firstQueueItem != null && IsBuildingInProduction())
			{
				CraftingQueueMediator component = firstQueueItem.GetComponent<CraftingQueueMediator>();
				if (component != null)
				{
					component.Rush(0, false, false);
					rewardedAdService.RewardPlayer(null, placement);
					telemetryService.Send_Telemetry_EVT_AD_INTERACTION(placement.Definition.Name, firstQueueItem.itemDef, placement.RewardPerPeriodCount);
				}
			}
			adPlacementInstance = null;
		}

		private bool IsBuildingInProduction()
		{
			CraftingQueueView firstQueueItem = base.view.GetFirstQueueItem();
			if (firstQueueItem != null)
			{
				return firstQueueItem.inProduction;
			}
			return false;
		}

		private void OnAdPlacementActivityStateChanged(AdPlacementInstance placement, bool enabled)
		{
			ScheduleAdButtonUpdate();
		}

		private void OnBuildingChangeState(int buildingId, BuildingState buildingState)
		{
			if (GetBuildingId() == buildingId)
			{
				ScheduleAdButtonUpdate();
			}
		}

		private void PanAndShowBuildingMenu(Building building)
		{
			GameObject instance = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER);
			BuildingManagerView component = instance.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(building.ID);
			Vector3 position = buildingObject.transform.position;
			ScreenPosition screenPosition = building.Definition.ScreenPosition;
			gameContext.injectionBinder.GetInstance<CameraAutoMoveSignal>().Dispatch(position, new Boxed<ScreenPosition>(screenPosition), new CameraMovementSettings(CameraMovementSettings.Settings.KeepUIOpen, building, null), false);
		}

		private void InitCraftingBuildingList(int craftingBuildingID)
		{
			List<CraftingBuilding> instancesByType = playerService.GetInstancesByType<CraftingBuilding>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				CraftingBuilding craftingBuilding = instancesByType[i];
				if (craftingBuilding.State != BuildingState.Construction && craftingBuilding.State != BuildingState.Inventory && craftingBuilding.State != 0 && craftingBuilding.State != BuildingState.Cooldown && craftingBuilding.State != BuildingState.Complete)
				{
					if (craftingBuildingID == craftingBuilding.ID)
					{
						currentIndex = validCraftingBuildingList.Count;
					}
					validCraftingBuildingList.Add(craftingBuilding);
				}
			}
			craftingBuildingCount = validCraftingBuildingList.Count;
			if (craftingBuildingCount <= 1)
			{
				base.view.backArrow.gameObject.SetActive(false);
				base.view.forwardArrow.gameObject.SetActive(false);
			}
		}

		private void OnMenuClose()
		{
			closedSignal.Dispatch();
			hideSignal.Dispatch("CraftingSkrim");
			guiService.Execute(GUIOperation.Unload, "screen_CraftingMenu");
		}

		private void ResetDoubleTap(int id)
		{
			base.view.ResetDoubleTap(id);
		}

		private void Init(int buildingID, CraftableBuildingObject buildingObject)
		{
			CraftingBuilding byInstanceId = playerService.GetByInstanceId<CraftingBuilding>(buildingID);
			base.view.Init(playerService, definitionService, questService, byInstanceId, buildingObject);
			base.view.SetTitle(localService.GetString(byInstanceId.Definition.LocalizedKey));
			ScheduleAdButtonUpdate();
		}

		private void OnUpdateQueue()
		{
			ScheduleAdButtonUpdate();
			base.view.CleanupTweens();
		}

		private void GotoResourceBuilding(int itemDefinitionId)
		{
			CraftingBuilding craftingBuilding = validCraftingBuildingList[currentIndex];
			int buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(itemDefinitionId);
			if (buildingDefintionIDFromItemDefintionID == craftingBuilding.Definition.ID)
			{
				base.view.highlightItem = true;
				base.view.higlightItemId = itemDefinitionId;
				PopulateBuilding(craftingBuilding);
				base.view.ShowDragTutorial();
				return;
			}
			ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(buildingDefintionIDFromItemDefintionID);
			if (byDefinitionId.Count > 0 && byDefinitionId.First() is CraftingBuilding)
			{
				for (int i = 0; i < validCraftingBuildingList.Count; i++)
				{
					if (validCraftingBuildingList[i].Definition.ID == buildingDefintionIDFromItemDefintionID)
					{
						currentIndex = i;
						base.view.highlightItem = true;
						base.view.higlightItemId = itemDefinitionId;
						OpenBuildingMenu();
						return;
					}
				}
			}
			Close();
			gotoService.GoToBuildingFromItem(itemDefinitionId);
		}
	}
}
