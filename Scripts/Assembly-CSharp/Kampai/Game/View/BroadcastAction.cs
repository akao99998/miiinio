using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	internal sealed class BroadcastAction : KampaiAction
	{
		private ActionableObject minionObj;

		private Signal signal;

		public BroadcastAction(ActionableObject obj, Signal signal, IKampaiLogger logger)
			: base(logger)
		{
			minionObj = obj;
			this.signal = signal;
			signal.AddListener(Callback);
		}

		public override void Abort()
		{
			signal.RemoveListener(Callback);
			base.Done = true;
		}

		private void Callback()
		{
			if (!base.Done && (minionObj.currentAction == this || minionObj.GetNextAction() == this))
			{
				base.Done = true;
				signal.RemoveListener(Callback);
			}
		}
	}
}
