using System;
using System.Collections.Generic;
using System.Reflection;
using Kampai.Util;

namespace Kampai.Game
{
	public class SignalListener
	{
		private sealed class Data
		{
			public List<Tuple<CompleteSignal, ReturnValueContainer>> callbacks = new List<Tuple<CompleteSignal, ReturnValueContainer>>();
		}

		private Dictionary<string, Data> watchedSignals = new Dictionary<string, Data>();

		private void SetKey<T>(ReturnValueContainer parent, int index, int max, T value)
		{
			if (index < max)
			{
				ReturnValueContainer obj = parent.PushIndex();
				Type type = value.GetType();
				if (type.IsEnum)
				{
					type = typeof(long);
				}
				MethodInfo method = typeof(ReturnValueContainer).GetMethod("Set", new Type[1] { type });
				method.Invoke(obj, new object[1] { value });
			}
		}

		public void SignalDispatched<T1, T2, T3, T4>(string name, int paramCount, T1 param1, T2 param2, T3 param3, T4 param4)
		{
			Data value;
			if (!watchedSignals.TryGetValue(name, out value))
			{
				return;
			}
			List<CompleteSignal> list = new List<CompleteSignal>();
			int i = 0;
			for (int count = value.callbacks.Count; i < count; i++)
			{
				Tuple<CompleteSignal, ReturnValueContainer> tuple = value.callbacks[i];
				ReturnValueContainer item = tuple.Item2;
				if (paramCount < 1)
				{
					item.SetEmptyArray();
				}
				else
				{
					SetKey(item, 0, paramCount, param1);
					SetKey(item, 1, paramCount, param2);
					SetKey(item, 2, paramCount, param3);
					SetKey(item, 3, paramCount, param4);
				}
				list.Add(tuple.Item1);
			}
			value.callbacks.Clear();
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				list[j].Dispatch(name);
			}
		}

		public void ListenForSignal(string name, CompleteSignal callback, ReturnValueContainer ret)
		{
			Data value;
			if (!watchedSignals.TryGetValue(name, out value))
			{
				value = new Data();
				watchedSignals.Add(name, value);
			}
			value.callbacks.Add(new Tuple<CompleteSignal, ReturnValueContainer>(callback, ret));
		}

		public void StopListeningForSignal(string name)
		{
			watchedSignals.Remove(name);
		}

		public void Clear()
		{
			watchedSignals.Clear();
		}
	}
}
