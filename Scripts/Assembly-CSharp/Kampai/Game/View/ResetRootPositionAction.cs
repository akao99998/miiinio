using Kampai.Util;

namespace Kampai.Game.View
{
	public class ResetRootPositionAction : KampaiAction
	{
		private MinionObject obj;

		public ResetRootPositionAction(MinionObject obj, IKampaiLogger logger)
			: base(logger)
		{
			this.obj = obj;
		}

		public override void Execute()
		{
			if (!base.Done)
			{
				obj.ResetRootPosition();
				base.Done = true;
			}
		}
	}
}
