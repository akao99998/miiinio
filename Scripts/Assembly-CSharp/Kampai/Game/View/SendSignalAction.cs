using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	internal sealed class SendSignalAction : KampaiAction
	{
		private Signal signal;

		public SendSignalAction(Signal signal, IKampaiLogger logger)
			: base(logger)
		{
			this.signal = signal;
		}

		public override void Execute()
		{
			signal.Dispatch();
			base.Done = true;
		}
	}
}
