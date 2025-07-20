using System;
using System.Collections;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RestoreBuildingCommand : Command
	{
		[Inject]
		public Building building { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public RestoreScaffoldingViewSignal restoreScaffoldingViewSignal { get; set; }

		[Inject]
		public RestoreRibbonViewSignal restoreRibbonViewSignal { get; set; }

		[Inject]
		public RestorePlatformViewSignal restorePlatformViewSignal { get; set; }

		[Inject]
		public RestoreBuildingViewSignal restoreBuildingViewSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public OrderBoardRefillTicketSignal refillTicketSignal { get; set; }

		[Inject]
		public OrderBoardSetNewTicketSignal setNewTicketSignal { get; set; }

		[Inject]
		public RestoreTaskableBuildingSignal restoreTaskingSignal { get; set; }

		[Inject]
		public RestoreCraftingBuildingsSignal craftingRestoreSignal { get; set; }

		[Inject]
		public RestoreLeisureBuildingSignal restoreLeisureBuildingSignal { get; set; }

		[Inject]
		public RestoreResourcePlotBuildingSignal restoreResourcePlotBuildingSignal { get; set; }

		[Inject]
		public CleanupDebrisSignal cleanupDebris { get; set; }

		[Inject]
		public ScheduleCooldownSignal scheduleCooldownSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		public override void Execute()
		{
			BuildingState state = building.State;
			int num = timeService.CurrentTime() - building.StateStartTime;
			VillainLairResourcePlot villainLairResourcePlot = building as VillainLairResourcePlot;
			if (villainLairResourcePlot != null)
			{
				restoreBuildingViewSignal.Dispatch(building);
				restoreResourcePlotBuildingSignal.Dispatch(villainLairResourcePlot);
			}
			else
			{
				switch (state)
				{
				case BuildingState.Inactive:
				case BuildingState.Construction:
					HandleInConstruction(num);
					break;
				case BuildingState.Complete:
					HandleCompletedConstruction();
					break;
				case BuildingState.Working:
				case BuildingState.Harvestable:
				case BuildingState.HarvestableAndWorking:
				{
					restoreBuildingViewSignal.Dispatch(building);
					TaskableBuilding taskableBuilding = building as TaskableBuilding;
					if (taskableBuilding != null)
					{
						restoreTaskingSignal.Dispatch(taskableBuilding);
					}
					CraftingBuilding craftingBuilding = building as CraftingBuilding;
					if (craftingBuilding != null)
					{
						craftingRestoreSignal.Dispatch(craftingBuilding);
					}
					LeisureBuilding leisureBuilding = building as LeisureBuilding;
					if (leisureBuilding != null)
					{
						restoreLeisureBuildingSignal.Dispatch(leisureBuilding);
					}
					break;
				}
				default:
					restoreBuildingViewSignal.Dispatch(building);
					break;
				case BuildingState.Inventory:
					break;
				}
			}
			if (building.GetType() == typeof(OrderBoard) && state != BuildingState.Broken && state != BuildingState.Complete && state != BuildingState.Construction && state != 0)
			{
				OrderBoard orderBoard = building as OrderBoard;
				routineRunner.StartCoroutine(WaitTwoFrames(delegate
				{
					RestoreBlackMarket(orderBoard);
				}));
			}
			CheckCooldownState(state, num);
			CleanupDebris();
		}

		private void HandleInConstruction(float passedTime)
		{
			BuildingDefinition definition = building.Definition;
			int num = definition.ConstructionTime;
			if (definition.IncrementalConstructionTime > 0)
			{
				num += (building.BuildingNumber - 1) * definition.IncrementalConstructionTime;
			}
			restoreBuildingViewSignal.Dispatch(building);
			restorePlatformViewSignal.Dispatch(building);
			if (passedTime > (float)num)
			{
				restoreScaffoldingViewSignal.Dispatch(building, false);
				restoreRibbonViewSignal.Dispatch(building);
				buildingChangeStateSignal.Dispatch(building.ID, BuildingState.Complete);
			}
			else
			{
				restoreScaffoldingViewSignal.Dispatch(building, true);
			}
		}

		private void HandleCompletedConstruction()
		{
			restoreBuildingViewSignal.Dispatch(building);
			restorePlatformViewSignal.Dispatch(building);
			restoreScaffoldingViewSignal.Dispatch(building, false);
			restoreRibbonViewSignal.Dispatch(building);
		}

		private void CleanupDebris()
		{
			DebrisBuilding debrisBuilding = building as DebrisBuilding;
			if (debrisBuilding != null && debrisBuilding.PaidInputCostToClear)
			{
				cleanupDebris.Dispatch(debrisBuilding.ID, false);
			}
		}

		private void CheckCooldownState(BuildingState buildingState, int passedTime)
		{
			if (buildingState != BuildingState.Cooldown || !(building is IBuildingWithCooldown))
			{
				return;
			}
			int cooldown = ((IBuildingWithCooldown)building).GetCooldown();
			if (passedTime >= cooldown)
			{
				buildingChangeStateSignal.Dispatch(building.ID, BuildingState.Idle);
				return;
			}
			bool second = false;
			MignetteBuilding mignetteBuilding = building as MignetteBuilding;
			if (mignetteBuilding != null)
			{
				second = true;
			}
			scheduleCooldownSignal.Dispatch(new Tuple<int, bool>(building.ID, second), false);
		}

		private void RestoreBlackMarket(OrderBoard blackMarketBuilding)
		{
			int num = 0;
			num = blackMarketBuilding.Definition.RefillTime;
			foreach (OrderBoardTicket ticket in blackMarketBuilding.tickets)
			{
				if (ticket.StartTime > 0)
				{
					int num2 = -ticket.BoardIndex;
					if (timeService.CurrentTime() >= ticket.StartTime + num)
					{
						setNewTicketSignal.Dispatch(num2, false);
					}
					else if (timeService.CurrentTime() < ticket.StartTime + num)
					{
						timeEventService.AddEvent(num2, ticket.StartTime, num, refillTicketSignal);
					}
				}
			}
		}

		private IEnumerator WaitTwoFrames(Action a)
		{
			yield return null;
			yield return null;
			a();
		}
	}
}
