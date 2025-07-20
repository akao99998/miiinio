using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game
{
	public class TransactionArg
	{
		private Dictionary<Type, object> arguments = new Dictionary<Type, object>();

		private ICollection<ItemAccumulator> accumulators = new List<ItemAccumulator>();

		public int InstanceId { get; set; }

		public Vector3 StartPosition { get; set; }

		public bool fromGlass { get; set; }

		public int TransactionUTCTime { get; set; }

		public bool IsFromPremiumSource { get; set; }

		public int IsFromQuestSource { get; set; }

		public int CraftableXPEarned { get; set; }

		public string Source { get; set; }

		public TransactionArg()
		{
		}

		public TransactionArg(int instanceId)
		{
			InstanceId = instanceId;
		}

		public TransactionArg(string source)
		{
			Source = source;
		}

		public void AddAccumulator(ItemAccumulator itemAccumulator)
		{
			accumulators.Add(itemAccumulator);
		}

		public ICollection<ItemAccumulator> GetAccumulators()
		{
			return accumulators;
		}

		public T Get<T>()
		{
			if (arguments.ContainsKey(typeof(T)))
			{
				return (T)arguments[typeof(T)];
			}
			return default(T);
		}

		public TransactionArg Add(object value)
		{
			if (value != null)
			{
				arguments.Add(value.GetType(), value);
			}
			return this;
		}
	}
}
