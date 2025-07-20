using UnityEngine;

namespace Kampai.Util.Graphics
{
	public static class MaterialModifierExtensions
	{
		public static MaterialModifier SetAlpha(this MaterialModifier modifier, float alpha, bool update = true)
		{
			modifier.SetFloat(GameConstants.ShaderProperties.Alpha, alpha);
			if (update)
			{
				modifier.Update();
			}
			return modifier;
		}

		public static MaterialModifier SetFadeAlpha(this MaterialModifier modifier, float fadeAlpha, bool update = true)
		{
			modifier.SetFloat(GameConstants.ShaderProperties.Procedural.FadeAlpha, fadeAlpha);
			if (update)
			{
				modifier.Update();
			}
			return modifier;
		}

		public static MaterialModifier SetMaterialColor(this MaterialModifier modifier, Color color, bool update = true)
		{
			modifier.SetColor(GameConstants.ShaderProperties.Color.MainColor, color);
			if (update)
			{
				modifier.Update();
			}
			return modifier;
		}

		public static MaterialModifier SetBlendedColor(this MaterialModifier modifier, Color color, bool update = true)
		{
			modifier.SetColor(GameConstants.ShaderProperties.Procedural.BlendedColor, color);
			if (update)
			{
				modifier.Update();
			}
			return modifier;
		}

		public static float GetFadeAlpha(this MaterialModifier modifier)
		{
			return modifier.GetFloat(GameConstants.ShaderProperties.Procedural.FadeAlpha);
		}
	}
}
