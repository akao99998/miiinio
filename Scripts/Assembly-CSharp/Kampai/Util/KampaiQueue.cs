using System.Collections;
using System.Collections.Generic;

namespace Kampai.Util
{
	public class KampaiQueue<T> : IEnumerable, IEnumerable<T>
	{
		protected LinkedList<T> queue;

		public int Count
		{
			get
			{
				return queue.Count;
			}
		}

		public KampaiQueue()
		{
			queue = new LinkedList<T>();
		}

		public KampaiQueue(IEnumerable<T> enumerable)
		{
			queue = new LinkedList<T>();
			foreach (T item in enumerable)
			{
				Enqueue(item);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return queue.GetEnumerator();
		}

		public T Peek()
		{
			return queue.First.Value;
		}

		public T Dequeue()
		{
			T value = queue.First.Value;
			queue.RemoveFirst();
			return value;
		}

		public void AddFirst(T item)
		{
			queue.AddFirst(item);
		}

		public void Enqueue(T item)
		{
			queue.AddLast(item);
		}

		public bool Remove(T item)
		{
			return queue.Remove(item);
		}

		public void Clear()
		{
			queue.Clear();
		}
	}
}
