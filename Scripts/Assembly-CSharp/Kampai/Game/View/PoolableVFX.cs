using System.Collections;
using UnityEngine;
using strange.extensions.pool.api;

namespace Kampai.Game.View
{
	public class PoolableVFX : IPoolable
	{
		public GameObject vfxGO { get; private set; }

		public bool retain { get; private set; }

		public PoolableVFX(GameObject prefab)
		{
			vfxGO = Object.Instantiate(prefab);
		}

		public void Restore()
		{
			vfxGO.SetActive(false);
		}

		public void Retain()
		{
			retain = true;
		}

		public void Release()
		{
			vfxGO.SetActive(false);
		}

		public IEnumerator CleanupCoroutine(IPool<PoolableVFX> pool)
		{
			yield return new WaitForSeconds(vfxGO.GetComponent<ParticleSystem>().duration);
			pool.ReturnInstance(this);
		}
	}
}
