using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class RemoveCraftingQueueCommand : Command
	{
		[Inject]
		public Tuple<int, int> buildingIDItemIndex { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RefreshQueueSlotSignal refreshSignal { get; set; }

		[Inject]
		public CraftingUpdateReagentsSignal craftingUpdateReagentsSignal { get; set; }

		[Inject]
		public HarvestReadySignal harvestSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public UIModel model { get; set; }

		public override void Execute()
		{
			CraftingBuilding byInstanceId = playerService.GetByInstanceId<CraftingBuilding>(buildingIDItemIndex.Item1);
			if (byInstanceId.RecipeInQueue.Count > 0)
			{
				int id = byInstanceId.RecipeInQueue[buildingIDItemIndex.Item2];
				IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(id);
				byInstanceId.RecipeInQueue.RemoveAt(buildingIDItemIndex.Item2);
				refreshSignal.Dispatch(false);
				if (model.CraftingUIOpen)
				{
					TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(ingredientsItemDefinition.TransactionId);
					foreach (QuantityItem output in transactionDefinition.Outputs)
					{
						TransactionArg transactionArg = new TransactionArg(byInstanceId.ID);
						transactionArg.CraftableXPEarned = TransactionUtil.GetXPOutputForTransaction(transactionDefinition);
						playerService.CreateAndRunCustomTransaction(output.ID, (int)output.Quantity, TransactionTarget.NO_VISUAL, transactionArg);
						masterPlanService.ProcessActiveComponent(MasterPlanComponentTaskType.Collect, output.Quantity, output.ID);
					}
					craftingUpdateReagentsSignal.Dispatch();
				}
				else
				{
					byInstanceId.CompletedCrafts.Add(ingredientsItemDefinition.ID);
					harvestSignal.Dispatch(buildingIDItemIndex.Item1);
				}
			}
			HandleStateChange(byInstanceId);
		}

		private void HandleStateChange(CraftingBuilding building)
		{
			BuildingChangeStateSignal instance = gameContext.injectionBinder.GetInstance<BuildingChangeStateSignal>();
			if (building.CompletedCrafts.Count == 0)
			{
				if (building.RecipeInQueue.Count > 0)
				{
					instance.Dispatch(building.ID, BuildingState.Working);
				}
				else
				{
					instance.Dispatch(building.ID, BuildingState.Idle);
				}
			}
			else if (building.RecipeInQueue.Count > 0)
			{
				instance.Dispatch(building.ID, BuildingState.HarvestableAndWorking);
			}
			else
			{
				instance.Dispatch(building.ID, BuildingState.Harvestable);
			}
		}
	}
}
