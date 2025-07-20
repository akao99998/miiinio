using System;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	internal sealed class MasterPlanRewardMediator : UIStackMediator<MasterPlanRewardView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MasterPlanRewardMediator") as IKampaiLogger;

		private TransactionDefinition reward;

		private int masterPlanInstanceID;

		private bool collected;

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public SpawnDooberSignal tweenSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public DoobersFlownSignal doobersFlownSignal { get; set; }

		[Inject]
		public EnableBuildMenuFromLairSignal setBuildMenuEnabledSignal { get; set; }

		[Inject]
		public DisableBuildMenuButtonSignal disableBuildMenuSignal { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public GenerateNewMasterPlanSignal generateNewMasterPlanSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			soundFXSignal.Dispatch("Play_completeQuest_01");
			base.view.collectButton.ClickedSignal.AddListener(CollectButton);
			base.view.OnMenuClose.AddListener(CloseAnimationComplete);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			reward = null;
			base.view.collectButton.ClickedSignal.RemoveListener(CollectButton);
			base.view.OnMenuClose.RemoveListener(CloseAnimationComplete);
		}

		public override void Initialize(GUIArguments args)
		{
			reward = args.Get<TransactionDefinition>();
			masterPlanInstanceID = args.Get<int>();
			soundFXSignal.Dispatch("Play_menu_popUp_01");
			base.view.Init(reward, localService, definitionService, playerService, fancyUIService, guiService, masterPlanInstanceID);
		}

		protected override void Close()
		{
			CollectButton();
		}

		public void CollectButton()
		{
			if (collected)
			{
				return;
			}
			collected = true;
			Button component = base.view.collectButton.GetComponent<Button>();
			component.interactable = false;
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			currentMasterPlan.displayCooldownReward = false;
			if (reward.GetOutputItem(0).ID == currentMasterPlan.Definition.LeavebehindBuildingDefID)
			{
				setBuildMenuEnabledSignal.Dispatch(true);
				disableBuildMenuSignal.Dispatch(true);
			}
			if (reward != null)
			{
				if (masterPlanInstanceID != 0)
				{
					playerService.AlterQuantity(StaticItem.MASTER_PLAN_COMPLETION_COUNT, 1);
					currentMasterPlan.completionCount++;
				}
				playerService.RunEntireTransaction(reward, TransactionTarget.AUTOMATIC, CollectTransactionCallback);
			}
			else
			{
				logger.Info("Reward is null, nothing to do.");
			}
		}

		public void CollectTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (!pct.Success)
			{
				logger.Error("CollectTransactionCallback PendingCurrencyTransaction was a failure.");
			}
			generateNewMasterPlanSignal.Dispatch(new Boxed<Action>(null));
			DooberUtil.CheckForTween(base.view.transactionDefinition, base.view.viewList, true, uiCamera, tweenSignal, definitionService);
			doobersFlownSignal.AddOnce(delegate
			{
				if (villainLairModel.currentActiveLair != null)
				{
					setBuildMenuEnabledSignal.Dispatch(false);
				}
				disableBuildMenuSignal.Dispatch(false);
			});
			CloseMenu();
		}

		private void CloseMenu()
		{
			base.view.Close();
		}

		public void CloseAnimationComplete()
		{
			hideSignal.Dispatch("MasterPlan");
			if (masterPlanInstanceID == 0)
			{
				guiService.Execute(GUIOperation.Unload, "screen_MasterPlanReward");
			}
			else
			{
				guiService.Execute(GUIOperation.Unload, "screen_MasterPlanCooldownReward");
			}
		}
	}
}
