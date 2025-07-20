using Kampai.Util;

namespace Kampai.Game.View
{
	public class TrackVFXAction : KampaiAction
	{
		private VFXScript vfxScript;

		private MinionObject minion;

		public TrackVFXAction(MinionObject minion, VFXScript vfxScript, IKampaiLogger logger)
			: base(logger)
		{
			this.vfxScript = vfxScript;
			this.minion = minion;
		}

		public override void Execute()
		{
			minion.TrackVFX(vfxScript);
			base.Done = true;
		}

		public override void Abort()
		{
			minion.UntrackVFX();
		}
	}
}
