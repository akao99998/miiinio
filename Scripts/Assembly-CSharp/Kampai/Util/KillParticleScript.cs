using UnityEngine;

namespace Kampai.Util
{
	public class KillParticleScript : MonoBehaviour
	{
		public ParticleSystem ps;

		public void Update()
		{
			if ((bool)ps && !ps.isPlaying)
			{
				Object.Destroy(base.transform.parent.gameObject);
			}
		}
	}
}
