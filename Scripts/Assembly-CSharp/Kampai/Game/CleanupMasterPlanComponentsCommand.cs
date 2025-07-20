using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CleanupMasterPlanComponentsCommand : Command
	{
		[Inject]
		public int componentBuildingId { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RemoveBuildingSignal removeBuildingSignal { get; set; }

		public override void Execute()
		{
			MasterPlanComponentBuilding byInstanceId = playerService.GetByInstanceId<MasterPlanComponentBuilding>(componentBuildingId);
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			component.RemoveBuilding(componentBuildingId);
			removeBuildingSignal.Dispatch(byInstanceId.Location, definitionService.GetBuildingFootprint(byInstanceId.Definition.FootprintID));
			playerService.Remove(byInstanceId);
		}
	}
}
