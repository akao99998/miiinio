using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI;
using Kampai.UI.View;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class MasterPlanComponentCompleteCommand : Command
	{
		[Inject]
		public int componentBuildingId { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		[Inject]
		public GetWayFinderSignal getWayFinderSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal playerTrainingSignal { get; set; }

		[Inject]
		public IGhostComponentService ghostService { get; set; }

		public override void Execute()
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			MasterPlanComponentBuilding byInstanceId = playerService.GetByInstanceId<MasterPlanComponentBuilding>(componentBuildingId);
			if (byInstanceId == null)
			{
				return;
			}
			getWayFinderSignal.Dispatch(componentBuildingId, delegate(int wayFinderId, IWayFinderView wayFinderView)
			{
				if (wayFinderView != null)
				{
					wayFinderView.SetForceHide(false);
				}
			});
			IList<MasterPlanComponent> instancesByType = playerService.GetInstancesByType<MasterPlanComponent>();
			foreach (MasterPlanComponent item in instancesByType)
			{
				if (item.buildingDefID == byInstanceId.Definition.ID)
				{
					ghostService.ClearGhostComponentBuildings(true, true);
					MasterPlanComponentBuildingObject masterPlanComponentBuildingObject = component.GetBuildingObject(byInstanceId.ID) as MasterPlanComponentBuildingObject;
					if (masterPlanComponentBuildingObject != null)
					{
						masterPlanComponentBuildingObject.TriggerVFX();
						globalAudioSignal.Dispatch("Play_building_repair_01");
					}
					break;
				}
			}
			List<MasterPlanComponentBuilding> instancesByType2 = playerService.GetInstancesByType<MasterPlanComponentBuilding>();
			if (instancesByType2.Count == 1)
			{
				playerTrainingSignal.Dispatch(19000031, false, new Signal<bool>());
			}
		}
	}
}
