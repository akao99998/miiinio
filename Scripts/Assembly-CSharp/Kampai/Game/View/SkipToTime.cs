using System;

namespace Kampai.Game.View
{
	public class SkipToTime
	{
		private Func<float> timeProvider;

		public int StateHash { get; private set; }

		public int Layer { get; private set; }

		public SkipToTime(int layer, int stateHash, Func<float> timeProvider)
		{
			Layer = layer;
			StateHash = stateHash;
			this.timeProvider = timeProvider;
		}

		public float GetTime()
		{
			return timeProvider();
		}
	}
}
