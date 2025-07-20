using strange.extensions.signal.impl;

namespace Kampai.Util
{
	public class SignalCallback<T> where T : Signal
	{
		private readonly T _signal;

		public bool WillDispatch { get; private set; }

		public SignalCallback(T signal)
		{
			_signal = signal;
		}

		public T PromiseDispatch()
		{
			WillDispatch = true;
			return _signal;
		}
	}
}
