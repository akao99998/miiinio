using Kampai.UI;
using Kampai.Util;

namespace Kampai.Game.View
{
	public class PickNextDummyUIAnimationAction : KampaiAction
	{
		protected DummyCharacterObject obj;

		protected DummyCharacterAnimationState targetState;

		public PickNextDummyUIAnimationAction(DummyCharacterObject obj, DummyCharacterAnimationState targetState, IKampaiLogger logger)
			: base(logger)
		{
			this.obj = obj;
			this.targetState = targetState;
		}

		public override void Execute()
		{
			obj.StartingState(targetState);
			base.Done = true;
		}
	}
}
