using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class VignetteView : strange.extensions.mediation.impl.View
	{
		private const string key_VignetteSize = "_Size";

		private float? initialVignetteSize;

		internal void SetVignetteSize(float? size)
		{
			Material material = GetComponent<Renderer>().material;
			if (!initialVignetteSize.HasValue)
			{
				initialVignetteSize = material.GetFloat("_Size");
			}
			float value = initialVignetteSize.Value;
			if (size.HasValue)
			{
				value = size.Value;
			}
			material.SetFloat("_Size", value);
		}
	}
}
