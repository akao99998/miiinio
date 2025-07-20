using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	internal sealed class SignalAction : KampaiAction
	{
		private ActionableObject minionObj;

		private Signal<int> signal;

		public SignalAction(ActionableObject obj, Signal<int> signal, IKampaiLogger logger)
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

		private void Callback(int id)
		{
			if (!base.Done && (minionObj.currentAction == this || minionObj.GetNextAction() == this) && minionObj.ID == id)
			{
				base.Done = true;
				signal.RemoveListener(Callback);
			}
		}
	}
}
