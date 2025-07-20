using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kampai.Util.Graphics
{
	public static class ShaderUtils
	{
		[Flags]
		public enum ShaderProperties
		{
			Mode = 1,
			LayerIndex = 2,
			MainTexture = 4,
			AlphaTexture = 8,
			WaterTexture = 0x10,
			UVScroll = 0x20,
			MainColor = 0x40,
			AlphaChannel = 0x80,
			ColorMask = 0x100,
			Boost = 0x200,
			VertexColor = 0x400,
			BlendedColor = 0x800,
			FadeAlpha = 0x1000,
			AlphaTex = 0x2000,
			Overlay = 0x4000,
			Desaturation = 0x8000,
			StencilRef = 0x10000,
			StencilComp = 0x20000,
			StencilReadMask = 0x40000,
			StencilWriteMask = 0x80000,
			StencilPassOp = 0x100000,
			StencilFailOp = 0x200000,
			StencilZFailOp = 0x400000,
			ZWrite = 0x800000,
			ZTest = 0x1000000,
			Cull = 0x2000000,
			AlphaClip = 0x4000000,
			Saturation = 0x8000000,
			Alpha = 0x10000000,
			DstBlend = 0x20000000,
			SrcBlend = 0x40000000,
			TextureValues = 0x201C,
			FloatValues = 0x1F8D9603,
			VectorValues = 0x20,
			ColorValues = 0x48C0,
			StencilOperation = 0x700000,
			BlendOperation = 0x60000000,
			CompareFunc = 0x20000,
			CullMode = 0x2000000
		}

		public static int ConvertShaderBlendToRenderQueueValue(BlendMode mode)
		{
			return (int)ConvertShaderBlendToRenderQueue(mode);
		}

		public static RenderQueue ConvertShaderBlendToRenderQueue(BlendMode mode)
		{
			RenderQueue result = RenderQueue.Geometry;
			switch (mode)
			{
			case BlendMode.Background:
				result = RenderQueue.Background;
				break;
			case BlendMode.Geometry:
				result = RenderQueue.Geometry;
				break;
			case BlendMode.AlphaTest:
				result = RenderQueue.AlphaTest;
				break;
			case BlendMode.Transparent:
				result = RenderQueue.Transparent;
				break;
			case BlendMode.Overlay:
				result = RenderQueue.Overlay;
				break;
			}
			return result;
		}

		public static int GetMaterialRenderQueue(Material material)
		{
			if (material == null)
			{
				return 0;
			}
			int num = -1;
			if (material.HasProperty("_Mode"))
			{
				num = (int)ConvertShaderBlendToRenderQueue((BlendMode)material.GetFloat("_Mode"));
				if (material.HasProperty("_LayerIndex"))
				{
					num += (int)material.GetFloat("_LayerIndex");
				}
			}
			return num;
		}

		public static void EnableStencilShader(Material material, int stencilRef, int count, StencilOperation passOp = StencilOperation.Replace, StencilOperation failOp = StencilOperation.Keep, StencilOperation zFailOp = StencilOperation.Keep)
		{
			material.SetProperty(ShaderProperties.StencilRef, stencilRef);
			material.SetProperty(ShaderProperties.StencilComp, UnityEngine.Rendering.CompareFunction.Equal);
			material.SetProperty(ShaderProperties.StencilReadMask, stencilRef);
			material.SetProperty(ShaderProperties.StencilWriteMask, stencilRef);
			material.SetProperty(ShaderProperties.StencilPassOp, passOp);
			material.SetProperty(ShaderProperties.StencilFailOp, failOp);
			material.SetProperty(ShaderProperties.StencilZFailOp, zFailOp);
			material.SetRenderQueueProperty(BlendMode.Transparent, count);
			material.SetProperty(ShaderProperties.Alpha, 0f);
		}

		public static int GetPropertyHash(ShaderProperties propertyName)
		{
			int result = 0;
			switch (propertyName)
			{
			case ShaderProperties.Mode:
				result = GameConstants.ShaderProperties.Queue.Mode;
				break;
			case ShaderProperties.LayerIndex:
				result = GameConstants.ShaderProperties.Queue.LayerIndex;
				break;
			case ShaderProperties.MainTexture:
				result = GameConstants.ShaderProperties.Texture.MainTexture;
				break;
			case ShaderProperties.AlphaTexture:
				result = GameConstants.ShaderProperties.Texture.AlphaTexture;
				break;
			case ShaderProperties.WaterTexture:
				result = GameConstants.ShaderProperties.Texture.WaterTexture;
				break;
			case ShaderProperties.UVScroll:
				result = GameConstants.ShaderProperties.Texture.UVScroll;
				break;
			case ShaderProperties.AlphaTex:
				result = GameConstants.ShaderProperties.Texture.AlphaTexture;
				break;
			case ShaderProperties.MainColor:
				result = GameConstants.ShaderProperties.Color.MainColor;
				break;
			case ShaderProperties.AlphaChannel:
				result = GameConstants.ShaderProperties.Color.AlphaChannel;
				break;
			case ShaderProperties.ColorMask:
				result = GameConstants.ShaderProperties.Color.ColorMask;
				break;
			case ShaderProperties.Boost:
				result = GameConstants.ShaderProperties.Color.Boost;
				break;
			case ShaderProperties.VertexColor:
				result = GameConstants.ShaderProperties.Color.VertexColor;
				break;
			case ShaderProperties.BlendedColor:
				result = GameConstants.ShaderProperties.Procedural.BlendedColor;
				break;
			case ShaderProperties.FadeAlpha:
				result = GameConstants.ShaderProperties.Procedural.FadeAlpha;
				break;
			case ShaderProperties.Overlay:
				result = GameConstants.ShaderProperties.UI.Overlay;
				break;
			case ShaderProperties.Desaturation:
				result = GameConstants.ShaderProperties.UI.Desaturation;
				break;
			case ShaderProperties.StencilRef:
				result = GameConstants.ShaderProperties.Stencil.Ref;
				break;
			case ShaderProperties.StencilComp:
				result = GameConstants.ShaderProperties.Stencil.Comp;
				break;
			case ShaderProperties.StencilReadMask:
				result = GameConstants.ShaderProperties.Stencil.ReadMask;
				break;
			case ShaderProperties.StencilWriteMask:
				result = GameConstants.ShaderProperties.Stencil.WriteMask;
				break;
			case ShaderProperties.StencilPassOp:
				result = GameConstants.ShaderProperties.Stencil.PassOp;
				break;
			case ShaderProperties.StencilFailOp:
				result = GameConstants.ShaderProperties.Stencil.FailOp;
				break;
			case ShaderProperties.StencilZFailOp:
				result = GameConstants.ShaderProperties.Stencil.ZFailOp;
				break;
			case ShaderProperties.ZWrite:
				result = GameConstants.ShaderProperties.ZWrite;
				break;
			case ShaderProperties.ZTest:
				result = GameConstants.ShaderProperties.ZTest;
				break;
			case ShaderProperties.Cull:
				result = GameConstants.ShaderProperties.Cull;
				break;
			case ShaderProperties.AlphaClip:
				result = GameConstants.ShaderProperties.AlphaClip;
				break;
			case ShaderProperties.Saturation:
				result = GameConstants.ShaderProperties.Saturation;
				break;
			case ShaderProperties.Alpha:
				result = GameConstants.ShaderProperties.Alpha;
				break;
			case ShaderProperties.DstBlend:
				result = GameConstants.ShaderProperties.Blend.DstBlend;
				break;
			case ShaderProperties.SrcBlend:
				result = GameConstants.ShaderProperties.Blend.SrcBlend;
				break;
			}
			return result;
		}
	}
}
