using Kampai.Util;

namespace Kampai.Game.View
{
	public class UntrackVFXAction : KampaiAction
	{
		private MinionObject minion;

		public UntrackVFXAction(MinionObject minion, IKampaiLogger logger)
			: base(logger)
		{
			this.minion = minion;
		}

		public override void Execute()
		{
			minion.UntrackVFX();
			base.Done = true;
		}

		public override void Abort()
		{
			minion.UntrackVFX();
		}
	}
}
