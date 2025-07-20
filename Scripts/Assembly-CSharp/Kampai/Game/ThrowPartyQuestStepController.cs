using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class ThrowPartyQuestStepController : QuestStepController
	{
		public override bool NeedActiveProgressBar
		{
			get
			{
				if (playerService.GetHighestFtueCompleted() < 999999)
				{
					return false;
				}
				return true;
			}
		}

		public ThrowPartyQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			if (base.questStep.state == QuestStepState.Notstarted)
			{
				GoToNextState();
			}
			if ((playerService.GetMinionPartyInstance().IsPartyHappening || questTaskTransition == QuestTaskTransition.Complete) && base.questStep.state == QuestStepState.Inprogress)
			{
				base.questStep.AmountCompleted++;
				if (base.questStep.AmountCompleted >= base.questStepDefinition.ItemAmount)
				{
					GoToTaskState(QuestStepState.Complete);
				}
			}
		}

		public override void SetupTracking()
		{
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("throwAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			return localService.GetString("throwParty");
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			mainSprite = UIUtils.LoadSpriteFromPath("img_throwparty_fill");
			maskSprite = UIUtils.LoadSpriteFromPath("img_throwparty_mask");
		}
	}
}
