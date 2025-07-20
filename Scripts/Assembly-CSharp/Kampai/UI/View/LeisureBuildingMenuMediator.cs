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
	public class LeisureBuildingMenuMediator : UIStackMediator<LeisureBuildingMenuView>
	{
		private int index;

		private LeisureBuilding building;

		private IList<LeisureBuilding> leisureBuildings;

		private ModalSettings modalSettings = new ModalSettings();

		private EndPartyBuffTimerSignal endPartyBuffTimerSignal;

		private BuildingManagerView buildingManagerView;

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public SendMinionToLeisureSignal sendMinionToLeisureSignal { get; set; }

		[Inject]
		public UpdateLeisureMenuViewSignal updateLeisureMenuViewSignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalSFX { get; set; }

		[Inject]
		public TryCollectLeisurePointsSignal tryCollectPoints { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		[Inject]
		public HarvestReadySignal harvestReadySignal { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		[Inject]
		public DisableLeisureRushButtonSignal disableLeisureRushButtonSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			updateLeisureMenuViewSignal.AddListener(UpdateView);
			disableLeisureRushButtonSignal.AddListener(DisableRushButton);
			base.view.CallMinions.ClickedSignal.AddListener(CallMinions);
			base.view.CollectPoints.ClickedSignal.AddListener(CollectPoints);
			base.view.RushMinions.ClickedSignal.AddListener(RushMinions);
			base.view.PrevBuilding.ClickedSignal.AddListener(PreviousBuilding);
			base.view.NextBuilding.ClickedSignal.AddListener(NextBuilding);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			endPartyBuffTimerSignal = injectionBinder.GetInstance<EndPartyBuffTimerSignal>();
			endPartyBuffTimerSignal.AddListener(MinionPartyEnded);
			injectionBinder.GetInstance<PostMinionPartyEndSignal>().AddListener(UpdateView);
			injectionBinder.GetInstance<IdleMinionSignal>().AddListener(base.view.SetIdleMinionCount);
			injectionBinder.GetInstance<CameraAutoPanCompleteSignal>().AddListener(PanComplete);
			uiModel.LeisureMenuOpen = true;
		}

		public override void OnRemove()
		{
			base.OnRemove();
			updateLeisureMenuViewSignal.RemoveListener(UpdateView);
			disableLeisureRushButtonSignal.RemoveListener(DisableRushButton);
			base.view.CallMinions.ClickedSignal.RemoveListener(CallMinions);
			base.view.CollectPoints.ClickedSignal.RemoveListener(CollectPoints);
			base.view.RushMinions.ClickedSignal.RemoveListener(RushMinions);
			base.view.PrevBuilding.ClickedSignal.RemoveListener(PreviousBuilding);
			base.view.NextBuilding.ClickedSignal.RemoveListener(NextBuilding);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			endPartyBuffTimerSignal.RemoveListener(MinionPartyEnded);
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			injectionBinder.GetInstance<PostMinionPartyEndSignal>().RemoveListener(UpdateView);
			injectionBinder.GetInstance<IdleMinionSignal>().RemoveListener(base.view.SetIdleMinionCount);
			injectionBinder.GetInstance<CameraAutoPanCompleteSignal>().RemoveListener(PanComplete);
			uiModel.LeisureMenuOpen = false;
		}

		public override void Initialize(GUIArguments args)
		{
			building = args.Get<LeisureBuilding>();
			leisureBuildings = playerService.GetInstancesByType<LeisureBuilding>();
			modalSettings.enableCallThrob = args.Contains<ThrobCallButtons>();
			modalSettings.enableRushThrob = args.Contains<ThrobRushButtons>();
			BuildingPopupPositionData positionData = args.Get<BuildingPopupPositionData>();
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			base.view.Init(localService, definitionService, playerService, timeEventService, injectionBinder.GetInstance<GameObject>(GameElement.MINION_MANAGER), positionData);
			int num = 0;
			for (int i = 0; i < leisureBuildings.Count; i++)
			{
				LeisureBuilding leisureBuilding = leisureBuildings[i];
				if (leisureBuilding.State == BuildingState.Inventory)
				{
					leisureBuildings.RemoveAt(i);
					i--;
					continue;
				}
				num++;
				if (building.ID == leisureBuilding.ID)
				{
					index = i;
				}
			}
			if (num <= 1)
			{
				base.view.SetArrowsActive(false);
			}
			GameObject instance = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER);
			buildingManagerView = instance.GetComponent<BuildingManagerView>();
			CheckPartyState();
			UpdateView();
		}

		protected override void Close()
		{
			hideSkrimSignal.Dispatch("BuildingSkrim");
			if (playGlobalSFX != null)
			{
				playGlobalSFX.Dispatch("Play_menu_disappear_01");
			}
			if (base.view != null)
			{
				base.view.Close();
			}
		}

		protected override void Update()
		{
			if (building != null && building.GetMinionsInBuilding() > 0)
			{
				base.view.SetClockTIme(building);
				base.view.SetRushCost(building);
				if (base.view.TimeRemaining == 0 && building.State == BuildingState.Working)
				{
					harvestReadySignal.Dispatch(building.ID);
					timeEventService.RemoveEvent(building.ID);
				}
			}
		}

		private void OnMenuClose()
		{
			base.view.Cleanup();
			guiService.Execute(GUIOperation.Unload, "screen_LeisureObject");
		}

		private void UpdateView()
		{
			if (building != null)
			{
				int partyPointsReward = building.Definition.PartyPointsReward;
				string time = UIUtils.FormatTime(building.Definition.LeisureTimeDuration, localService);
				base.view.SetTitle(building.Definition.LocalizedKey);
				if (playerService.IsMinionPartyUnlocked())
				{
					base.view.SetProduction("LeisureProduction", partyPointsReward, time);
				}
				else
				{
					base.view.SetProduction("LeisureProductionXP", partyPointsReward, time);
				}
				base.view.SetMinionsNeeded(building.Definition.WorkStations);
				base.view.SetRushCost(building);
				base.view.EnablePartyPoints(playerService.IsMinionPartyUnlocked());
				if (building.GetMinionsInBuilding() > 0 && building.State == BuildingState.Working)
				{
					base.view.EnableRush();
				}
				else if (building.UTCLastTaskingTimeStarted != 0 && building.State == BuildingState.Harvestable)
				{
					base.view.EnableCollect();
				}
				else
				{
					SetupCallButton();
				}
				base.view.SetIdleMinionCount();
				if (base.view.IsCallButtonEnabled())
				{
					base.view.Throb(base.view.CallMinions, modalSettings.enableCallThrob);
					base.view.Throb(base.view.RushMinions, modalSettings.enableRushThrob);
				}
			}
		}

		private void DisableRushButton()
		{
			base.view.DisableRushButton();
		}

		private void SetupCallButton()
		{
			base.view.EnableCallMinion();
			int highestMinionForLeisure = playerService.GetHighestMinionForLeisure(building.Definition.WorkStations);
			base.view.SetCallButtonInfo(highestMinionForLeisure);
		}

		private void CheckPartyState()
		{
			float currentBuffMultiplierForBuffType = guestService.GetCurrentBuffMultiplierForBuffType(BuffType.PARTY);
			base.view.SetPartyInfo(currentBuffMultiplierForBuffType, localService.GetString("partyBuffMultiplier", currentBuffMultiplierForBuffType), playerService.GetMinionPartyInstance().IsBuffHappening);
		}

		private void MinionPartyEnded(int duration)
		{
			base.view.SetPartyInfo(1f, string.Empty, false);
		}

		private void CollectPoints()
		{
			tryCollectPoints.Dispatch(building);
			UpdateView();
		}

		private void CallMinions()
		{
			modalSettings.enableCallThrob = false;
			sendMinionToLeisureSignal.Dispatch(building.ID);
			UpdateView();
		}

		private void RushMinions()
		{
			if (base.view.RushMinions.isDoubleConfirmed())
			{
				playerService.ProcessRush(base.view.rushCost, true, RushTransactionCallback, building.ID);
			}
			else if (modalSettings.enableRushButtons)
			{
				base.view.Throb(base.view.RushMinions, false);
				base.view.RushMinions.ShowConfirmMessage();
			}
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				playGlobalSFX.Dispatch("Play_button_premium_01");
				setPremiumCurrencySignal.Dispatch();
				modalSettings.enableRushThrob = false;
				if (timeEventService.HasEventID(building.ID))
				{
					timeEventService.RushEvent(building.ID);
				}
				else
				{
					gameContext.injectionBinder.GetInstance<HarvestReadySignal>().Dispatch(building.ID);
				}
			}
		}

		private void PreviousBuilding()
		{
			if (index <= 0)
			{
				index = leisureBuildings.Count - 1;
			}
			else
			{
				index--;
			}
			BeginPan();
		}

		private void NextBuilding()
		{
			if (index >= leisureBuildings.Count - 1)
			{
				index = 0;
			}
			else
			{
				index++;
			}
			BeginPan();
		}

		private void BeginPan()
		{
			building = leisureBuildings[index];
			base.view.SetArrowsInteractable(false);
			PanToBuilding(building);
			UpdateView();
		}

		private void PanToBuilding(Building building)
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			BuildingObject buildingObject = buildingManagerView.GetBuildingObject(building.ID);
			Vector3 position = buildingObject.transform.position;
			ScreenPosition screenPosition = building.Definition.ScreenPosition;
			injectionBinder.GetInstance<CameraAutoMoveSignal>().Dispatch(position, new Boxed<ScreenPosition>(screenPosition), new CameraMovementSettings(CameraMovementSettings.Settings.KeepUIOpen, building, null), false);
		}

		private void PanComplete()
		{
			base.view.SetArrowsInteractable(true);
		}
	}
}
