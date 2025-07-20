using Kampai.Common;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class AwardLairBonusDropsThenSetHarvestReadyCommand : Command
	{
		[Inject]
		public int buildingID { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public HarvestReadySignal harvestReadySignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingStateChangeSignal { get; set; }

		[Inject]
		public RemoveMinionFromLairResourcePlotSignal removeMinionFromLairResourcePlotSignal { get; set; }

		[Inject]
		public UpdateVillainLairMenuViewSignal updateVillainLairMenuViewSignal { get; set; }

		public override void Execute()
		{
			VillainLairResourcePlot byInstanceId = playerService.GetByInstanceId<VillainLairResourcePlot>(buildingID);
			Minion byInstanceId2 = playerService.GetByInstanceId<Minion>(byInstanceId.LastMinionTasked);
			removeMinionFromLairResourcePlotSignal.Dispatch(byInstanceId.ID);
			buildingStateChangeSignal.Dispatch(byInstanceId.ID, BuildingState.Harvestable);
			updateVillainLairMenuViewSignal.Dispatch();
			CheckBonusItems(byInstanceId, byInstanceId2);
			byInstanceId.harvestCount++;
			harvestReadySignal.Dispatch(byInstanceId.ID);
		}

		private void CheckBonusItems(VillainLairResourcePlot resourcePlot, Minion minion)
		{
			MinionBenefitLevelBandDefintion minionBenefitLevelBandDefintion = definitionService.Get<MinionBenefitLevelBandDefintion>(StaticItem.MINION_BENEFITS_DEF_ID);
			MinionBenefitLevel minionBenefit = minionBenefitLevelBandDefintion.GetMinionBenefit(minion.Level);
			if (randomService.NextFloat() < minionBenefit.doubleDropPercentage)
			{
				resourcePlot.BonusMinionItems.Add(resourcePlot.parentLair.Definition.ResourceItemID);
			}
			if (randomService.NextFloat() < minionBenefit.premiumDropPercentage)
			{
				resourcePlot.BonusMinionItems.Add(1);
			}
			if (randomService.NextFloat() < minionBenefit.rareDropPercentage)
			{
				int iD = playerService.GetWeightedInstance(4000).NextPick(randomService).ID;
				resourcePlot.BonusMinionItems.Add(iD);
			}
		}
	}
}
