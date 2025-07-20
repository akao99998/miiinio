using System;
using Kampai.Util;

namespace Kampai.Game.View
{
	public class DelegateAction : KampaiAction
	{
		private Action Once;

		public DelegateAction(Action once, IKampaiLogger logger)
			: base(logger)
		{
			Once = once;
		}

		public override void Execute()
		{
			if (!base.Done)
			{
				Once();
				base.Done = true;
			}
		}
	}
}
