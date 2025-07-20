using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class SetCullingModeAction : KampaiAction
	{
		private Animatable obj;

		private AnimatorCullingMode mode;

		public SetCullingModeAction(Animatable animatable, AnimatorCullingMode mode, IKampaiLogger logger)
			: base(logger)
		{
			obj = animatable;
			this.mode = mode;
		}

		public override void Execute()
		{
			obj.SetAnimatorCullingMode(mode);
			base.Done = true;
		}

		public override void Abort()
		{
			Execute();
		}
	}
}
