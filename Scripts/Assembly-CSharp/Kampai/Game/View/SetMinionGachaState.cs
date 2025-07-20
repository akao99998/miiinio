using Kampai.Util;

namespace Kampai.Game.View
{
	public class SetMinionGachaState : KampaiAction
	{
		private readonly MinionObject minionObject;

		private readonly MinionObject.MinionGachaState gachaState;

		public SetMinionGachaState(MinionObject minionObject, MinionObject.MinionGachaState gachaState, IKampaiLogger logger)
			: base(logger)
		{
			this.minionObject = minionObject;
			this.gachaState = gachaState;
		}

		public override void Execute()
		{
			minionObject.GachaState = gachaState;
			base.Done = true;
		}

		public override void Abort()
		{
			Execute();
		}
	}
}
