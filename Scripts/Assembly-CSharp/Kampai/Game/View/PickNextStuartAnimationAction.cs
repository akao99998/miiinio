using Kampai.Util;

namespace Kampai.Game.View
{
	public class PickNextStuartAnimationAction : KampaiAction
	{
		protected StuartView obj;

		protected StuartStageAnimationType targetState;

		public PickNextStuartAnimationAction(StuartView obj, StuartStageAnimationType targetState, IKampaiLogger logger)
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
