using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class UpgradeMinionQuestStepController : QuestStepController
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
				if (UpgradeLevel == 0)
				{
					return base.questStep.AmountCompleted;
				}
				int num = CurrentAmtUpgraded();
				return (base.questStepDefinition.ItemAmount >= num) ? num : base.questStepDefinition.ItemAmount;
			}
		}

		public override int AmountNeeded
		{
			get
			{
				return base.questStepDefinition.ItemAmount - AmountCompleted;
			}
		}

		public UpgradeMinionQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
		}

		private int CurrentAmtUpgraded()
		{
			return playerService.GetMinionCountAtOrAboveLevel(UpgradeLevel);
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			if (questTaskTransition == QuestTaskTransition.Complete)
			{
				base.questStep.AmountCompleted++;
			}
			if (AmountNeeded < 1)
			{
				if (base.questStep.state == QuestStepState.Notstarted)
				{
					GoToTaskState(QuestStepState.WaitComplete);
				}
				else if (questTaskTransition == QuestTaskTransition.Complete && base.questStep.state != QuestStepState.RunningStartScript)
				{
					GoToNextState(true);
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
			return localService.GetString("LevelUpAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			return localService.GetString("Minions*", base.questStepDefinition.ItemAmount);
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			ItemDefinition itemDefinition = defService.Get<ItemDefinition>(base.questStepDefinition.ItemDefinitionID);
			mainSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
		}
	}
}
