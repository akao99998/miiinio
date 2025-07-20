using UnityEngine;

namespace Kampai.Game
{
	public static class AudioCommandHelper
	{
		public static void playSound(AudioSource source, string audioClip, bool oneShot)
		{
			if ((source.clip == null || source.clip.name != audioClip) && !oneShot)
			{
				source.clip = Resources.Load(audioClip) as AudioClip;
				source.Play();
			}
			else if (oneShot)
			{
				AudioClip clip = Resources.Load(audioClip) as AudioClip;
				source.PlayOneShot(clip);
			}
		}
	}
}
