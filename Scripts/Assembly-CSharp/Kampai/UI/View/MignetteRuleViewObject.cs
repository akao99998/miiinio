using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class MignetteRuleViewObject : MonoBehaviour
	{
		public Text AmountLabel;

		public KampaiImage CauseImage;

		public KampaiImage EffectImage;

		public void RenderRule(MignetteRuleDefinition ruleDefinition)
		{
			StartCoroutine(Wait_thenRenderImages(ruleDefinition));
			AmountLabel.text = ruleDefinition.EffectAmount.ToString();
		}

		private IEnumerator Wait_thenRenderImages(MignetteRuleDefinition ruleDefinition)
		{
			yield return new WaitForEndOfFrame();
			if (!string.IsNullOrEmpty(ruleDefinition.CauseImageMask))
			{
				CauseImage.maskSprite = UIUtils.LoadSpriteFromPath(ruleDefinition.CauseImageMask);
			}
			CauseImage.sprite = UIUtils.LoadSpriteFromPath(ruleDefinition.CauseImage);
			if (!string.IsNullOrEmpty(ruleDefinition.EffectImageMask))
			{
				EffectImage.maskSprite = UIUtils.LoadSpriteFromPath(ruleDefinition.EffectImageMask);
			}
			EffectImage.sprite = UIUtils.LoadSpriteFromPath(ruleDefinition.EffectImage);
		}
	}
}
