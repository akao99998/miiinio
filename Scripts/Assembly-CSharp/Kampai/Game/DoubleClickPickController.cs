using Kampai.Common;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DoubleClickPickController : Command
	{
		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public DeselectAllMinionsSignal deselectAllSignal { get; set; }

		public override void Execute()
		{
			if (model.SelectedMinions.Count > 0)
			{
				deselectAllSignal.Dispatch();
			}
		}
	}
}
