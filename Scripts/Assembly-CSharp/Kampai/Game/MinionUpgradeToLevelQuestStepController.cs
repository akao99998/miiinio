using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class MinionUpgradeToLevelQuestStepController : QuestStepController
	{
		public int UpgradeLevel
		{
			get
			{
				return base.questStepDefinition.UpgradeLevel;
			}
		}

		public int AmountCompleted
		{
			get
			{
				int num = CurrentAmtUpgraded();
				base.questStep.AmountCompleted = ((base.questStepDefinition.ItemAmount >= num) ? num : base.questStepDefinition.ItemAmount);
				return base.questStep.AmountCompleted;
			}
		}

		public override int AmountNeeded
		{
			get
			{
				return base.questStepDefinition.ItemAmount - AmountCompleted;
			}
		}

		public MinionUpgradeToLevelQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
		}

		private int CurrentAmtUpgraded()
		{
			return playerService.GetMinionCountAtOrAboveLevel(UpgradeLevel);
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			if (AmountNeeded < 1)
			{
				if (base.questStep.state == QuestStepState.Notstarted)
				{
					GoToTaskState(QuestStepState.WaitComplete);
				}
				else
				{
					GoToTaskState(QuestStepState.Complete);
				}
			}
			else if (base.questStep.state == QuestStepState.Notstarted)
			{
				GoToNextState();
			}
		}

		public override void SetupTracking()
		{
			base.questStep.TrackedID = 375;
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("haveAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			return MinionTaskQuestStepController.GetDescription(localService, UpgradeLevel, base.questStepDefinition);
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			mainSprite = UIUtils.LoadSpriteFromPath("icn_populationGoals_fill");
			maskSprite = UIUtils.LoadSpriteFromPath("icn_populationGoals_mask");
		}
	}
}
