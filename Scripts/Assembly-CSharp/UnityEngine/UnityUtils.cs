namespace UnityEngine
{
	public static class UnityUtils
	{
		public static void SetEmissionEnabled(this ParticleSystem ps, bool enabled)
		{
			ParticleSystem.EmissionModule emission = ps.emission;
			emission.enabled = enabled;
		}
	}
}
