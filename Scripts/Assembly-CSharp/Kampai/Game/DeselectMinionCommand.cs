using System.Collections.Generic;
using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DeselectMinionCommand : Command
	{
		[Inject]
		public int minionID { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public MinionStateChangeSignal stateChangeSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		public override void Execute()
		{
			if (!model.HasPlayedSFX)
			{
				soundFXSignal.Dispatch("Play_minion_deselect_01");
				model.HasPlayedSFX = true;
			}
			stateChangeSignal.Dispatch(minionID, MinionState.Idle);
			routineRunner.StopTimer("MinionSelectionComplete");
			Point item = new Point(-1, -1);
			if (model.Points != null)
			{
				SelectedMinionModel selectedMinionModel = model.SelectedMinions[minionID];
				item = new Point(selectedMinionModel.RunLocation.x, selectedMinionModel.RunLocation.z);
				model.Points.Enqueue(item);
			}
			model.SelectedMinions.Remove(minionID);
			if (model.SelectedMinions.Count == 0)
			{
				model.HasPlayedGacha = false;
			}
			else
			{
				if (model.Points == null || !item.Equals(model.MainPoint))
				{
					return;
				}
				using (Dictionary<int, SelectedMinionModel>.KeyCollection.Enumerator enumerator = model.SelectedMinions.Keys.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						int current = enumerator.Current;
						SelectedMinionModel selectedMinionModel2 = model.SelectedMinions[current];
						model.MainPoint = new Point(selectedMinionModel2.RunLocation.x, selectedMinionModel2.RunLocation.z);
					}
				}
			}
		}
	}
}
