using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util
{
	public class RoutineRunnerBehaviour : MonoBehaviour
	{
		private Dictionary<string, TimerUnit> timerDictionary = new Dictionary<string, TimerUnit>();

		private List<string> removeList = new List<string>();

		public void StartTimer(string timerID, float time, Action onComplete)
		{
			if (timerDictionary.ContainsKey(timerID))
			{
				timerDictionary[timerID].TimeLeft = time;
				return;
			}
			TimerUnit value = new TimerUnit(time, onComplete);
			timerDictionary.Add(timerID, value);
		}

		public void StopTimer(string timerID)
		{
			if (timerDictionary.ContainsKey(timerID))
			{
				timerDictionary.Remove(timerID);
			}
		}

		private void Update()
		{
			Dictionary<string, TimerUnit>.Enumerator enumerator = timerDictionary.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, TimerUnit> current = enumerator.Current;
					current.Value.TimeLeft -= Time.deltaTime;
					if (current.Value.TimeLeft < 0f)
					{
						if (current.Value.OnComplete != null)
						{
							current.Value.OnComplete();
						}
						removeList.Add(current.Key);
					}
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			if (removeList.Count == 0)
			{
				return;
			}
			List<string>.Enumerator enumerator2 = removeList.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					timerDictionary.Remove(enumerator2.Current);
				}
			}
			finally
			{
				enumerator2.Dispose();
			}
			removeList.Clear();
		}
	}
}
