using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.UI.View
{
	public class ResourceModalMediator : UIStackMediator<ResourceModalView>
	{
		private ModalSettings modalSettings = new ModalSettings();

		private List<ResourceBuilding> validResourceBuildingList = new List<ResourceBuilding>();

		private int currentIndex;

		private int resourceBuildingCount;

		private bool initialized;

		private CameraAutoPanCompleteSignal cameraAutoPanCompleteSignal;

		private EndPartyBuffTimerSignal endPartyBuffTimerSignal;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public UpdateSliderSignal updateSliderSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFX { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public UpdateUIButtonsSignal updateLevelSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public FTUEProgressSignal ftueSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeOtherMenusSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			ftueSignal.Dispatch();
			base.view.LeftArrow.ClickedSignal.AddListener(MoveToPreviousBuilding);
			base.view.RightArrow.ClickedSignal.AddListener(MoveToNextBuilding);
			updateSliderSignal.AddListener(UpdateDisplay);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			updateLevelSignal.AddListener(LevelUnlockItems);
			endPartyBuffTimerSignal = gameContext.injectionBinder.GetInstance<EndPartyBuffTimerSignal>();
			endPartyBuffTimerSignal.AddListener(MinionPartyEnded);
			cameraAutoPanCompleteSignal = gameContext.injectionBinder.GetInstance<CameraAutoPanCompleteSignal>();
			cameraAutoPanCompleteSignal.AddListener(PanComplete);
			uiContext.injectionBinder.GetInstance<HideAllResourceIconsSignal>().Dispatch();
		}

		public override void OnRemove()
		{
			base.OnRemove();
			updateSliderSignal.RemoveListener(UpdateDisplay);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			updateLevelSignal.RemoveListener(LevelUnlockItems);
			endPartyBuffTimerSignal.RemoveListener(MinionPartyEnded);
			cameraAutoPanCompleteSignal.RemoveListener(PanComplete);
			uiContext.injectionBinder.GetInstance<ShowAllResourceIconsSignal>().Dispatch();
		}

		public override void Initialize(GUIArguments args)
		{
			if (!initialized)
			{
				initialized = true;
				ResourceBuilding resourceBuilding = args.Get<ResourceBuilding>();
				modalSettings.enableRushButtons = !args.Contains<DisableRushButtons>();
				modalSettings.enableHarvestButtons = playerService.HasStorageBuilding();
				modalSettings.enableCallButtons = !args.Contains<DisableCallButtons>();
				modalSettings.enableLockedButtons = !args.Contains<DisableLockedButton>();
				modalSettings.enableRushThrob = args.Contains<ThrobRushButtons>();
				modalSettings.enableCallThrob = args.Contains<ThrobCallButtons>();
				modalSettings.enableLockedThrob = args.Contains<ThrobLockedButtons>();
				BuildingPopupPositionData buildingPopupPositionData = args.Get<BuildingPopupPositionData>();
				closeOtherMenusSignal.Dispatch(base.gameObject);
				Init(resourceBuilding, buildingPopupPositionData);
				InitResourceBuildingList(resourceBuilding);
				CheckPartyState();
			}
		}

		private void CheckPartyState()
		{
			float currentBuffMultiplierForBuffType = guestService.GetCurrentBuffMultiplierForBuffType(BuffType.PRODUCTION);
			base.view.SetPartyInfo(currentBuffMultiplierForBuffType, localService.GetString("partyBuffMultiplier", currentBuffMultiplierForBuffType), playerService.GetMinionPartyInstance().IsBuffHappening);
		}

		private void MinionPartyEnded(int duration)
		{
			if (initialized)
			{
				base.view.SetPartyInfo(1f, string.Empty, false);
			}
		}

		private void MoveToPreviousBuilding()
		{
			if (currentIndex <= 0)
			{
				currentIndex = resourceBuildingCount - 1;
			}
			else
			{
				currentIndex--;
			}
			OpenBuildingMenu();
		}

		private void OpenBuildingMenu()
		{
			base.view.ResetRushButtonsState();
			base.view.SetArrowButtonState(false);
			ResourceBuilding building = validResourceBuildingList[currentIndex];
			RecreateModal(building);
			PanAndShowBuildingMenu(building);
		}

		private void PanComplete()
		{
			base.view.SetArrowButtonState(true);
		}

		private void MoveToNextBuilding()
		{
			if (currentIndex >= resourceBuildingCount - 1)
			{
				currentIndex = 0;
			}
			else
			{
				currentIndex++;
			}
			OpenBuildingMenu();
		}

		private void PanAndShowBuildingMenu(Building building)
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			GameObject instance = injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER);
			BuildingManagerView component = instance.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(building.ID);
			Vector3 position = buildingObject.transform.position;
			ScreenPosition screenPosition = building.Definition.ScreenPosition;
			injectionBinder.GetInstance<CameraAutoMoveSignal>().Dispatch(position, new Boxed<ScreenPosition>(screenPosition), new CameraMovementSettings(CameraMovementSettings.Settings.KeepUIOpen, building, null), false);
			injectionBinder.GetInstance<ShowHiddenBuildingsSignal>().Dispatch();
		}

		private void InitResourceBuildingList(ResourceBuilding currentResourceBuilding)
		{
			List<ResourceBuilding> instancesByType = playerService.GetInstancesByType<ResourceBuilding>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				ResourceBuilding resourceBuilding = instancesByType[i];
				if (resourceBuilding.State != BuildingState.Construction && resourceBuilding.State != BuildingState.Inventory && resourceBuilding.State != 0 && resourceBuilding.State != BuildingState.Cooldown && resourceBuilding.State != BuildingState.Complete)
				{
					if (currentResourceBuilding.ID == resourceBuilding.ID)
					{
						currentIndex = validResourceBuildingList.Count;
					}
					validResourceBuildingList.Add(resourceBuilding);
				}
			}
			resourceBuildingCount = validResourceBuildingList.Count;
			if (resourceBuildingCount <= 1)
			{
				base.view.LeftArrow.gameObject.SetActive(false);
				base.view.RightArrow.gameObject.SetActive(false);
			}
		}

		private void Init(ResourceBuilding building, BuildingPopupPositionData buildingPopupPositionData)
		{
			if (building == null)
			{
				return;
			}
			List<Minion> list = new List<Minion>();
			foreach (int minion in building.MinionList)
			{
				if (!building.CompleteMinionQueue.Contains(minion))
				{
					list.Add(playerService.GetByInstanceId<Minion>(minion));
				}
			}
			base.view.Init(building, list, localService, definitionService, playerService, modalSettings, buildingPopupPositionData);
		}

		private void RecreateModal(ResourceBuilding building)
		{
			if (building == null)
			{
				return;
			}
			List<Minion> list = new List<Minion>();
			foreach (int minion in building.MinionList)
			{
				if (!building.CompleteMinionQueue.Contains(minion))
				{
					list.Add(playerService.GetByInstanceId<Minion>(minion));
				}
			}
			modalSettings.enableRushThrob = false;
			modalSettings.enableCallThrob = false;
			modalSettings.enableLockedThrob = false;
			base.view.RecreateModal(building, list, modalSettings);
		}

		protected override void Close()
		{
			globalSFX.Dispatch("Play_menu_disappear_01");
			base.view.Close();
		}

		private void OnMenuClose()
		{
			hideSignal.Dispatch("BuildingSkrim");
			guiService.Execute(GUIOperation.Unload, "screen_BaseResource");
		}

		private void UpdateDisplay()
		{
			base.view.UpdateDisplay();
		}

		private void LevelUnlockItems(bool clearUnlock)
		{
			uint quantity = playerService.GetQuantity(StaticItem.LEVEL_ID);
			base.view.LevelUpUnlock(quantity);
		}
	}
}
