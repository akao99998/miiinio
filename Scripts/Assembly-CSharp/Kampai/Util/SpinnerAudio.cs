using System.Collections;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Util
{
	public class SpinnerAudio : MonoBehaviour
	{
		public Signal<string> PlaySFXSignal { get; set; }

		private bool playTickingSound { get; set; }

		public void StartSpinningSound()
		{
			playTickingSound = true;
			StartCoroutine(PlaySpinningSoundCoroutine());
		}

		public void StopSpinningSound()
		{
			playTickingSound = false;
		}

		private IEnumerator PlaySpinningSoundCoroutine()
		{
			yield return new WaitForSeconds(0.5f);
			while (playTickingSound)
			{
				PlaySFXSignal.Dispatch("Play_marketplace_slotTick_01");
				yield return new WaitForSeconds(0.12f);
			}
			PlaySFXSignal.Dispatch("Play_marketplace_slotEnd_01");
		}
	}
}
