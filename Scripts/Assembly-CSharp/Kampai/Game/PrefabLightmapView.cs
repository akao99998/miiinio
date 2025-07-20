using System;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class PrefabLightmapView : MonoBehaviour
	{
		[Serializable]
		public struct RendererInfo
		{
			public Renderer renderer;

			public int lightmapIndex;

			public Vector4 lightmapOffsetScale;

			public void SetValues(int lightmapOffsetIndex)
			{
				if (!(renderer == null))
				{
					renderer.lightmapIndex = lightmapOffsetIndex + lightmapIndex;
					renderer.lightmapScaleOffset = lightmapOffsetScale;
				}
			}

			public override string ToString()
			{
				return string.Format("renderer: {0}, lightmap index {1}, scale {2}", renderer, lightmapIndex, lightmapOffsetScale);
			}
		}

		public const int INVALID_LIGHTMAP_INDEX = -1;

		public const int NO_LIGHTMAP_INDEX = 65534;

		[SerializeField]
		public RendererInfo[] m_RendererInfo;

		public int LightmapCount;

		public string SceneName;

		public static bool IsValidRenderer(Renderer renderer)
		{
			return renderer != null && IsValidRenderer(renderer.lightmapIndex);
		}

		public static bool IsValidRenderer(RendererInfo renderer)
		{
			return IsValidRenderer(renderer.lightmapIndex);
		}

		public static bool IsValidRenderer(int lightmapIndex)
		{
			return lightmapIndex != -1 && lightmapIndex < 65534;
		}

		private void Awake()
		{
			if (m_RendererInfo != null && m_RendererInfo.Length != 0)
			{
				LoadLightMaps();
			}
		}

		private void LoadLightMaps()
		{
			int num = LightmapSettings.lightmaps.Length;
			int num2 = num + LightmapCount;
			LightmapData[] array = new LightmapData[num2];
			LightmapSettings.lightmaps.CopyTo(array, 0);
			for (int i = 0; i < LightmapCount; i++)
			{
				string path = string.Format("{0}_Lightmap-{1}_comp_light", SceneName, i);
				array[num + i] = new LightmapData
				{
					lightmapFar = KampaiResources.Load<Texture2D>(path)
				};
			}
			ApplyRendererInfo(m_RendererInfo, num);
			LightmapSettings.lightmaps = array;
		}

		private static void ApplyRendererInfo(RendererInfo[] infos, int lightmapOffsetIndex)
		{
			foreach (RendererInfo rendererInfo in infos)
			{
				rendererInfo.SetValues(lightmapOffsetIndex);
			}
		}
	}
}
