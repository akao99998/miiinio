using System.Collections;
using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class HarvestReadyCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("HarvestReadyCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public CreateResourceIconSignal createResourceIconSignal { get; set; }

		[Inject]
		public RemoveMinionFromLeisureSignal removeMinionFromLeisureSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingStateChangeSignal { get; set; }

		[Inject]
		public UpdateLeisureMenuViewSignal updateLeisureMenuViewSignal { get; set; }

		[Inject]
		public int buildingID { get; set; }

		public override void Execute()
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingID);
			ResourceBuilding resourceBuilding = byInstanceId as ResourceBuilding;
			CraftingBuilding craftingBuilding = byInstanceId as CraftingBuilding;
			LeisureBuilding leisureBuilding = byInstanceId as LeisureBuilding;
			VillainLairResourcePlot villainLairResourcePlot = byInstanceId as VillainLairResourcePlot;
			if (resourceBuilding != null)
			{
				HarvestTaskableBuilding(resourceBuilding);
			}
			else if (craftingBuilding != null)
			{
				HarvestCraftingBuilding(craftingBuilding);
			}
			else if (leisureBuilding != null)
			{
				HarvestLeisureBuilding(leisureBuilding);
			}
			else if (villainLairResourcePlot != null)
			{
				HarvestLairResourcePLot(villainLairResourcePlot);
			}
			else
			{
				logger.Fatal(FatalCode.TE_NULL_ARG, "A non-producing building just finished its task...");
			}
		}

		private void HarvestTaskableBuilding(ResourceBuilding resourceBuilding)
		{
			int transactionID = resourceBuilding.GetTransactionID(definitionService);
			int harvestItemDefinitionIdFromTransactionId = definitionService.GetHarvestItemDefinitionIdFromTransactionId(transactionID);
			IQuestService obj = questService;
			int item = harvestItemDefinitionIdFromTransactionId;
			obj.UpdateAllQuestsWithQuestStepType(QuestStepType.Harvest, QuestTaskTransition.Harvestable, null, 0, item);
			int count = resourceBuilding.BonusMinionItems.Count;
			if (count > 0)
			{
				routineRunner.StartCoroutine(CreateResourceIcon(resourceBuilding.ID, 196, count, true));
			}
			int availableHarvest = resourceBuilding.GetAvailableHarvest();
			routineRunner.StartCoroutine(CreateResourceIcon(resourceBuilding.ID, harvestItemDefinitionIdFromTransactionId, availableHarvest));
		}

		private void HarvestLeisureBuilding(LeisureBuilding leisureBuilding)
		{
			removeMinionFromLeisureSignal.Dispatch(leisureBuilding.ID);
			buildingStateChangeSignal.Dispatch(leisureBuilding.ID, BuildingState.Harvestable);
			updateLeisureMenuViewSignal.Dispatch();
			createResourceIconSignal.Dispatch(new ResourceIconSettings(leisureBuilding.ID, 2, leisureBuilding.Definition.PartyPointsReward));
		}

		private void HarvestLairResourcePLot(VillainLairResourcePlot resourcePlot)
		{
			VillainLairEntranceBuilding byInstanceId = playerService.GetByInstanceId<VillainLairEntranceBuilding>(resourcePlot.parentLair.portalInstanceID);
			int count = resourcePlot.BonusMinionItems.Count;
			Tuple<int, int> newHarvestAvailableForPortal = byInstanceId.GetNewHarvestAvailableForPortal(playerService);
			if (count > 0)
			{
				routineRunner.StartCoroutine(CreateResourceIcon(resourcePlot.ID, 196, count, true));
				routineRunner.StartCoroutine(CreateResourceIcon(byInstanceId.ID, 196, newHarvestAvailableForPortal.Item2, true));
			}
			int resourceItemID = resourcePlot.parentLair.Definition.ResourceItemID;
			routineRunner.StartCoroutine(CreateResourceIcon(resourcePlot.ID, resourceItemID, resourcePlot.harvestCount));
			routineRunner.StartCoroutine(CreateResourceIcon(byInstanceId.ID, resourceItemID, newHarvestAvailableForPortal.Item1));
		}

		private void HarvestCraftingBuilding(CraftingBuilding craftingBuilding)
		{
			int num = craftingBuilding.CompletedCrafts[craftingBuilding.CompletedCrafts.Count - 1];
			IQuestService obj = questService;
			int item = num;
			obj.UpdateAllQuestsWithQuestStepType(QuestStepType.Harvest, QuestTaskTransition.Harvestable, null, 0, item);
			for (int i = 0; i < craftingBuilding.CompletedCrafts.Count; i++)
			{
				SetupCraftingIcon(craftingBuilding, i);
			}
		}

		private void SetupCraftingIcon(CraftingBuilding craftingBuilding, int itemIndex)
		{
			int num = craftingBuilding.CompletedCrafts[itemIndex];
			int num2 = 0;
			foreach (int completedCraft in craftingBuilding.CompletedCrafts)
			{
				if (completedCraft == num)
				{
					num2++;
				}
			}
			routineRunner.StartCoroutine(CreateResourceIcon(buildingID, num, num2));
		}

		private IEnumerator CreateResourceIcon(int buildingId, int itemDefId, int count, bool isBonus = false)
		{
			if (count != 0)
			{
				yield return null;
				createResourceIconSignal.Dispatch(new ResourceIconSettings(buildingId, itemDefId, count, isBonus));
			}
		}
	}
}
