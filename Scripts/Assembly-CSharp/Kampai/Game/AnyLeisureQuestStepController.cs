using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class AnyLeisureQuestStepController : QuestStepController
	{
		public AnyLeisureQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			if (base.questStep.state == QuestStepState.Notstarted)
			{
				GoToNextState();
			}
			if (base.questStep.state == QuestStepState.Inprogress)
			{
				base.questStep.AmountCompleted++;
				if (base.questStep.AmountCompleted >= base.questStepDefinition.ItemAmount)
				{
					GoToNextState(true);
				}
			}
		}

		public override void SetupTracking()
		{
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			if (StepType == QuestStepType.PlayAnyLeisure)
			{
				return localService.GetString("leisureAction");
			}
			return localService.GetString("harvestAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			return localService.GetString("anyLeisureBuildingDesc");
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			mainSprite = UIUtils.LoadSpriteFromPath("img_fill_32_Yellow");
			maskSprite = UIUtils.LoadSpriteFromPath(string.Format("icn_build_mask_cat_{0}", "leisure"));
		}
	}
}
