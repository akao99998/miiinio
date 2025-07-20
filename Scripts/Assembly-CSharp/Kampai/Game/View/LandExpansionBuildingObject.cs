using System.Collections;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class LandExpansionBuildingObject : BuildingObject
	{
		private BurnLandExpansionSignal burnSignal;

		private string vfxGrassClearing;

		private string burntPrefab;

		private ParticleSystem grassClearingParticles;

		private float seconds = 3f;

		public float BurnTimer { get; set; }

		internal void Burn(BurnLandExpansionSignal burnSignal, int ID, string vfxGrassClearing)
		{
			this.burnSignal = burnSignal;
			this.ID = ID;
			this.vfxGrassClearing = vfxGrassClearing;
			StartCoroutine(BurnSequence());
		}

		private IEnumerator BurnSequence()
		{
			IncrementMaterialRenderQueue(1);
			Go.to(this, seconds, new GoTweenConfig().floatProp("BurnTimer", 0.5f).setEaseType(GoEaseType.Linear).onUpdate(delegate
			{
				for (int i = 0; i < base.objectRenderers.Length; i++)
				{
					Material[] materials = base.objectRenderers[i].materials;
					for (int j = 0; j < materials.Length; j++)
					{
						bool flag = false;
						string[] shaderKeywords = materials[j].shaderKeywords;
						for (int k = 0; k < shaderKeywords.Length; k++)
						{
							if ("ALPHA_CLIP".Equals(shaderKeywords[k]))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							materials[j].SetFloat("_AlphaClip", BurnTimer);
						}
						else
						{
							materials[j].color = new Color(1f, 1f, 1f, 1f - 4f * BurnTimer);
						}
					}
				}
			}));
			GameObject clearingGO = Object.Instantiate(KampaiResources.Load<GameObject>(vfxGrassClearing));
			clearingGO.transform.parent = base.transform;
			clearingGO.transform.localPosition = new Vector3(1f, 1f, -1f);
			grassClearingParticles = clearingGO.GetComponent<ParticleSystem>();
			yield return new WaitForSeconds(1f);
			grassClearingParticles.Stop();
			yield return new WaitForSeconds(seconds);
			burnSignal.Dispatch(ID);
		}

		public override void SetMaterialColor(Color color)
		{
			for (int i = 0; i < base.objectRenderers.Length; i++)
			{
				Renderer renderer = base.objectRenderers[i];
				Material[] materials = renderer.materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j].color = color;
				}
			}
		}

		public override void SetMaterialShaderFloat(string name, float value)
		{
			for (int i = 0; i < base.objectRenderers.Length; i++)
			{
				Material[] materials = base.objectRenderers[i].materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j].SetFloat(name, value);
				}
			}
		}

		public override void IncrementMaterialRenderQueue(int delta)
		{
			for (int i = 0; i < base.objectRenderers.Length; i++)
			{
				Material[] materials = base.objectRenderers[i].materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j].renderQueue += delta;
				}
			}
		}
	}
}
