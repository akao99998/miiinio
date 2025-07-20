using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public interface ITimeEventService
	{
		float TimerScale { get; set; }

		bool AddEvent(int instanceId, int startTime, int eventTime, Signal<int> timeEventSignal, TimeEventType eventType = TimeEventType.Default);

		void RushEvent(int instanceId);

		void RemoveEvent(int instanceId);

		int GetTimeRemaining(int instanceId);

		int GetEventDuration(int instanceId);

		int CalculateRushCostForTimer(int timerDurationInSecond, RushActionType rushActionType);

		bool HasEventID(int id);

		void SpeedUpTimers(int amount);

		void StartBuff(TimeEventType eventType, float buffMultipler, int buffStartTime);

		void StopBuff(TimeEventType eventType, int buffDuration);
	}
}
