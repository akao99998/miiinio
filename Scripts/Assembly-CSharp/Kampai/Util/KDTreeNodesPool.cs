using System.Collections.Generic;

namespace Kampai.Util
{
	internal sealed class KDTreeNodesPool<T> where T : SpatiallySortable
	{
		private List<KDNode<T>> pool;

		private int currentIndex;

		public KDTreeNodesPool(int initialCapacity)
		{
			pool = new List<KDNode<T>>(initialCapacity);
			currentIndex = -1;
		}

		internal void Reset()
		{
			List<KDNode<T>>.Enumerator enumerator = pool.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KDNode<T> current = enumerator.Current;
					current.data = default(T);
					current.leftChild = null;
					current.rightChild = null;
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			currentIndex = -1;
		}

		internal KDNode<T> GetFreeNode()
		{
			currentIndex++;
			if (currentIndex < pool.Count)
			{
				return pool[currentIndex];
			}
			KDNode<T> kDNode = new KDNode<T>();
			pool.Add(kDNode);
			return kDNode;
		}
	}
}
