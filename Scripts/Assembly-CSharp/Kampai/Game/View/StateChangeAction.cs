using Kampai.Util;

namespace Kampai.Game.View
{
	public class StateChangeAction : KampaiAction
	{
		private MinionStateChangeSignal StateChangeSignal;

		private int MinionId;

		private MinionState NewState;

		public StateChangeAction(int minionId, MinionStateChangeSignal stateChangeSignal, MinionState newState, IKampaiLogger logger)
			: base(logger)
		{
			StateChangeSignal = stateChangeSignal;
			MinionId = minionId;
			NewState = newState;
		}

		public override void Abort()
		{
			base.Done = true;
		}

		public override void Execute()
		{
			StateChangeSignal.Dispatch(MinionId, NewState);
			base.Done = true;
		}
	}
}
