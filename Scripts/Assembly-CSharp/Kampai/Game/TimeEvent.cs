using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class TimeEvent
	{
		private Signal<int> timeEventSignal;

		public int instanceId { get; private set; }

		public int startTime { get; set; }

		public int eventTime { get; set; }

		public int buffTime { get; set; }

		public TimeEventType type { get; private set; }

		public TimeEvent(int instanceId, int startTime, int eventTime, TimeEventType type, Signal<int> signal)
		{
			this.instanceId = instanceId;
			this.startTime = startTime;
			this.eventTime = eventTime;
			this.type = type;
			timeEventSignal = signal;
		}

		public void Dispatch()
		{
			if (timeEventSignal != null)
			{
				timeEventSignal.Dispatch(instanceId);
			}
		}

		public void ClearSignal()
		{
			timeEventSignal = null;
		}
	}
}
