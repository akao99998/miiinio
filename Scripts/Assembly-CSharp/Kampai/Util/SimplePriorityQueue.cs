using System;
using System.Collections.Generic;

namespace Kampai.Util
{
	public class SimplePriorityQueue<T>
	{
		private SortedDictionary<int, Queue<T>> heap;

		private int count;

		public int Count
		{
			get
			{
				return count;
			}
		}

		public SimplePriorityQueue()
		{
			heap = new SortedDictionary<int, Queue<T>>();
			count = 0;
		}

		public T Dequeue()
		{
			if (count <= 0)
			{
				return default(T);
			}
			T result = default(T);
			foreach (Queue<T> value in heap.Values)
			{
				if (value.Count > 0)
				{
					count--;
					result = value.Dequeue();
					break;
				}
			}
			return result;
		}

		public void Enqueue(T item, int priority)
		{
			if (!heap.ContainsKey(priority))
			{
				heap.Add(priority, new Queue<T>());
			}
			heap[priority].Enqueue(item);
			count++;
		}

		public T Peek()
		{
			if (count <= 0)
			{
				throw new InvalidOperationException();
			}
			T result = default(T);
			foreach (Queue<T> value in heap.Values)
			{
				if (value.Count > 0)
				{
					result = value.Peek();
					break;
				}
			}
			return result;
		}

		public void Clear()
		{
			count = 0;
			heap.Clear();
		}
	}
}
