using Kampai.Game;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class RewardReminderHUDView : strange.extensions.mediation.impl.View
	{
		public KampaiImage rewardImage;

		public ButtonView imageButton;

		internal PendingRewardDefinition pendingRewardDef;

		public void Init(IPositionService positionService)
		{
			positionService.AddHUDElementToAvoid(base.gameObject);
		}

		public void SetImage(string imageName, string maskName)
		{
			rewardImage.sprite = UIUtils.LoadSpriteFromPath(imageName);
			rewardImage.maskSprite = UIUtils.LoadSpriteFromPath(maskName);
		}

		public void SetArguments(PendingRewardDefinition prd)
		{
			SetImage(prd.hudReminderImage, prd.hudReminderMask);
			pendingRewardDef = prd;
		}

		public void ShowReminder(bool show)
		{
			imageButton.gameObject.SetActive(show);
		}
	}
}
