using System;

namespace Kampai.Util
{
	public class TimerUnit
	{
		public float TimeLeft { get; set; }

		public Action OnComplete { get; set; }

		public TimerUnit(float timeLeft, Action onComplete)
		{
			TimeLeft = timeLeft;
			OnComplete = onComplete;
		}
	}
}
