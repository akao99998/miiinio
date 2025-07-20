using Kampai.Common;
using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ShuffleCompositeBuildingPiecesCommand : Command
	{
		private const float MINION_REACT_RADIUS = 15f;

		[Inject]
		public int buildingID { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public MinionReactInRadiusSignal minionReactInRadiusSignal { get; set; }

		public override void Execute()
		{
			CompositeBuilding byInstanceId = playerService.GetByInstanceId<CompositeBuilding>(buildingID);
			byInstanceId.ShufflePieceIDs();
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(buildingID);
			CompositeBuildingMediator component2 = buildingObject.GetComponent<CompositeBuildingMediator>();
			component2.PlayShuffleSequence(byInstanceId.AttachedCompositePieceIDs);
			minionReactInRadiusSignal.Dispatch(15f, buildingObject.transform.position);
		}
	}
}
