using Kampai.Game.Mignette.View;
using Kampai.Main;
using UnityEngine;

namespace Kampai.Game.Mignette.ButterflyCatch.View
{
	public class ButterflyCatchBuildingViewObject : MignetteBuildingViewObject
	{
		public GameObject ButterflyParticleParent;

		public float YOffset = 1f;

		private ParticleSystem[] AmbientButterflies;

		public void Start()
		{
			AmbientButterflies = ButterflyParticleParent.GetComponentsInChildren<ParticleSystem>();
			ToggleAmbientButterflies(true);
			base.gameObject.AddComponent<MignetteBuildingCooldownView>();
			Vector3 position = base.gameObject.transform.position;
			base.gameObject.transform.position = new Vector3(position.x, position.y + YOffset, position.z);
		}

		public override void ResetCooldownView(PlayLocalAudioSignal localAudioSignal)
		{
			ToggleAmbientButterflies(true);
		}

		public override void UpdateCooldownView(PlayLocalAudioSignal localAudioSignal, int buildingData, float pctDone)
		{
			if (pctDone < 1f)
			{
				ToggleAmbientButterflies(false);
			}
		}

		public void ToggleAmbientButterflies(bool enable)
		{
			if (AmbientButterflies != null)
			{
				ParticleSystem[] ambientButterflies = AmbientButterflies;
				foreach (ParticleSystem ps in ambientButterflies)
				{
					ps.SetEmissionEnabled(enable);
				}
			}
		}
	}
}
