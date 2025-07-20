using System.Collections;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.pool.api;

namespace Kampai.Game.View
{
	public class EnvironmentalMignetteObject : MonoBehaviour
	{
		public enum EnvironmentalMignetteAudioTypes
		{
			Tree = 0
		}

		private IKampaiLogger logger = LogManager.GetClassLogger("EnvironmentalMignetteObject") as IKampaiLogger;

		public EnvironmentalMignetteAudioTypes AudioEffectType;

		public Animator Animator;

		public Transform VfxSpawnPoint;

		private bool IsPlaying;

		private void Start()
		{
			Animator = GetComponentInChildren<Animator>();
		}

		public void PlayEnvironmentalMignetteEffect(IPool<PoolableVFX> vfxPool = null)
		{
			if (Animator != null)
			{
				if (IsPlaying)
				{
					return;
				}
				if (vfxPool != null)
				{
					PoolableVFX instance = vfxPool.GetInstance();
					if (instance != null)
					{
						instance.vfxGO.transform.position = VfxSpawnPoint.position;
						instance.vfxGO.SetActive(true);
						StartCoroutine(instance.CleanupCoroutine(vfxPool));
					}
				}
				StartCoroutine(PlayAnimation());
			}
			else
			{
				logger.Warning("EnvironmentalMignetteObject is missing an animator.");
			}
		}

		private IEnumerator PlayAnimation()
		{
			IsPlaying = true;
			Animator.enabled = true;
			yield return null;
			Animator.Play("Base Layer.Wiggle");
			yield return null;
			float delay = Animator.GetCurrentAnimatorStateInfo(0).length;
			yield return new WaitForSeconds(delay);
			Animator.enabled = false;
			IsPlaying = false;
		}

		public string GetAudioStringConst()
		{
			if (AudioEffectType == EnvironmentalMignetteAudioTypes.Tree)
			{
				return "Play_tree_shake_01";
			}
			return string.Empty;
		}
	}
}
