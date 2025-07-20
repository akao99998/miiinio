using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class RewardReminderHUDMediator : EventMediator
	{
		[Inject]
		public RewardReminderHUDView view { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public ShowHUDReminderSignal showReminderSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(positionService);
			view.imageButton.ClickedSignal.AddListener(ClickedReminder);
			showReminderSignal.AddListener(view.ShowReminder);
		}

		public override void OnRemove()
		{
			view.imageButton.ClickedSignal.RemoveListener(ClickedReminder);
			showReminderSignal.RemoveListener(view.ShowReminder);
		}

		private void ClickedReminder()
		{
			if (view.pendingRewardDef != null)
			{
				popupMessageSignal.Dispatch(localizationService.GetString(view.pendingRewardDef.aspirationalLocKey, view.pendingRewardDef.awardAtLevel), PopupMessageType.NORMAL);
			}
		}
	}
}
