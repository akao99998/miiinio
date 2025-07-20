using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CleanupMasterPlanCommand : Command
	{
		[Inject]
		public MasterPlan plan { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public RemoveBuildingSignal removeBuildingSignal { get; set; }

		public override void Execute()
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			MasterPlanComponentBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(plan.Definition.BuildingDefID);
			if (firstInstanceByDefinitionId == null)
			{
				return;
			}
			component.RemoveBuilding(firstInstanceByDefinitionId.ID);
			removeBuildingSignal.Dispatch(firstInstanceByDefinitionId.Location, definitionService.GetBuildingFootprint(firstInstanceByDefinitionId.Definition.FootprintID));
			playerService.Remove(firstInstanceByDefinitionId);
			for (int i = 0; i < plan.Definition.ComponentDefinitionIDs.Count; i++)
			{
				MasterPlanComponent firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(plan.Definition.ComponentDefinitionIDs[i]);
				if (firstInstanceByDefinitionId2 != null)
				{
					playerService.Remove(firstInstanceByDefinitionId2);
				}
			}
		}
	}
}
