using System.Collections.Generic;
using Kampai.Common;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DeselectAllMinionsCommand : Command
	{
		[Inject]
		public DeselectMinionSignal deselectSignal { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		public override void Execute()
		{
			DeselectAll();
		}

		private void DeselectAll()
		{
			List<int> list = new List<int>();
			foreach (int key in model.SelectedMinions.Keys)
			{
				list.Add(key);
			}
			foreach (int item in list)
			{
				deselectSignal.Dispatch(item);
			}
			list.Clear();
			list = null;
			model.SelectedMinions.Clear();
			model.HasPlayedGacha = false;
		}
	}
}
