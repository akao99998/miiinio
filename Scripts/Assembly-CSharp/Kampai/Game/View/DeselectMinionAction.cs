using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	internal sealed class DeselectMinionAction : KampaiAction
	{
		private int minionID;

		private Signal<int> signal;

		public DeselectMinionAction(int minionID, Signal<int> signal, IKampaiLogger logger)
			: base(logger)
		{
			this.minionID = minionID;
			this.signal = signal;
		}

		public override void Abort()
		{
			base.Done = true;
		}

		public override void Execute()
		{
			signal.Dispatch(minionID);
			base.Done = true;
		}
	}
}
