using Kampai.Util;

namespace Kampai.Game.View
{
	internal sealed class IncidentalFinishedAction : KampaiAction
	{
		private int id;

		private MinionStateChangeSignal stateChangeSignal;

		public IncidentalFinishedAction(int id, MinionStateChangeSignal stateChangeSignal, IKampaiLogger logger)
			: base(logger)
		{
			this.id = id;
			this.stateChangeSignal = stateChangeSignal;
		}

		public override void Execute()
		{
			stateChangeSignal.Dispatch(id, MinionState.Idle);
			base.Done = true;
		}
	}
}
