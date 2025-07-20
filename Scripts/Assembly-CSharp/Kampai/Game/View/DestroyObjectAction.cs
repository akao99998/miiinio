using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class DestroyObjectAction : KampaiAction
	{
		private Object target;

		public DestroyObjectAction(Object target, IKampaiLogger logger)
			: base(logger)
		{
			this.target = target;
		}

		public override void Execute()
		{
			if (!base.Done)
			{
				Object.Destroy(target);
			}
			base.Done = true;
		}

		public override void Abort()
		{
			Execute();
		}
	}
}
