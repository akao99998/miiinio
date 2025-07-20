using Kampai.Util;

namespace Kampai.Game.View
{
	public class SkipToTimeAction : KampaiAction
	{
		private SkipToTime skipToTime;

		private ActionableObject minion;

		public SkipToTimeAction(ActionableObject minion, SkipToTime skipToTime, IKampaiLogger logger)
			: base(logger)
		{
			this.minion = minion;
			this.skipToTime = skipToTime;
		}

		public override void LateUpdate()
		{
			if (minion.IsInAnimatorState(skipToTime.StateHash, skipToTime.Layer))
			{
				float time = skipToTime.GetTime();
				if (time > 0f)
				{
					minion.PlayAnimation(skipToTime.StateHash, skipToTime.Layer, time);
					base.Done = true;
				}
			}
		}
	}
}
