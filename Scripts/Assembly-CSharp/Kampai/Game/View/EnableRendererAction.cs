using Kampai.Util;

namespace Kampai.Game.View
{
	public class EnableRendererAction : KampaiAction
	{
		private MinionObject obj;

		private bool enable;

		public EnableRendererAction(MinionObject obj, bool enable, IKampaiLogger logger)
			: base(logger)
		{
			this.obj = obj;
			this.enable = enable;
		}

		public override void Abort()
		{
			base.Done = true;
		}

		public override void Execute()
		{
			obj.EnableRenderers(enable);
			base.Done = true;
		}
	}
}
