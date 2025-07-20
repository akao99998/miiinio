using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class TimeEventService : MonoBehaviour, ITimeEventService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("TimeEventService") as IKampaiLogger;

		private List<TimeEvent> timeEventList;

		private List<TimeEvent> dispatchList;

		private int buffStartTime;

		private Dictionary<TimeEventType, float> buffMultiplier;

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		public float TimerScale { get; set; }

		public TimeEventService()
		{
			timeEventList = new List<TimeEvent>();
			dispatchList = new List<TimeEvent>();
			buffMultiplier = new Dictionary<TimeEventType, float>();
		}

		public bool AddEvent(int instanceId, int startTime, int eventTime, Signal<int> timeEventSignal, TimeEventType eventType = TimeEventType.Default)
		{
			TimeEvent timeEvent = new TimeEvent(instanceId, startTime, eventTime, eventType, timeEventSignal);
			timeEventList.Add(timeEvent);
			if (TimerScale > 0.01f)
			{
				timeEvent.eventTime = Math.Max((int)((float)timeEvent.eventTime * TimerScale), 1);
			}
			logger.Log(KampaiLogLevel.Info, string.Format("Add Time Event: {0}\tStartTime: {1}\tTime: {2}\tSignal: {3}", instanceId, timeService.CurrentTime(), eventTime, timeEventSignal));
			return true;
		}

		public void RushEvent(int instanceId)
		{
			List<TimeEvent> list = new List<TimeEvent>();
			foreach (TimeEvent timeEvent in timeEventList)
			{
				if (timeEvent.instanceId == instanceId)
				{
					list.Add(timeEvent);
				}
			}
			foreach (TimeEvent item in list)
			{
				item.Dispatch();
				timeEventList.Remove(item);
			}
			setPremiumCurrencySignal.Dispatch();
		}

		public void RemoveEvent(int instanceId)
		{
			List<TimeEvent> list = new List<TimeEvent>();
			foreach (TimeEvent timeEvent in timeEventList)
			{
				if (timeEvent.instanceId == instanceId)
				{
					list.Add(timeEvent);
				}
			}
			foreach (TimeEvent item in list)
			{
				item.ClearSignal();
				timeEventList.Remove(item);
			}
		}

		public void StartBuff(TimeEventType eventType, float buffMultipler, int buffStartTime)
		{
			if (!buffMultiplier.ContainsKey(eventType))
			{
				buffMultiplier.Add(eventType, buffMultipler);
			}
			else
			{
				buffMultiplier[eventType] = buffMultipler;
			}
			this.buffStartTime = buffStartTime;
		}

		public void StopBuff(TimeEventType eventType, int buffDuration)
		{
			if (!buffMultiplier.ContainsKey(eventType))
			{
				return;
			}
			float num = buffMultiplier[eventType] - 1f;
			if (num < 0f || buffDuration < 0)
			{
				return;
			}
			foreach (TimeEvent timeEvent in timeEventList)
			{
				if (timeEvent.type != eventType)
				{
					continue;
				}
				if (buffStartTime < timeEvent.startTime)
				{
					int num2 = buffDuration + buffStartTime - timeEvent.startTime;
					if (num2 < 0)
					{
						num2 = 0;
					}
					timeEvent.buffTime += (int)((float)num2 * num);
				}
				else
				{
					timeEvent.buffTime += (int)((float)buffDuration * num);
				}
			}
			buffMultiplier.Remove(eventType);
			if (buffMultiplier.Count == 0)
			{
				RemoveEvent(80000);
				buffStartTime = 0;
			}
		}

		public int GetTimeRemaining(int instanceId)
		{
			foreach (TimeEvent timeEvent in timeEventList)
			{
				if (timeEvent.instanceId != instanceId)
				{
					continue;
				}
				int num = 0;
				if (timeEvent.type != 0)
				{
					if (buffStartTime == 0 || !buffMultiplier.ContainsKey(timeEvent.type))
					{
						num = timeEvent.eventTime - timeEvent.buffTime - (timeService.CurrentTime() - timeEvent.startTime);
						if (num < 0)
						{
							return 0;
						}
						return num;
					}
					int num2 = Mathf.Max(buffStartTime, timeEvent.startTime);
					int num3 = num2 - timeEvent.startTime;
					int num4 = (int)((float)(timeService.CurrentTime() - num2) * GetMultiplier(timeEvent));
					num = timeEvent.eventTime - timeEvent.buffTime - num4 - num3;
					if (num < 0)
					{
						return 0;
					}
					return num;
				}
				num = timeEvent.eventTime - (timeService.CurrentTime() - timeEvent.startTime);
				if (num <= 0)
				{
					return 0;
				}
				return num;
			}
			return -1;
		}

		public int GetEventDuration(int instanceId)
		{
			foreach (TimeEvent timeEvent in timeEventList)
			{
				if (timeEvent.instanceId == instanceId)
				{
					return timeEvent.eventTime;
				}
			}
			return 0;
		}

		public int CalculateRushCostForTimer(int timerDurationInSecond, RushActionType rushActionType)
		{
			if (timerDurationInSecond <= 0)
			{
				return 0;
			}
			if (rushActionType == RushActionType.HARVESTING)
			{
				uint quantity = playerService.GetQuantity(StaticItem.FREE_RESOURCE_RUSH_THRESHOLD);
				if (timerDurationInSecond <= quantity)
				{
					return 0;
				}
			}
			RushTimeBandDefinition rushTimeBandForTime = definitionService.GetRushTimeBandForTime(timerDurationInSecond);
			return rushTimeBandForTime.GetCostForRushActionType(rushActionType);
		}

		public void Update()
		{
			List<TimeEvent> list = new List<TimeEvent>(timeEventList);
			List<TimeEvent>.Enumerator enumerator = list.GetEnumerator();
			try
			{
				int currentGameTime = timeService.CurrentTime();
				while (enumerator.MoveNext())
				{
					TimeEvent current = enumerator.Current;
					if (IsEventExpired(current, currentGameTime))
					{
						dispatchList.Add(current);
					}
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			if (dispatchList == null || dispatchList.Count <= 0)
			{
				return;
			}
			List<TimeEvent> list2 = new List<TimeEvent>(dispatchList);
			enumerator = list2.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TimeEvent current2 = enumerator.Current;
					current2.Dispatch();
					logger.Debug(string.Format("Dispatching ID: {0}", current2.instanceId));
					timeEventList.Remove(current2);
				}
				dispatchList.Clear();
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		private bool IsEventExpired(TimeEvent timeEvent, int currentGameTime)
		{
			if (timeEvent.type != 0)
			{
				if (buffStartTime == 0 || !buffMultiplier.ContainsKey(timeEvent.type))
				{
					return currentGameTime - timeEvent.startTime >= timeEvent.eventTime - timeEvent.buffTime;
				}
				int num = Mathf.Max(timeEvent.startTime, buffStartTime);
				int num2 = currentGameTime - num;
				int num3 = (int)((float)num2 * GetMultiplier(timeEvent)) + num - timeEvent.startTime;
				int num4 = timeEvent.eventTime - timeEvent.buffTime;
				return num3 >= num4;
			}
			return currentGameTime - timeEvent.startTime >= timeEvent.eventTime;
		}

		private float GetMultiplier(TimeEvent timeEvent)
		{
			return (!buffMultiplier.ContainsKey(timeEvent.type)) ? 0f : buffMultiplier[timeEvent.type];
		}

		public bool HasEventID(int id)
		{
			foreach (TimeEvent timeEvent in timeEventList)
			{
				if (timeEvent.instanceId == id)
				{
					return true;
				}
			}
			return false;
		}

		public void SpeedUpTimers(int amount)
		{
			foreach (TimeEvent timeEvent in timeEventList)
			{
				timeEvent.eventTime -= amount;
			}
		}
	}
}
