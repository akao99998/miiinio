using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class OrderBoardQuestStepController : QuestStepController
	{
		public OrderBoardQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			QuestStep questStep = base.questStep;
			if (questStep.state == QuestStepState.Notstarted)
			{
				GoToNextState();
			}
			if (questTaskTransition == QuestTaskTransition.Harvestable)
			{
				if (questStep.state == QuestStepState.Inprogress)
				{
					questStep.AmountReady++;
					if (questStep.AmountReady + questStep.AmountCompleted >= base.questStepDefinition.ItemAmount)
					{
						GoToNextState();
					}
				}
			}
			else
			{
				if (questStep.state != QuestStepState.Inprogress && questStep.state != QuestStepState.Ready)
				{
					return;
				}
				questStep.AmountCompleted++;
				questStep.AmountReady--;
				if (questStep.AmountCompleted >= base.questStepDefinition.ItemAmount)
				{
					GoToNextState(true);
					if (base.isProceduralQuest)
					{
						gameContext.injectionBinder.GetInstance<UpdateProceduralQuestPanelSignal>().Dispatch(base.QuestInstanceID);
					}
				}
			}
		}

		public override void SetupTracking()
		{
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("orderBoardAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			return localService.GetString("orderboardTaskDesc", base.questStepDefinition.ItemAmount);
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			BuildingDefinition buildingDefinition = defService.Get<BuildingDefinition>(3022);
			mainSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Image);
			maskSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Mask);
		}
	}
}
