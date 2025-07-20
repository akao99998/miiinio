using Kampai.Util;

namespace Kampai.Game.View
{
	public class PlayMecanimStateAction : KampaiAction
	{
		private ActionableObject target;

		private int StateHash;

		private int Layer;

		public PlayMecanimStateAction(ActionableObject target, int stateHash, IKampaiLogger logger, int layer = 0)
			: base(logger)
		{
			this.target = target;
			StateHash = stateHash;
			Layer = layer;
		}

		public override void Execute()
		{
			target.PlayAnimation(StateHash, Layer, 0f);
			base.Done = true;
		}
	}
}
