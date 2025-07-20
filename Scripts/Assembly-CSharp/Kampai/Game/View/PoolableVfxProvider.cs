using System;
using UnityEngine;
using strange.framework.api;

namespace Kampai.Game.View
{
	public class PoolableVfxProvider : IInstanceProvider
	{
		private GameObject prefab;

		public PoolableVfxProvider(GameObject prefab)
		{
			this.prefab = prefab;
		}

		public T GetInstance<T>()
		{
			return (T)GetInstance(typeof(T));
		}

		public object GetInstance(Type key)
		{
			return new PoolableVFX(prefab);
		}
	}
}
