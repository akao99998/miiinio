using Kampai.Common;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MoveMinionFinishedCommand : Command
	{
		[Inject]
		public int minionID { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SelectionCompleteSignal selectionCompleteSignal { get; set; }

		[Inject]
		public DeselectAllMinionsSignal deselectAllMinionsSignal { get; set; }

		public override void Execute()
		{
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionID);
			if (byInstanceId != null && byInstanceId.State == MinionState.Selected)
			{
				SelectedMinionFinishedRouting();
			}
		}

		private void SelectedMinionFinishedRouting()
		{
			model.SelectedMinions[minionID].FinishedRouting = true;
			bool flag = true;
			Vector3 type = default(Vector3);
			foreach (SelectedMinionModel value in model.SelectedMinions.Values)
			{
				if (!value.FinishedRouting)
				{
					flag = false;
					break;
				}
				type += value.RunLocation;
			}
			if (flag)
			{
				type /= (float)model.SelectedMinions.Count;
				selectionCompleteSignal.Dispatch(type);
				deselectAllMinionsSignal.Dispatch();
			}
		}
	}
}
