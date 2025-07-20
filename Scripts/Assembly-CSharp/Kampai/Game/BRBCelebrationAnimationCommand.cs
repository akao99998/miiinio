using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class BRBCelebrationAnimationCommand : Command
	{
		[Inject]
		public int minionID { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public StartIncidentalAnimationSignal incidentalSignal { get; set; }

		public override void Execute()
		{
			QuantityItem quantityItem = playerService.GetWeightedInstance(4012).NextPick(randomService);
			incidentalSignal.Dispatch(minionID, quantityItem.ID);
		}
	}
}
