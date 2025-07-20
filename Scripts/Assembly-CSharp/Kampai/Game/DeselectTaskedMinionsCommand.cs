using System.Collections.Generic;
using Kampai.Common;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DeselectTaskedMinionsCommand : Command
	{
		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChangeSignal { get; set; }

		[Inject]
		public DeselectMinionSignal deselectMinionSignal { get; set; }

		public override void Execute()
		{
			Deselect();
		}

		private void Deselect()
		{
			List<int> list = new List<int>();
			foreach (int key in model.SelectedMinions.Keys)
			{
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(key);
				if (byInstanceId.State == MinionState.Tasking || byInstanceId.State == MinionState.PlayingMignette || byInstanceId.State == MinionState.Leisure)
				{
					list.Add(key);
				}
			}
			foreach (int item in list)
			{
				Minion byInstanceId2 = playerService.GetByInstanceId<Minion>(item);
				MinionState state = byInstanceId2.State;
				deselectMinionSignal.Dispatch(item);
				minionStateChangeSignal.Dispatch(byInstanceId2.ID, state);
			}
		}
	}
}
