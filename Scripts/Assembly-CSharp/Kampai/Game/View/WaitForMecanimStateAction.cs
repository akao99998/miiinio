using Kampai.Util;

namespace Kampai.Game.View
{
	public class WaitForMecanimStateAction : KampaiAction
	{
		private ActionableObject target;

		private int StateHash;

		private int Layer;

		public WaitForMecanimStateAction(ActionableObject target, int stateHash, IKampaiLogger logger, int layer = 0)
			: base(logger)
		{
			this.target = target;
			StateHash = stateHash;
			Layer = layer;
		}

		public override void Abort()
		{
			base.Done = true;
		}

		public override void Update()
		{
			if (target.IsInAnimatorState(StateHash, Layer))
			{
				base.Done = true;
			}
		}
	}
}
