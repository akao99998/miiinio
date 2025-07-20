using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class BuyMinionRushCommand : Command
	{
		private BuildingType.BuildingTypeIdentifier rushedBuildingType;

		[Inject]
		public int minionID { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalSFX { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public HarvestReadySignal harvestReadySignal { get; set; }

		[Inject]
		public AwardLairBonusDropsThenSetHarvestReadySignal awardDropsThenHarvestReadySignal { get; set; }

		public override void Execute()
		{
			int rushCost = MinionUtil.RushCost(minionID, playerService, timeEventService, definitionService);
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionID);
			Building byInstanceId2 = playerService.GetByInstanceId<Building>(byInstanceId.BuildingID);
			int instanceId = 0;
			switch (byInstanceId.State)
			{
			default:
				return;
			case MinionState.Leisure:
				rushedBuildingType = BuildingType.BuildingTypeIdentifier.LEISURE;
				instanceId = byInstanceId.BuildingID;
				break;
			case MinionState.Tasking:
			{
				if (byInstanceId2 is VillainLairResourcePlot)
				{
					rushedBuildingType = BuildingType.BuildingTypeIdentifier.LAIR_RESOURCEPLOT;
					instanceId = byInstanceId2.ID;
					break;
				}
				ResourceBuilding resourceBuilding = byInstanceId2 as ResourceBuilding;
				if (resourceBuilding != null)
				{
					rushedBuildingType = BuildingType.BuildingTypeIdentifier.RESOURCE;
					instanceId = resourceBuilding.Definition.ItemId;
				}
				break;
			}
			}
			playerService.ProcessRush(rushCost, true, RushTransactionCallback, instanceId);
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionID);
				playGlobalSFX.Dispatch("Play_button_premium_01");
				if (rushedBuildingType == BuildingType.BuildingTypeIdentifier.LAIR_RESOURCEPLOT || rushedBuildingType == BuildingType.BuildingTypeIdentifier.LEISURE)
				{
					LavaOrLeisureRush(byInstanceId.BuildingID, rushedBuildingType);
				}
				else if (rushedBuildingType == BuildingType.BuildingTypeIdentifier.RESOURCE)
				{
					timeEventService.RushEvent(minionID);
					bool alreadyRushed = playerService.GetByInstanceId<TaskableBuilding>(byInstanceId.BuildingID) != null;
					byInstanceId.AlreadyRushed = alreadyRushed;
				}
			}
		}

		private void LavaOrLeisureRush(int ID, BuildingType.BuildingTypeIdentifier type)
		{
			setPremiumCurrencySignal.Dispatch();
			if (timeEventService.HasEventID(ID))
			{
				timeEventService.RushEvent(ID);
				return;
			}
			switch (type)
			{
			case BuildingType.BuildingTypeIdentifier.LAIR_RESOURCEPLOT:
				awardDropsThenHarvestReadySignal.Dispatch(ID);
				break;
			case BuildingType.BuildingTypeIdentifier.LEISURE:
				harvestReadySignal.Dispatch(ID);
				break;
			}
		}
	}
}
