using strange.extensions.signal.impl;

namespace Kampai.Util
{
	public abstract class RateLimitedSignal : Signal, RateLimitedSignalProvider
	{
		private float lastDispatch = float.MinValue;

		public abstract float MinimumGap { get; }

		public abstract float CurrentTime { get; }

		public new virtual void Dispatch()
		{
			if (CanDispatch(this, ref lastDispatch))
			{
				base.Dispatch();
			}
		}

		public static bool CanDispatch(RateLimitedSignalProvider provider, ref float lastDispatch)
		{
			float currentTime = provider.CurrentTime;
			if (currentTime - lastDispatch > provider.MinimumGap)
			{
				lastDispatch = currentTime;
				return true;
			}
			return false;
		}
	}
	public abstract class RateLimitedSignal<T> : Signal<T>, RateLimitedSignalProvider
	{
		private float lastDispatch = float.MinValue;

		public abstract float MinimumGap { get; }

		public abstract float CurrentTime { get; }

		public new void Dispatch(T param1)
		{
			if (RateLimitedSignal.CanDispatch(this, ref lastDispatch))
			{
				base.Dispatch(param1);
			}
		}
	}
	public abstract class RateLimitedSignal<T, U> : Signal<T, U>, RateLimitedSignalProvider
	{
		private float lastDispatch = float.MinValue;

		public abstract float MinimumGap { get; }

		public abstract float CurrentTime { get; }

		public new void Dispatch(T param1, U param2)
		{
			if (RateLimitedSignal.CanDispatch(this, ref lastDispatch))
			{
				base.Dispatch(param1, param2);
			}
		}
	}
	public abstract class RateLimitedSignal<T, U, V> : Signal<T, U, V>, RateLimitedSignalProvider
	{
		private float lastDispatch = float.MinValue;

		public abstract float MinimumGap { get; }

		public abstract float CurrentTime { get; }

		public new void Dispatch(T param1, U param2, V param3)
		{
			if (RateLimitedSignal.CanDispatch(this, ref lastDispatch))
			{
				base.Dispatch(param1, param2, param3);
			}
		}
	}
}
