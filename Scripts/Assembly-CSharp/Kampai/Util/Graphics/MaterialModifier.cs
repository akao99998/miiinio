using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util.Graphics
{
	public class MaterialModifier
	{
		public static readonly List<int> FixedFuncProperties = new List<int>
		{
			GameConstants.ShaderProperties.ZWrite,
			GameConstants.ShaderProperties.Cull,
			GameConstants.ShaderProperties.ZTest,
			GameConstants.ShaderProperties.Blend.DstBlend,
			GameConstants.ShaderProperties.Blend.SrcBlend,
			GameConstants.ShaderProperties.Color.ColorMask,
			GameConstants.ShaderProperties.Stencil.Ref
		};

		private Dictionary<int, float> m_floatParams;

		private Dictionary<int, Color> m_colorParams;

		private Dictionary<int, Texture> m_textureParams;

		private Dictionary<int, Vector4> m_vectorParams;

		protected Dictionary<int, float> m_fixedFunctionParams;

		private MaterialPropertyBlock m_propertyBlock;

		private bool m_isDirty;

		private readonly Dictionary<Renderer, Material[]> m_materialCache = new Dictionary<Renderer, Material[]>();

		private Dictionary<Renderer, Material[]> m_instancedMaterialCache;

		private List<Renderer> m_targetRenderers;

		public int PropertyCount { get; private set; }

		public MaterialModifier(IEnumerable<Renderer> renderers)
		{
			m_targetRenderers = new List<Renderer>();
			foreach (Renderer renderer in renderers)
			{
				m_targetRenderers.Add(renderer);
				m_materialCache.Add(renderer, renderer.sharedMaterials);
			}
			m_propertyBlock = new MaterialPropertyBlock();
		}

		public bool IsEmpty()
		{
			return PropertyCount == 0;
		}

		public MaterialModifier SetFloat(int parameterHash, float value)
		{
			if (FixedFuncProperties.Contains(parameterHash))
			{
				m_fixedFunctionParams = m_fixedFunctionParams ?? new Dictionary<int, float>();
				SetValue(m_fixedFunctionParams, value, parameterHash);
				return this;
			}
			m_floatParams = m_floatParams ?? new Dictionary<int, float>();
			SetValue(m_floatParams, value, parameterHash);
			return this;
		}

		public MaterialModifier SetColor(int parameterHash, Color value)
		{
			m_colorParams = m_colorParams ?? new Dictionary<int, Color>();
			SetValue(m_colorParams, value, parameterHash);
			return this;
		}

		public MaterialModifier SetTexture(int parameterHash, Texture value)
		{
			m_textureParams = m_textureParams ?? new Dictionary<int, Texture>();
			SetValue(m_textureParams, value, parameterHash);
			return this;
		}

		public MaterialModifier SetVector(int parameterHash, Vector4 value)
		{
			m_vectorParams = m_vectorParams ?? new Dictionary<int, Vector4>();
			SetValue(m_vectorParams, value, parameterHash);
			return this;
		}

		public MaterialModifier SetFloat(string paramName, float value)
		{
			return SetFloat(Shader.PropertyToID(paramName), value);
		}

		public MaterialModifier SetColor(string paramName, Color value)
		{
			return SetColor(Shader.PropertyToID(paramName), value);
		}

		public MaterialModifier SetTexture(string paramName, Texture value)
		{
			return SetTexture(Shader.PropertyToID(paramName), value);
		}

		public MaterialModifier SetVector(string paramName, Vector4 value)
		{
			return SetVector(Shader.PropertyToID(paramName), value);
		}

		private void SetValue<T>(Dictionary<int, T> dictionary, T value, int paramHash)
		{
			if (dictionary.ContainsKey(paramHash))
			{
				m_isDirty = true;
				dictionary[paramHash] = value;
			}
			else
			{
				m_isDirty = true;
				dictionary.Add(paramHash, value);
				PropertyCount++;
			}
		}

		public bool HasFloat(int parameterHash)
		{
			return HasFixedFunction(parameterHash) || (m_floatParams != null && m_floatParams.ContainsKey(parameterHash));
		}

		private bool HasFixedFunction(int parameterHash)
		{
			return m_fixedFunctionParams != null && m_fixedFunctionParams.ContainsKey(parameterHash);
		}

		public bool HasColor(int parameterHash)
		{
			return m_colorParams != null && m_colorParams.ContainsKey(parameterHash);
		}

		public bool HasTexture(int parameterHash)
		{
			return m_textureParams != null && m_textureParams.ContainsKey(parameterHash);
		}

		public bool HasVector(int parameterHash)
		{
			return m_vectorParams != null && m_vectorParams.ContainsKey(parameterHash);
		}

		public bool HasFloat(string paramName)
		{
			return m_floatParams != null && m_floatParams.ContainsKey(Shader.PropertyToID(paramName));
		}

		public bool HasColor(string paramName)
		{
			return m_colorParams != null && m_colorParams.ContainsKey(Shader.PropertyToID(paramName));
		}

		public bool HasTexture(string paramName)
		{
			return m_textureParams != null && m_textureParams.ContainsKey(Shader.PropertyToID(paramName));
		}

		public bool HasVector(string paramName)
		{
			return m_vectorParams != null && m_vectorParams.ContainsKey(Shader.PropertyToID(paramName));
		}

		public float GetFloat(int parameterHash)
		{
			return GetValue((!HasFixedFunction(parameterHash)) ? m_floatParams : m_fixedFunctionParams, parameterHash);
		}

		public Color GetColor(int parameterHash)
		{
			return GetValue(m_colorParams, parameterHash);
		}

		public Texture GetTexture(int parameterHash)
		{
			return GetValue(m_textureParams, parameterHash);
		}

		public Vector4 GetVector(int parameterHash)
		{
			return GetValue(m_vectorParams, parameterHash);
		}

		public float GetFloat(string paramName)
		{
			return GetFloat(Shader.PropertyToID(paramName));
		}

		public Color GetColor(string paramName)
		{
			return GetColor(Shader.PropertyToID(paramName));
		}

		public Texture GetTexture(string paramName)
		{
			return GetTexture(Shader.PropertyToID(paramName));
		}

		public Vector4 GetVector(string paramName)
		{
			return GetVector(Shader.PropertyToID(paramName));
		}

		private static T GetValue<T>(Dictionary<int, T> dictionary, int parameterHash)
		{
			if (dictionary == null || !dictionary.ContainsKey(parameterHash))
			{
				return default(T);
			}
			return dictionary[parameterHash];
		}

		public MaterialModifier RemoveFloat(int parameterHash)
		{
			RemoveValue((!HasFixedFunction(parameterHash)) ? m_floatParams : m_fixedFunctionParams, parameterHash);
			return this;
		}

		public MaterialModifier RemoveColor(int parameterHash)
		{
			RemoveValue(m_colorParams, parameterHash);
			return this;
		}

		public MaterialModifier RemoveTexture(int parameterHash)
		{
			RemoveValue(m_textureParams, parameterHash);
			return this;
		}

		public MaterialModifier RemoveVector(int parameterHash)
		{
			RemoveValue(m_vectorParams, parameterHash);
			return this;
		}

		public MaterialModifier RemoveFloat(string paramName)
		{
			RemoveValue(m_floatParams, Shader.PropertyToID(paramName));
			return this;
		}

		public MaterialModifier RemoveColor(string paramName)
		{
			RemoveValue(m_colorParams, Shader.PropertyToID(paramName));
			return this;
		}

		public MaterialModifier RemoveTexture(string paramName)
		{
			RemoveValue(m_textureParams, Shader.PropertyToID(paramName));
			return this;
		}

		public MaterialModifier RemoveVector(string paramName)
		{
			RemoveValue(m_vectorParams, Shader.PropertyToID(paramName));
			return this;
		}

		private void RemoveValue<T>(Dictionary<int, T> dictionary, int parameterHash)
		{
			if (dictionary.ContainsKey(parameterHash))
			{
				m_isDirty = true;
				PropertyCount--;
				dictionary.Remove(parameterHash);
				if (dictionary.Count == 0)
				{
					dictionary.Clear();
					dictionary = null;
				}
			}
		}

		public void Update()
		{
			if (m_targetRenderers != null && m_isDirty)
			{
				m_propertyBlock.Clear();
				UpdateFixedFunctions();
				UpdateParams(m_floatParams, m_propertyBlock.SetFloat);
				UpdateParams(m_colorParams, m_propertyBlock.SetColor);
				UpdateParams(m_textureParams, m_propertyBlock.SetTexture);
				UpdateParams(m_vectorParams, m_propertyBlock.SetVector);
				for (int i = 0; i < m_targetRenderers.Count; i++)
				{
					m_targetRenderers[i].SetPropertyBlock(m_propertyBlock);
				}
				m_isDirty = false;
			}
		}

		public void Destroy()
		{
			Reset();
			m_propertyBlock.Clear();
			m_targetRenderers = null;
		}

		public MaterialModifier Reset()
		{
			ResetRenderers();
			CleanInstancedMaterials();
			ClearParams(m_floatParams);
			ClearParams(m_colorParams);
			ClearParams(m_textureParams);
			ClearParams(m_vectorParams);
			return this;
		}

		private void ClearParams<T>(Dictionary<int, T> dict)
		{
			if (dict != null)
			{
				PropertyCount -= dict.Count;
				dict.Clear();
			}
		}

		private void ResetRenderers()
		{
			if (m_targetRenderers != null)
			{
				for (int i = 0; i < m_targetRenderers.Count; i++)
				{
					Renderer renderer = m_targetRenderers[i];
					renderer.materials = m_materialCache[renderer];
					renderer.SetPropertyBlock(null);
				}
			}
		}

		private void CleanInstancedMaterials()
		{
			if (m_instancedMaterialCache == null || m_instancedMaterialCache.Count <= 0 || m_materialCache.Count <= 0)
			{
				return;
			}
			foreach (KeyValuePair<Renderer, Material[]> item in m_instancedMaterialCache)
			{
				if (item.Value != null)
				{
					for (int i = 0; i < item.Value.Length; i++)
					{
						UnityEngine.Object.DestroyImmediate(item.Value[i], true);
					}
				}
			}
			m_instancedMaterialCache.Clear();
			m_instancedMaterialCache = null;
		}

		private void UpdateFixedFunctions()
		{
			if (m_fixedFunctionParams == null || m_fixedFunctionParams.Count == 0)
			{
				return;
			}
			if (m_instancedMaterialCache == null)
			{
				m_instancedMaterialCache = new Dictionary<Renderer, Material[]>();
				foreach (KeyValuePair<Renderer, Material[]> item in m_materialCache)
				{
					Renderer key = item.Key;
					Material[] value = item.Value;
					Material[] array = new Material[value.Length];
					for (int i = 0; i < value.Length; i++)
					{
						array[i] = new Material(value[i]);
					}
					key.materials = array;
					m_instancedMaterialCache.Add(item.Key, array);
				}
			}
			using (IEnumerator<KeyValuePair<int, float>> enumerator2 = (object)m_fixedFunctionParams.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					KeyValuePair<int, float> current2 = enumerator2.Current;
					foreach (Material[] value2 in m_instancedMaterialCache.Values)
					{
						for (int j = 0; j < value2.Length; j++)
						{
							value2[j].SetFloat(current2.Key, current2.Value);
						}
					}
				}
			}
		}

		private void UpdateParams<T>(Dictionary<int, T> dictionary, Action<int, T> updateValueAction)
		{
			if (dictionary == null || dictionary.Count == 0)
			{
				return;
			}
			using (IEnumerator<KeyValuePair<int, T>> enumerator = (object)dictionary.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<int, T> current = enumerator.Current;
					updateValueAction(current.Key, current.Value);
				}
			}
		}
	}
}
