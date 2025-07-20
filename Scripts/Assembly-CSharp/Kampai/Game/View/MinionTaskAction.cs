using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	internal sealed class MinionTaskAction : KampaiAction
	{
		private MinionObject minion;

		private Building building;

		private Signal<MinionObject, Building> signal;

		public MinionTaskAction(MinionObject minion, Building building, Signal<MinionObject, Building> signal, IKampaiLogger logger)
			: base(logger)
		{
			this.minion = minion;
			this.building = building;
			this.signal = signal;
		}

		public override void Abort()
		{
			base.Done = true;
		}

		public override void Execute()
		{
			signal.Dispatch(minion, building);
			base.Done = true;
		}
	}
}
