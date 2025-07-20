using UnityEngine;

namespace Kampai.Util
{
	public class ParticleInstantiationScript : MonoBehaviour
	{
		private void Start()
		{
			PlayerParticle("fx_smokePoofTest");
		}

		public void PlayerParticle(string path)
		{
			Object.Instantiate(Resources.Load("VFX/FTEE/" + path));
		}

		private void Update()
		{
		}
	}
}
