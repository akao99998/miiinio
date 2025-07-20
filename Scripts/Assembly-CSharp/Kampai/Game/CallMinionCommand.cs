using Kampai.Common;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CallMinionCommand : Command
	{
		[Inject]
		public Building building { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public FinishCallMinionSignal finishSignal { get; set; }

		[Inject]
		public GameObject signalSender { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public StartMinionTaskSignal startMinionTaskSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playMinionNoAnimAudioSignal { get; set; }

		[Inject]
		public DeselectMinionSignal deselectSignal { get; set; }

		public override void Execute()
		{
			TaskableBuilding taskableBuilding = building as TaskableBuilding;
			int harvestTimeForTaskableBuilding = BuildingUtil.GetHarvestTimeForTaskableBuilding(taskableBuilding, definitionService);
			if (taskableBuilding == null)
			{
				return;
			}
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			Minion closestMinionToLocation = component.GetClosestMinionToLocation(building.Location, building is ResourceBuilding);
			if (closestMinionToLocation == null)
			{
				return;
			}
			if (closestMinionToLocation.State == MinionState.Selected)
			{
				DebrisBuilding debrisBuilding = building as DebrisBuilding;
				if (debrisBuilding == null)
				{
					return;
				}
				deselectSignal.Dispatch(closestMinionToLocation.ID);
			}
			playMinionNoAnimAudioSignal.Dispatch("Play_minion_confirm_pathToBldg_01");
			MinionObject second = component.Get(closestMinionToLocation.ID);
			startMinionTaskSignal.Dispatch(new Tuple<int, MinionObject, int>(taskableBuilding.ID, second, timeService.CurrentTime()));
			finishSignal.Dispatch(new Tuple<int, int, GameObject>(closestMinionToLocation.ID, harvestTimeForTaskableBuilding, signalSender));
		}
	}
}
