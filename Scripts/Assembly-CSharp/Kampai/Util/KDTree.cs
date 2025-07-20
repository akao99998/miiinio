using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util
{
	public class KDTree<T> : KDNode<T> where T : SpatiallySortable
	{
		private List<T> objectList;

		private List<T> foundObjects;

		public int Count { get; private set; }

		public KDTree(List<T> objectList)
		{
			Count = objectList.Count;
			foundObjects = new List<T>(Count);
			this.objectList = objectList;
		}

		public void Rebuild(List<T> objectList)
		{
			Count = objectList.Count;
			this.objectList = objectList;
		}

		public List<T> WithinRange(Vector3 center, float size)
		{
			foundObjects.Clear();
			AABox2D aABox2D = new AABox2D(center.x, center.z, size, size);
			for (int i = 0; i < objectList.Count; i++)
			{
				T item = objectList[i];
				if (aABox2D.Contains(new Vector2(item.Position.x, item.Position.z)))
				{
					foundObjects.Add(item);
				}
			}
			return foundObjects;
		}

		private void WithinRangeRecursive(AABox2D box, KDNode<T> node, int depth, AABox2D space, ref List<T> found)
		{
			if (node != null && box.Overlaps(space))
			{
				Vector3 position = node.data.Position;
				if (box.Contains(new Vector2(position.x, position.z)))
				{
					found.Add(node.data);
				}
				int num = depth % 2;
				float v = position.x;
				if (num == 1)
				{
					v = position.z;
				}
				AABox2D left;
				AABox2D right;
				space.Split(v, num, out left, out right);
				WithinRangeRecursive(box, node.leftChild, depth + 1, left, ref found);
				WithinRangeRecursive(box, node.rightChild, depth + 1, right, ref found);
			}
		}
	}
}
