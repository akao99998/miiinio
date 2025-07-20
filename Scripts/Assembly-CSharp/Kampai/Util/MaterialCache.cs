using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util
{
	public class MaterialCache
	{
		private sealed class CacheEntry
		{
			public int refCount;

			public Material material;
		}

		private const int CheckCount = 50;

		private readonly Dictionary<int, CacheEntry> m_materialCache;

		public MaterialCache()
		{
			m_materialCache = new Dictionary<int, CacheEntry>();
		}

		public void RemoveReference(int hashCode)
		{
			if (m_materialCache.ContainsKey(hashCode))
			{
				CacheEntry cacheEntry = m_materialCache[hashCode];
				if (--cacheEntry.refCount <= 0)
				{
					Object.DestroyImmediate(cacheEntry.material, true);
					m_materialCache.Remove(hashCode);
				}
			}
		}

		public Material GetMaterial(int hashCode, Material defaultMaterial)
		{
			if (m_materialCache.ContainsKey(hashCode))
			{
				CacheEntry cacheEntry = m_materialCache[hashCode];
				cacheEntry.refCount++;
				return cacheEntry.material;
			}
			CacheEntry cacheEntry2 = new CacheEntry();
			cacheEntry2.refCount = 1;
			cacheEntry2.material = new Material(defaultMaterial);
			CacheEntry cacheEntry3 = cacheEntry2;
			m_materialCache.Add(hashCode, cacheEntry3);
			return cacheEntry3.material;
		}
	}
}
