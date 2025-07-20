using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Splash;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game.View
{
	public class LevelUpCommand : Command
	{
		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public AwardLevelSignal awardLevelSignal { get; set; }

		[Inject]
		public IPrestigeService characterService { get; set; }

		[Inject]
		public GetNewQuestSignal getNewQuestSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public UpdateForSaleSignsSignal updateForSaleSignsSignal { get; set; }

		[Inject]
		public UpdateMarketplaceRepairStateSignal updateMarketplaceSignal { get; set; }

		[Inject]
		public UnlockCharacterModel model { get; set; }

		[Inject]
		public DisplayNotificationReminderSignal notificationSignal { get; set; }

		[Inject]
		public ILoadInService loadInService { get; set; }

		[Inject]
		public ReconcileSalesSignal reconcileSalesSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllMenuSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		public override void Execute()
		{
			playerService.AlterQuantity(StaticItem.LEVEL_ID, 1);
			loadInService.SaveTipsForNextLaunch((int)playerService.GetQuantity(StaticItem.LEVEL_ID));
			if (model.characterUnlocks.Count == 0)
			{
				closeAllMenuSignal.Dispatch(null);
				uiContext.injectionBinder.GetInstance<DisplayLevelUpRewardSignal>().Dispatch(true);
			}
			TransactionDefinition rewardTransaction = RewardUtil.GetRewardTransaction(definitionService, playerService);
			awardLevelSignal.Dispatch(rewardTransaction);
			characterService.UpdateEligiblePrestigeList();
			getNewQuestSignal.Dispatch();
			telemetryService.Send_Telemetry_EVT_GP_LEVEL_PROMOTION();
			playerDurationService.MarkLevelUpUTC();
			updateMarketplaceSignal.Dispatch();
			updateForSaleSignsSignal.Dispatch();
			if (playerService.GetHighestFtueCompleted() >= 999999)
			{
				reconcileSalesSignal.Dispatch(0);
			}
			CheckNotifications();
		}

		private void CheckNotifications()
		{
			if (Native.AreNotificationsEnabled())
			{
				return;
			}
			NotificationSystemDefinition notificationSystemDefinition = definitionService.Get<NotificationSystemDefinition>(66666);
			foreach (NotificationReminder notificationReminder in notificationSystemDefinition.notificationReminders)
			{
				if (notificationReminder.level == playerService.GetQuantity(StaticItem.LEVEL_ID))
				{
					notificationSignal.Dispatch(localizationService.GetString(notificationReminder.messageLocalizedKey), true);
				}
			}
		}
	}
}
