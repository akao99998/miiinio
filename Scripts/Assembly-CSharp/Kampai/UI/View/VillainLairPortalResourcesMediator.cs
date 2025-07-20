using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class VillainLairPortalResourcesMediator : UIStackMediator<VillainLairPortalResourcesView>
	{
		private int lairDefinitionID;

		private VillainLair currentLair;

		private VillainLairEntranceBuilding thisPortal;

		private List<VillainLairResourcePlot> resourcePlots;

		private ModalSettings modalSettings = new ModalSettings();

		private EndPartyBuffTimerSignal endPartyBuffTimerSignal;

		[Inject]
		public EnterVillainLairSignal enterVillainLairSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeOtherMenusSignal { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		[Inject]
		public UpdateSliderSignal updateSliderSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public UpdateVillainLairMenuViewSignal updateVillainLairMenuViewSignal { get; set; }

		[Inject]
		public DisplayDisco3DElements displayDisco3DElements { get; set; }

		[Inject]
		public VillainLairAssetsLoadedSignal villainLairAssetsLoadedSignal { get; set; }

		public override void OnRegister()
		{
			villainLairModel.isPortalResourceModalOpen = true;
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.enterLair.ClickedSignal.AddListener(EnterLair);
			villainLairAssetsLoadedSignal.AddListener(SetEnterLairButtonActive);
			updateSliderSignal.AddListener(UpdateDisplay);
			updateVillainLairMenuViewSignal.AddListener(ResetSlotStates);
			endPartyBuffTimerSignal = gameContext.injectionBinder.GetInstance<EndPartyBuffTimerSignal>();
			endPartyBuffTimerSignal.AddListener(MinionPartyEnded);
			base.OnRegister();
		}

		public override void OnRemove()
		{
			villainLairModel.isPortalResourceModalOpen = false;
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.enterLair.ClickedSignal.RemoveListener(EnterLair);
			villainLairAssetsLoadedSignal.RemoveListener(SetEnterLairButtonActive);
			updateSliderSignal.RemoveListener(UpdateDisplay);
			updateVillainLairMenuViewSignal.RemoveListener(ResetSlotStates);
			endPartyBuffTimerSignal.RemoveListener(MinionPartyEnded);
			base.OnRemove();
		}

		public override void Initialize(GUIArguments args)
		{
			lairDefinitionID = args.Get<int>();
			thisPortal = args.Get<VillainLairEntranceBuilding>();
			BuildingPopupPositionData buildingPopupPositionData = args.Get<BuildingPopupPositionData>();
			currentLair = playerService.GetFirstInstanceByDefinitionId<VillainLair>(lairDefinitionID);
			Initialize(buildingPopupPositionData);
		}

		private void Initialize(BuildingPopupPositionData buildingPopupPositionData)
		{
			resourcePlots = new List<VillainLairResourcePlot>();
			for (int i = 0; i < currentLair.resourcePlotInstanceIDs.Count; i++)
			{
				int id = currentLair.resourcePlotInstanceIDs[i];
				VillainLairResourcePlot byInstanceId = playerService.GetByInstanceId<VillainLairResourcePlot>(id);
				resourcePlots.Add(byInstanceId);
			}
			resourcePlots.Sort(delegate(VillainLairResourcePlot a, VillainLairResourcePlot b)
			{
				int num = a.State.CompareTo(b.State);
				if (a.State != BuildingState.Inaccessible && b.State != BuildingState.Inaccessible)
				{
					if (a.State == BuildingState.Idle)
					{
						return 1;
					}
					if (b.State == BuildingState.Idle)
					{
						return -1;
					}
					if (a.State == BuildingState.Working)
					{
						return 1;
					}
					if (b.State == BuildingState.Working)
					{
						return -1;
					}
				}
				return (num == 0) ? a.ID.CompareTo(b.ID) : num;
			});
			modalSettings.enableRushButtons = true;
			modalSettings.enableHarvestButtons = true;
			modalSettings.enableCallButtons = true;
			modalSettings.enableLockedButtons = true;
			modalSettings.enableRushThrob = false;
			modalSettings.enableCallThrob = false;
			modalSettings.enableLockedThrob = false;
			closeOtherMenusSignal.Dispatch(base.gameObject);
			base.view.Init(currentLair, resourcePlots, localizationService, definitionService, playerService, modalSettings, buildingPopupPositionData);
			SetEnterLairButtonActive(villainLairModel.areLairAssetsLoaded);
			CheckPartyState();
		}

		private void SetEnterLairButtonActive(bool active)
		{
			base.view.SetEnterLairButtonActive(active);
		}

		private void EnterLair()
		{
			Close();
			enterVillainLairSignal.Dispatch(thisPortal.VillainLairInstanceID, false);
			displayDisco3DElements.Dispatch(false);
		}

		protected override void Close()
		{
			base.view.Close();
		}

		private void OnMenuClose()
		{
			villainLairModel.isPortalResourceModalOpen = false;
			guiService.Execute(GUIOperation.Unload, "screen_Resource_LairPortal");
			hideSignal.Dispatch("VillainLairPortalSkrim");
		}

		private void MinionPartyEnded(int duration)
		{
			if (base.view != null)
			{
				base.view.SetPartyInfo(1f, string.Empty, false);
			}
		}

		private void CheckPartyState()
		{
			float currentBuffMultiplierForBuffType = guestService.GetCurrentBuffMultiplierForBuffType(BuffType.PRODUCTION);
			base.view.SetPartyInfo(currentBuffMultiplierForBuffType, localizationService.GetString("partyBuffMultiplier", currentBuffMultiplierForBuffType), playerService.GetMinionPartyInstance().IsBuffHappening);
		}

		private void UpdateDisplay()
		{
			if (thisPortal != null)
			{
				base.view.UpdateDisplay();
			}
		}

		private void ResetSlotStates()
		{
			if (thisPortal != null)
			{
				base.view.SetupModalInfo(resourcePlots, modalSettings);
			}
		}
	}
}
