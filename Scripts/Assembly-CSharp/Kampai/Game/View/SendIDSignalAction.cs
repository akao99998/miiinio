using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	internal sealed class SendIDSignalAction : KampaiAction
	{
		private ActionableObject minionObj;

		private Signal<int> signal;

		public SendIDSignalAction(ActionableObject obj, Signal<int> signal, IKampaiLogger logger)
			: base(logger)
		{
			minionObj = obj;
			this.signal = signal;
		}

		public override void Execute()
		{
			signal.Dispatch(minionObj.ID);
			base.Done = true;
		}
	}
}
