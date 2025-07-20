using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util
{
	internal sealed class FadeMaterialBlock : MonoBehaviour
	{
		private const int ClearCacheTimeSeconds = 10;

		private MaterialPropertyBlock propertyBlock;

		private float m_fadeAlpha = 1f;

		private readonly IList<Renderer> m_renderers = new List<Renderer>();

		private readonly Dictionary<ParticleSystem, bool> m_particleSystems = new Dictionary<ParticleSystem, bool>();

		private readonly Dictionary<Renderer, Material[]> m_materialCache = new Dictionary<Renderer, Material[]>();

		private readonly Dictionary<Renderer, Material[]> m_fadeMaterialCache = new Dictionary<Renderer, Material[]>();

		private readonly List<Material> m_fadeMaterials = new List<Material>();

		private bool fadeIn;

		private GoTween m_fadeTween;

		public float zWriteOffValue = 0.15f;

		private Coroutine m_clearCacheCoroutine;

		public float FadeAlpha
		{
			get
			{
				return m_fadeAlpha;
			}
			set
			{
				m_fadeAlpha = value;
				propertyBlock.Clear();
				propertyBlock.SetFloat(GameConstants.ShaderProperties.Alpha, 0f);
				propertyBlock.SetFloat(GameConstants.ShaderProperties.Procedural.FadeAlpha, value);
				Dictionary<Renderer, Material[]> dictionary = ((!(m_fadeAlpha <= zWriteOffValue)) ? m_materialCache : m_fadeMaterialCache);
				int num = 0;
				while (num < m_renderers.Count)
				{
					Renderer renderer = m_renderers[num];
					if (renderer != null)
					{
						renderer.materials = dictionary[renderer];
						renderer.SetPropertyBlock(propertyBlock);
						num++;
					}
					else
					{
						m_renderers.RemoveAt(num);
					}
				}
			}
		}

		private void Awake()
		{
			propertyBlock = new MaterialPropertyBlock();
		}

		private void OnDestroy()
		{
			Clear();
		}

		public void StartFade(bool fadeIn, float duration, List<Renderer> renderers)
		{
			this.fadeIn = fadeIn;
			if (!fadeIn)
			{
				m_fadeAlpha = 1f;
				SetRenderers(renderers);
			}
			if (m_fadeTween != null)
			{
				m_fadeTween.playBackwards();
				return;
			}
			if (m_clearCacheCoroutine != null)
			{
				StopCoroutine(m_clearCacheCoroutine);
				m_clearCacheCoroutine = null;
			}
			m_fadeTween = Go.to(this, duration, new GoTweenConfig().floatProp("FadeAlpha", (!fadeIn) ? 0f : 1f).onComplete(OnCompleteTween));
		}

		private bool SetRenderers(List<Renderer> renderers)
		{
			if (renderers == null || renderers.Count == 0)
			{
				return false;
			}
			Clear();
			for (int i = 0; i < renderers.Count; i++)
			{
				Renderer renderer = renderers[i];
				ParticleSystemRenderer particleSystemRenderer = renderer as ParticleSystemRenderer;
				if (particleSystemRenderer != null)
				{
					ParticleSystem component = particleSystemRenderer.gameObject.GetComponent<ParticleSystem>();
					if (component != null && !m_particleSystems.ContainsKey(component))
					{
						m_particleSystems.Add(component, component.emission.enabled);
						component.SetEmissionEnabled(false);
					}
				}
				else
				{
					m_renderers.Add(renderer);
					if (!m_materialCache.ContainsKey(renderer))
					{
						CheckIfRendererFades(renderer);
					}
				}
			}
			return true;
		}

		private void CheckIfRendererFades(Renderer renderer)
		{
			m_materialCache.Add(renderer, renderer.sharedMaterials);
			Material[] array = new Material[renderer.sharedMaterials.Length];
			for (int i = 0; i < renderer.sharedMaterials.Length; i++)
			{
				Material material = renderer.sharedMaterials[i];
				if (material != null && material.HasProperty(GameConstants.ShaderProperties.ZWrite) && material.HasProperty(GameConstants.ShaderProperties.Procedural.FadeAlpha))
				{
					Material material2 = new Material(renderer.sharedMaterials[i]);
					string[] shaderKeywords = material.shaderKeywords;
					foreach (string keyword in shaderKeywords)
					{
						material2.EnableKeyword(keyword);
					}
					material2.SetFloat(GameConstants.ShaderProperties.ZWrite, 0f);
					material = material2;
					m_fadeMaterials.Add(material2);
				}
				if (material != null)
				{
					array[i] = material;
				}
			}
			m_fadeMaterialCache.Add(renderer, array);
		}

		public void Clear()
		{
			if (m_renderers.Count == 0 && m_particleSystems.Count == 0)
			{
				return;
			}
			if (m_fadeTween != null)
			{
				m_fadeTween.complete();
			}
			for (int i = 0; i < m_renderers.Count; i++)
			{
				Renderer renderer = m_renderers[i];
				if (renderer != null)
				{
					renderer.materials = m_materialCache[renderer];
					renderer.SetPropertyBlock(null);
				}
			}
			foreach (KeyValuePair<ParticleSystem, bool> particleSystem in m_particleSystems)
			{
				if (particleSystem.Key != null)
				{
					particleSystem.Key.SetEmissionEnabled(particleSystem.Value);
				}
			}
			m_particleSystems.Clear();
			m_renderers.Clear();
		}

		private void OnCompleteTween(AbstractGoTween thisTween)
		{
			m_fadeTween.destroy();
			m_fadeTween = null;
			if (fadeIn)
			{
				Clear();
				if (m_clearCacheCoroutine != null)
				{
					StopCoroutine(m_clearCacheCoroutine);
					m_clearCacheCoroutine = null;
				}
				m_clearCacheCoroutine = StartCoroutine(ClearCache());
			}
		}

		private IEnumerator ClearCache()
		{
			yield return new WaitForSeconds(10f);
			if (m_fadeMaterialCache.Count > 0 && m_materialCache.Count > 0 && fadeIn && m_fadeTween == null)
			{
				for (int i = 0; i < m_fadeMaterials.Count; i++)
				{
					Object.DestroyImmediate(m_fadeMaterials[i], true);
				}
				m_fadeMaterials.Clear();
				m_fadeMaterialCache.Clear();
				m_materialCache.Clear();
			}
		}
	}
}
