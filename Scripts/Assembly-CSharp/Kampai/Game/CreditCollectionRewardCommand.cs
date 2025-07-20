using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreditCollectionRewardCommand : Command
	{
		[Inject]
		public MignetteCollectionService collectionService { get; set; }

		[Inject]
		public OpenStoreHighlightItemSignal highlightStoreItemSignal { get; set; }

		[Inject]
		public CameraAutoMoveToBuildingSignal cameraAutoMoveToBuildingSignal { get; set; }

		[Inject]
		public CompositeBuildingPieceAddedSignal compositeBuildingPieceAddedSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			TransactionDefinition pendingRewardTransaction = collectionService.pendingRewardTransaction;
			foreach (QuantityItem output in pendingRewardTransaction.Outputs)
			{
				int iD = output.ID;
				CompositeBuildingPieceDefinition definition = null;
				if (!definitionService.TryGet<CompositeBuildingPieceDefinition>(iD, out definition))
				{
					continue;
				}
				CompositeBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<CompositeBuilding>(definition.BuildingDefinitionID);
				if (firstInstanceByDefinitionId != null)
				{
					if (firstInstanceByDefinitionId.State == BuildingState.Inventory)
					{
						highlightStoreItemSignal.Dispatch(definition.BuildingDefinitionID, true);
					}
					else
					{
						cameraAutoMoveToBuildingSignal.Dispatch(firstInstanceByDefinitionId, new PanInstructions(firstInstanceByDefinitionId));
					}
					compositeBuildingPieceAddedSignal.Dispatch(firstInstanceByDefinitionId);
				}
			}
		}
	}
}
