using System;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.UI.View
{
	public class VillainLairResourceModalMediator : UIStackMediator<VillainLairResourceModalView>
	{
		private VillainLair lair;

		private List<VillainLairResourcePlot> unlockedPlots;

		private List<VillainLairResourcePlot> lockedPlots;

		private int currentIndex;

		private VillainLairResourcePlot currentPlot;

		private bool movingToLockedPlot;

		private IdleMinionSignal idleMinionSignal;

		private PostMinionPartyEndSignal postMinionPartyEndSignal;

		private EndPartyBuffTimerSignal endPartyBuffTimerSignal;

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalSFX { get; set; }

		[Inject]
		public HighlightBuildingSignal highlightBuildingSignal { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		[Inject]
		public SendMinionToLairResourcePlotSignal callMinionSignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public UITryHarvestSignal tryHarvestSignal { get; set; }

		[Inject]
		public AwardLairBonusDropsThenSetHarvestReadySignal awardDropsThenHarvestReadySignal { get; set; }

		[Inject]
		public UpdateVillainLairMenuViewSignal updateVillainLairMenuViewSignal { get; set; }

		[Inject]
		public CameraMoveToCustomLairPlotSignal cameraMoveToCustomLairPlotSignal { get; set; }

		[Inject]
		public CameraMoveToCustomPositionSignal cameraMoveToCustomPositionSignal { get; set; }

		[Inject]
		public OpenVillainLairResourcePlotBuildingSignal openResourcePlotBuildingSignal { get; set; }

		[Inject]
		public VillainLairModel model { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.Init();
			base.view.Setup();
			base.view.prevButton.ClickedSignal.AddListener(PreviousBuilding);
			base.view.nextButton.ClickedSignal.AddListener(NextBuilding);
			base.view.callMinionButton.ClickedSignal.AddListener(CallMinionClicked);
			base.view.collectButton.ClickedSignal.AddListener(CollectClicked);
			base.view.rushButton.ClickedSignal.AddListener(RushClicked);
			updateVillainLairMenuViewSignal.AddListener(UpdateView);
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			idleMinionSignal = injectionBinder.GetInstance<IdleMinionSignal>();
			idleMinionSignal.AddListener(CheckIdleMinionCount);
			postMinionPartyEndSignal = injectionBinder.GetInstance<PostMinionPartyEndSignal>();
			postMinionPartyEndSignal.AddListener(UpdateView);
			endPartyBuffTimerSignal = injectionBinder.GetInstance<EndPartyBuffTimerSignal>();
			endPartyBuffTimerSignal.AddListener(MinionPartyEnded);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.Open();
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.prevButton.ClickedSignal.RemoveListener(PreviousBuilding);
			base.view.nextButton.ClickedSignal.RemoveListener(NextBuilding);
			base.view.callMinionButton.ClickedSignal.RemoveListener(CallMinionClicked);
			base.view.collectButton.ClickedSignal.RemoveListener(CollectClicked);
			base.view.rushButton.ClickedSignal.RemoveListener(RushClicked);
			updateVillainLairMenuViewSignal.RemoveListener(UpdateView);
			idleMinionSignal.RemoveListener(CheckIdleMinionCount);
			postMinionPartyEndSignal.RemoveListener(UpdateView);
			endPartyBuffTimerSignal.RemoveListener(MinionPartyEnded);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
		}

		public override void Initialize(GUIArguments args)
		{
			currentPlot = args.Get<VillainLairResourcePlot>();
			lair = currentPlot.parentLair;
			unlockedPlots = new List<VillainLairResourcePlot>();
			lockedPlots = new List<VillainLairResourcePlot>();
			int num = 0;
			foreach (int resourcePlotInstanceID in lair.resourcePlotInstanceIDs)
			{
				VillainLairResourcePlot byInstanceId = playerService.GetByInstanceId<VillainLairResourcePlot>(resourcePlotInstanceID);
				if (byInstanceId.State != BuildingState.Inaccessible)
				{
					if (resourcePlotInstanceID == currentPlot.ID)
					{
						currentIndex = num;
					}
					unlockedPlots.Add(byInstanceId);
					num++;
				}
				else
				{
					lockedPlots.Add(byInstanceId);
				}
			}
			base.view.EnableArrows(true);
			currentPlot = unlockedPlots[currentIndex];
			base.view.SetResourcePlotTitle(localizationService.GetString(lair.Definition.ResourcePlots[currentPlot.indexInLairResourcePlots].descriptionKey));
			EnablePlotHighlight(true);
			SetLairResourceDescription();
			CheckPartyState();
			UpdateView();
		}

		protected override void Update()
		{
			if (currentPlot != null && currentPlot.MinionIsTaskedToBuilding() && SetClockTimeAndRushCost() == 0 && currentPlot.State == BuildingState.Working)
			{
				awardDropsThenHarvestReadySignal.Dispatch(currentPlot.ID);
				timeEventService.RemoveEvent(currentPlot.ID);
			}
		}

		protected override void Close()
		{
			if (playGlobalSFX != null)
			{
				playGlobalSFX.Dispatch("Play_menu_disappear_01");
			}
			if (!movingToLockedPlot)
			{
				cameraMoveToCustomPositionSignal.Dispatch(60017, new Boxed<Action>(null));
			}
			if (base.view != null)
			{
				base.view.Close();
			}
		}

		private void OnMenuClose()
		{
			EnablePlotHighlight(false);
			if (!movingToLockedPlot)
			{
				hideSkrimSignal.Dispatch("VillainLairResourceSkrim");
			}
			guiService.Execute(GUIOperation.Unload, "screen_Resource_Lair_Unlocked");
		}

		private void CheckPartyState()
		{
			float currentBuffMultiplierForBuffType = guestService.GetCurrentBuffMultiplierForBuffType(BuffType.PRODUCTION);
			base.view.SetPartyInfo(currentBuffMultiplierForBuffType, localizationService.GetString("partyBuffMultiplier", currentBuffMultiplierForBuffType), playerService.GetMinionPartyInstance().IsBuffHappening);
		}

		private void MinionPartyEnded(int duration)
		{
			base.view.SetPartyInfo(1f, string.Empty, false);
		}

		private void SetLairResourceDescription()
		{
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(lair.Definition.ResourceItemID);
			string @string = localizationService.GetString("ResourceProd", localizationService.GetString(itemDefinition.LocalizedKey, 1), UIUtils.FormatTime(lair.Definition.SecondsToHarvest, localizationService));
			SetResourceItemAmount();
			base.view.SetResourceDescription(itemDefinition, @string);
		}

		private void SetResourceItemAmount()
		{
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(lair.Definition.ResourceItemID);
			int quantityByDefinitionId = (int)playerService.GetQuantityByDefinitionId(itemDefinition.ID);
			base.view.SetResourceItemAmount(quantityByDefinitionId);
		}

		private void SelectResourcePlotAndUpdate()
		{
			currentPlot = unlockedPlots[currentIndex];
			base.view.SetResourcePlotTitle(localizationService.GetString(lair.Definition.ResourcePlots[currentPlot.indexInLairResourcePlots].descriptionKey));
			EnablePlotHighlight(true);
			UpdateView();
		}

		private void EnablePlotHighlight(bool enable)
		{
			if (currentPlot != null)
			{
				highlightBuildingSignal.Dispatch(currentPlot.ID, enable);
			}
		}

		private void PreviousBuilding()
		{
			ChangeBuilding(false);
		}

		private void NextBuilding()
		{
			ChangeBuilding(true);
		}

		private void ChangeBuilding(bool next)
		{
			if (model.cameraFlow != null && model.cameraFlow.state != GoTweenState.Complete)
			{
				return;
			}
			EnablePlotHighlight(false);
			if (next)
			{
				if (currentIndex >= unlockedPlots.Count - 1)
				{
					if (OpenLockedPlotInstead(0))
					{
						return;
					}
					currentIndex = 0;
				}
				else
				{
					currentIndex++;
				}
			}
			else if (currentIndex <= 0)
			{
				if (OpenLockedPlotInstead(lockedPlots.Count - 1))
				{
					return;
				}
				currentIndex = unlockedPlots.Count - 1;
			}
			else
			{
				currentIndex--;
			}
			SelectResourcePlotAndUpdate();
		}

		private bool OpenLockedPlotInstead(int index)
		{
			if (lockedPlots.Count > 0)
			{
				VillainLairResourcePlot type = lockedPlots[index];
				openResourcePlotBuildingSignal.Dispatch(type);
				movingToLockedPlot = true;
				Close();
				return true;
			}
			return false;
		}

		private void PanToCurrentBuilding()
		{
			Vector3 type = (Vector3)currentPlot.Location + GameConstants.LairResourcePlotCustomUIOffsets.position;
			cameraMoveToCustomLairPlotSignal.Dispatch(type);
		}

		private void CallMinionClicked()
		{
			if (currentPlot != null)
			{
				callMinionSignal.Dispatch(currentPlot.ID);
				UpdateView();
			}
		}

		private void CollectClicked()
		{
			tryHarvestSignal.Dispatch(currentPlot.ID, UpdateView, true);
		}

		private void RushClicked()
		{
			if (currentPlot != null)
			{
				if (base.view.rushButton.isDoubleConfirmed())
				{
					playerService.ProcessRush(base.view.rushPrice, true, RushTransactionCallBack, currentPlot.ID);
				}
				else
				{
					base.view.rushButton.ShowConfirmMessage();
				}
			}
		}

		private void RushTransactionCallBack(PendingCurrencyTransaction pct)
		{
			if (pct.Success && currentPlot != null)
			{
				playGlobalSFX.Dispatch("Play_button_premium_01");
				setPremiumCurrencySignal.Dispatch();
				if (timeEventService.HasEventID(currentPlot.ID))
				{
					timeEventService.RushEvent(currentPlot.ID);
				}
				else
				{
					awardDropsThenHarvestReadySignal.Dispatch(currentPlot.ID);
				}
			}
		}

		private int SetClockTimeAndRushCost()
		{
			if (currentPlot == null)
			{
				return -1;
			}
			int num = 0;
			int secondsToHarvest = lair.Definition.SecondsToHarvest;
			num = ((!timeEventService.HasEventID(currentPlot.ID)) ? secondsToHarvest : timeEventService.GetTimeRemaining(currentPlot.ID));
			num = Mathf.Min(num, secondsToHarvest);
			base.view.rushPrice = timeEventService.CalculateRushCostForTimer(num, RushActionType.HARVESTING);
			if (base.view.rushPrice == 0)
			{
				base.view.SetStateFreeRush();
			}
			base.view.SetClockTimeAndRushCost(UIUtils.FormatTime(num, localizationService), base.view.rushPrice.ToString());
			return num;
		}

		private void UpdateView()
		{
			if (currentPlot != null)
			{
				SetResourceItemAmount();
				PanToCurrentBuilding();
				if (currentPlot.State == BuildingState.Working && currentPlot.UTCLastTaskingTimeStarted != 0)
				{
					base.view.SetStateRush();
					return;
				}
				if (currentPlot.State == BuildingState.Harvestable && currentPlot.UTCLastTaskingTimeStarted != 0)
				{
					base.view.SetStateCollect();
					return;
				}
				base.view.SetStateCallMinion();
				base.view.SetMinionLevel(playerService);
				CheckIdleMinionCount();
			}
		}

		private void CheckIdleMinionCount()
		{
			int count = playerService.GetIdleMinions().Count;
			base.view.SetAvailableMinionInformation(count);
		}
	}
}
