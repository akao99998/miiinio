using System.Collections.Generic;

namespace Kampai.Util
{
	public class KDNode<T> where T : SpatiallySortable
	{
		private sealed class CompareByXAxis : IComparer<T>
		{
			public int Compare(T a, T b)
			{
				return a.Position.x.CompareTo(b.Position.x);
			}
		}

		private sealed class CompareByZAxis : IComparer<T>
		{
			public int Compare(T a, T b)
			{
				return a.Position.z.CompareTo(b.Position.z);
			}
		}

		internal T data;

		internal KDNode<T> leftChild;

		internal KDNode<T> rightChild;

		private static CompareByXAxis compareByXAxis = new CompareByXAxis();

		private static CompareByZAxis compareByZAxis = new CompareByZAxis();

		public KDNode()
		{
			data = default(T);
			leftChild = null;
			rightChild = null;
		}

		internal void Build(KDTreeNodesPool<T> pool, List<T> objectList, int first, int count, int depth)
		{
			data = default(T);
			leftChild = (rightChild = null);
			if (objectList == null)
			{
				return;
			}
			switch (count)
			{
			case 0:
				return;
			case 1:
				data = objectList[first];
				return;
			}
			if (depth % 2 == 0)
			{
				objectList.Sort(first, count, compareByXAxis);
			}
			else
			{
				objectList.Sort(first, count, compareByZAxis);
			}
			int num = count / 2;
			data = objectList[first + num];
			if (num > 0)
			{
				leftChild = pool.GetFreeNode();
				leftChild.Build(pool, objectList, first, num, depth + 1);
			}
			int num2 = count - (num + 1);
			if (num2 > 0)
			{
				rightChild = pool.GetFreeNode();
				rightChild.Build(pool, objectList, first + num + 1, num2, depth + 1);
			}
		}

		public override string ToString()
		{
			return string.Format("[{0}, {1}, {2}]", (data == null) ? "null" : data.Position.ToString(), (leftChild == null) ? "null" : leftChild.ToString(), (rightChild == null) ? "null" : rightChild.ToString());
		}
	}
}
