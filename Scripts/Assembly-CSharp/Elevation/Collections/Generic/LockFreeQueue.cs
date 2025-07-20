using System.Threading;

namespace Elevation.Collections.Generic
{
	public class LockFreeQueue<T> where T : class
	{
		private sealed class Node<U>
		{
			public Node<U> Next;

			public U Item;
		}

		private Node<T> _head;

		private Node<T> _tail;

		public LockFreeQueue()
		{
			_head = new Node<T>();
			_tail = _head;
		}

		public void Enqueue(T item)
		{
			Node<T> node = null;
			Node<T> node2 = new Node<T>();
			node2.Item = item;
			bool flag = false;
			while (!flag)
			{
				node = _tail;
				Node<T> next = node.Next;
				if (_tail == node)
				{
					if (next == null)
					{
						flag = CompareExchange(ref _tail.Next, node2, null);
					}
					else
					{
						CompareExchange(ref _tail, next, node);
					}
				}
			}
			CompareExchange(ref _tail, node2, node);
		}

		public bool Dequeue(out T item)
		{
			item = (T)null;
			Node<T> node = null;
			bool flag = false;
			while (!flag)
			{
				node = _head;
				Node<T> next = node.Next;
				Node<T> tail = _tail;
				if (node != _head)
				{
					continue;
				}
				if (node == tail)
				{
					if (next == null)
					{
						return false;
					}
					CompareExchange(ref _tail, next, tail);
				}
				else
				{
					item = next.Item;
					flag = CompareExchange(ref _head, next, node);
				}
			}
			return true;
		}

		public T Dequeue()
		{
			T item;
			Dequeue(out item);
			return item;
		}

		private static bool CompareExchange(ref Node<T> location, Node<T> newValue, Node<T> comparand)
		{
			return comparand == Interlocked.CompareExchange(ref location, newValue, comparand);
		}
	}
}
