using System.Collections.Generic;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MinionTaskCompleteCommand : Command
	{
		[Inject]
		public int minionId { get; set; }

		[Inject]
		public EjectMinionFromBuildingSignal ejectMinionFromBuildingSignal { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChange { get; set; }

		[Inject]
		public ToggleMinionRendererSignal toggleMinionSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public HarvestReadySignal harvestReadySignal { get; set; }

		[Inject]
		public StopGagAnimationSignal stopGagAnimationSignal { get; set; }

		[Inject]
		public ShowHarvestReadySignal showHarvestReadySignal { get; set; }

		[Inject]
		public AddMinionToTikiBarSignal addMinionToTikiBarSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		public override void Execute()
		{
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionId);
			if (byInstanceId != null)
			{
				byInstanceId.TaskDuration = 0;
				int buildingID = byInstanceId.BuildingID;
				TaskableBuilding byInstanceId2 = playerService.GetByInstanceId<TaskableBuilding>(buildingID);
				if (byInstanceId2 != null)
				{
					TaskCompleted(byInstanceId, byInstanceId2, buildingID);
				}
			}
		}

		private void TaskCompleted(Minion minion, TaskableBuilding building, int buildingId)
		{
			int utcTime = timeService.CurrentTime();
			ResourceBuilding resourceBuilding = building as ResourceBuilding;
			DebrisBuilding debrisBuilding = building as DebrisBuilding;
			if (resourceBuilding != null)
			{
				resourceBuilding.PrepareForHarvest(utcTime, minion.ID);
				CheckBonusItems(resourceBuilding, minion);
				minion.AlreadyRushed = false;
				ejectMinionFromBuildingSignal.Dispatch(building, minionId);
				minionStateChange.Dispatch(minionId, MinionState.Idle);
				toggleMinionSignal.Dispatch(minionId, true);
				stopGagAnimationSignal.Dispatch(buildingId);
				int prestigeId = minion.PrestigeId;
				if (prestigeId > 0)
				{
					TikiBarBuilding byInstanceId = playerService.GetByInstanceId<TikiBarBuilding>(313);
					Prestige byInstanceId2 = playerService.GetByInstanceId<Prestige>(prestigeId);
					int minionSlotIndex = byInstanceId.GetMinionSlotIndex(byInstanceId2.Definition.ID);
					if (minionSlotIndex != -1 && minionSlotIndex < 3)
					{
						addMinionToTikiBarSignal.Dispatch(byInstanceId, minion, byInstanceId2, minionSlotIndex);
					}
				}
			}
			else
			{
				if (debrisBuilding != null)
				{
					ejectMinionFromBuildingSignal.Dispatch(building, minionId);
					minionStateChange.Dispatch(minionId, MinionState.Idle);
					timeEventService.RemoveEvent(buildingId);
					return;
				}
				building.AddToCompletedMinions(minionId, utcTime);
				showHarvestReadySignal.Dispatch(new Tuple<int, int>(building.ID, minionId));
			}
			buildingChangeStateSignal.Dispatch(building.ID, BuildingState.Harvestable);
			harvestReadySignal.Dispatch(buildingId);
			timeEventService.RemoveEvent(buildingId);
		}

		public void CheckBonusItems(ResourceBuilding resourceBuilding, Minion minion)
		{
			int quantity = (int)playerService.GetQuantity(StaticItem.MYSTERY_BOXES_OPENED);
			if (quantity < 1)
			{
				return;
			}
			MinionBenefitLevelBandDefintion minionBenefitLevelBandDefintion = definitionService.Get<MinionBenefitLevelBandDefintion>(89898);
			if (quantity != 1 || resourceBuilding.Definition.ID == minionBenefitLevelBandDefintion.FirstBuildingId)
			{
				List<int> bonusMinionItems = resourceBuilding.BonusMinionItems;
				bool flag = false;
				MinionBenefitLevel minionBenefit = minionBenefitLevelBandDefintion.GetMinionBenefit(minion.Level);
				if (((quantity != 1) ? randomService.NextFloat() : 0f) < minionBenefit.doubleDropPercentage)
				{
					bonusMinionItems.Add(resourceBuilding.Definition.ItemId);
					flag = true;
				}
				if (((quantity != 1) ? randomService.NextFloat() : 0f) < minionBenefit.premiumDropPercentage)
				{
					bonusMinionItems.Add(1);
					flag = true;
				}
				if (((quantity != 1) ? randomService.NextFloat() : 0f) < minionBenefit.rareDropPercentage)
				{
					int iD = playerService.GetWeightedInstance(4000).NextPick(randomService).ID;
					bonusMinionItems.Add(iD);
					flag = true;
				}
				if (flag)
				{
					playerService.AlterQuantity(StaticItem.MYSTERY_BOXES_OPENED, 1);
				}
			}
		}
	}
}
