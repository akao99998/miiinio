using UnityEngine;
using UnityEngine.Rendering;

namespace Kampai.Util.Graphics
{
	public static class MaterialPropertyExtensions
	{
		private static bool IsValidProperty(Material material, int propertyHash, ShaderUtils.ShaderProperties property, ShaderUtils.ShaderProperties propertyGroup)
		{
			return material != null && (property & propertyGroup) != 0 && material.HasProperty(propertyHash);
		}

		public static void SetRenderQueueProperty(this Material material, BlendMode blendMode, int layerIndex = 0)
		{
			if (!(material == null))
			{
				int propertyHash = ShaderUtils.GetPropertyHash(ShaderUtils.ShaderProperties.Mode);
				material.SetFloat(propertyHash, (float)blendMode);
				propertyHash = ShaderUtils.GetPropertyHash(ShaderUtils.ShaderProperties.LayerIndex);
				material.SetFloat(propertyHash, layerIndex);
				material.renderQueue = ShaderUtils.ConvertShaderBlendToRenderQueueValue(blendMode) + layerIndex;
			}
		}

		public static void SetProperty(this Material material, ShaderUtils.ShaderProperties property, float value)
		{
			int propertyHash = ShaderUtils.GetPropertyHash(property);
			if (IsValidProperty(material, propertyHash, property, ShaderUtils.ShaderProperties.FloatValues))
			{
				material.SetFloat(propertyHash, value);
			}
		}

		public static void SetProperty(this Material material, ShaderUtils.ShaderProperties property, ColorMask value)
		{
			int propertyHash = ShaderUtils.GetPropertyHash(property);
			if (IsValidProperty(material, propertyHash, property, ShaderUtils.ShaderProperties.ColorMask))
			{
				material.SetFloat(propertyHash, (float)value);
			}
		}

		public static void SetProperty(this Material material, ShaderUtils.ShaderProperties property, BlendOp value)
		{
			int propertyHash = ShaderUtils.GetPropertyHash(property);
			if (IsValidProperty(material, propertyHash, property, ShaderUtils.ShaderProperties.BlendOperation))
			{
				material.SetFloat(propertyHash, (float)value);
			}
		}

		public static void SetProperty(this Material material, ShaderUtils.ShaderProperties property, CullMode value)
		{
			int propertyHash = ShaderUtils.GetPropertyHash(property);
			if (IsValidProperty(material, propertyHash, property, ShaderUtils.ShaderProperties.Cull))
			{
				material.SetFloat(propertyHash, (float)value);
			}
		}

		public static void SetProperty(this Material material, ShaderUtils.ShaderProperties property, UnityEngine.Rendering.CompareFunction value)
		{
			int propertyHash = ShaderUtils.GetPropertyHash(property);
			if (IsValidProperty(material, propertyHash, property, ShaderUtils.ShaderProperties.StencilComp))
			{
				material.SetFloat(propertyHash, (float)value);
			}
		}

		public static void SetProperty(this Material material, ShaderUtils.ShaderProperties property, StencilOperation value)
		{
			int propertyHash = ShaderUtils.GetPropertyHash(property);
			if (IsValidProperty(material, propertyHash, property, ShaderUtils.ShaderProperties.StencilOperation))
			{
				material.SetFloat(propertyHash, (float)value);
			}
		}

		public static void SetProperty(this Material material, ShaderUtils.ShaderProperties property, Color value)
		{
			int propertyHash = ShaderUtils.GetPropertyHash(property);
			if (IsValidProperty(material, propertyHash, property, ShaderUtils.ShaderProperties.ColorValues))
			{
				material.SetColor(propertyHash, value);
			}
		}

		public static void SetProperty(this Material material, ShaderUtils.ShaderProperties property, Texture value)
		{
			int propertyHash = ShaderUtils.GetPropertyHash(property);
			if (IsValidProperty(material, propertyHash, property, ShaderUtils.ShaderProperties.TextureValues))
			{
				material.SetTexture(propertyHash, value);
			}
		}
	}
}
