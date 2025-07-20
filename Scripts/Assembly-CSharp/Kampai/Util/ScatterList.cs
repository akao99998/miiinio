using System;
using System.Collections.Generic;
using Kampai.Common;

namespace Kampai.Util
{
	public class ScatterList<T>
	{
		private bool dirty = true;

		private int size;

		private int index;

		private T[] items;

		private List<T> insertions;

		public int MaxSize { get; private set; }

		public ScatterList(int maxSize)
		{
			if (maxSize <= 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			MaxSize = maxSize;
		}

		public void Add(T item)
		{
			dirty = true;
			if (insertions == null)
			{
				insertions = new List<T>();
			}
			insertions.Add(item);
		}

		public T Pick(IRandomService randomService)
		{
			if (dirty)
			{
				Dirty(randomService);
			}
			if (size > 0)
			{
				if (index >= size)
				{
					ListUtil.Shuffle(randomService, items);
					index = 0;
				}
				return items[index++];
			}
			return default(T);
		}

		public void ShuffleContents(IRandomService randomService)
		{
			ListUtil.Shuffle(randomService, items);
			index = 0;
		}

		private void Dirty(IRandomService randomService)
		{
			if (insertions != null && insertions.Count > 0)
			{
				ListUtil.Shuffle(randomService, insertions);
				size = Math.Min(MaxSize, insertions.Count);
				index = 0;
				items = new T[size];
				for (int i = 0; i < size; i++)
				{
					items[i] = insertions[i];
				}
				insertions = null;
				dirty = false;
			}
		}

		public void Clear()
		{
			if (insertions != null)
			{
				insertions.Clear();
			}
		}
	}
}
