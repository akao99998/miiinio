using Kampai.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class RewardedAdRewardView : MonoBehaviour
	{
		public KampaiImage RewardItemImage;

		public Text RewardAmountText;

		public ParticleSystem FlashParticleSystem;

		private void Start()
		{
			Transform transform = base.gameObject.transform;
			transform.localScale /= 3f;
			GoTween tween = new GoTween(transform, 1f, new GoTweenConfig().setEaseType(GoEaseType.CubicInOut).scale(1f));
			GoTweenFlow goTweenFlow = new GoTweenFlow();
			goTweenFlow.insert(0f, tween);
			goTweenFlow.play();
		}

		internal void Init(ItemDefinition rewardItem, int rewardAmount, bool playVFX = true)
		{
			RewardItemImage.sprite = UIUtils.LoadSpriteFromPath(rewardItem.Image);
			RewardItemImage.maskSprite = UIUtils.LoadSpriteFromPath(rewardItem.Mask);
			RewardAmountText.text = UIUtils.FormatLargeNumber(rewardAmount);
			if (!playVFX)
			{
				FlashParticleSystem.gameObject.SetActive(false);
			}
		}
	}
}
