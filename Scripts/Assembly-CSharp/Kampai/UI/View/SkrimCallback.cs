using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SkrimCallback
	{
		public Signal<KampaiDisposable> Callback { get; private set; }

		public SkrimCallback(Signal<KampaiDisposable> signal)
		{
			Callback = signal;
		}
	}
}
