using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class MysteryBoxOnboardingQuestStepController : QuestStepController
	{
		private int benefitBuildingId;

		private IDefinitionService definitionService;

		public MysteryBoxOnboardingQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
			definitionService = gameContext.injectionBinder.GetInstance<IDefinitionService>();
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			if (base.questStep.state == QuestStepState.Notstarted)
			{
				GoToNextState();
			}
			QuestStepDefinition questStepDefinition = base.questStepDefinition;
			QuestStep questStep = base.questStep;
			ResourceBuilding resourceBuilding = building as ResourceBuilding;
			if (resourceBuilding == null || buildingDefId != benefitBuildingId)
			{
				return;
			}
			if (questTaskTransition == QuestTaskTransition.Complete)
			{
				questStep.AmountCompleted++;
			}
			if (questStep.AmountCompleted >= questStepDefinition.ItemAmount)
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
		}

		public override void SetupTracking()
		{
			if (benefitBuildingId == 0)
			{
				benefitBuildingId = definitionService.Get<MinionBenefitLevelBandDefintion>(StaticItem.MINION_BENEFITS_DEF_ID).FirstBuildingId;
			}
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("mysteryBoxAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			return localService.GetString("mysteryBoxDesc");
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(StaticItem.RANDOM_RANDOMDROP_ITEM);
			mainSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
		}
	}
}
