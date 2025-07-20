using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class LeisureQuestStepController : QuestStepController
	{
		public LeisureQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			if (questTaskTransition == QuestTaskTransition.Complete && base.questStep.TrackedID == buildingDefId)
			{
				base.questStep.AmountReady++;
				base.questStep.AmountCompleted++;
				if (base.questStep.AmountCompleted >= base.questStepDefinition.ItemAmount)
				{
					GoToTaskState(QuestStepState.Complete);
				}
			}
		}

		public override void SetupTracking()
		{
			base.questStep.TrackedID = base.questStepDefinition.ItemDefinitionID;
			if (base.questStep.TrackedID == 0)
			{
				logger.Fatal(FatalCode.QS_NO_SUCH_TRACKED_LEISURE_ID, "Item definition id not found for {0} Type quests", base.questStepDefinition.Type);
			}
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("leisureAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			BuildingDefinition buildingDefinition = defService.Get<BuildingDefinition>(base.questStepDefinition.ItemDefinitionID);
			string @string = localService.GetString(buildingDefinition.LocalizedKey + "*", base.questStepDefinition.ItemAmount);
			return localService.GetString("leisureTaskDesc", @string);
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			BuildingDefinition buildingDefinition = defService.Get<BuildingDefinition>(base.questStepDefinition.ItemDefinitionID);
			mainSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Image);
			maskSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Mask);
		}
	}
}
