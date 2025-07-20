using System;
using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game.View
{
	public class SyncDelegateAction : SyncAction
	{
		private bool Fired;

		private Action Once;

		public SyncDelegateAction(IList<ActionableObject> syncObjects, Action once, IKampaiLogger logger)
			: base(syncObjects, logger)
		{
			Once = once;
		}

		public override void LateUpdate()
		{
			bool flag = false;
			lock (syncObjects)
			{
				base.LateUpdate();
				if (!Fired && base.Done)
				{
					Fired = true;
					flag = true;
				}
			}
			if (flag)
			{
				Once();
			}
		}
	}
}
