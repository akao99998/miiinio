using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class MignetteQuestStepController : QuestStepController
	{
		public MignetteQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			QuestStepState state = base.questStep.state;
			if (base.questStepDefinition.ItemDefinitionID != building.Definition.ID)
			{
				return;
			}
			if (questTaskTransition == QuestTaskTransition.Start && state == QuestStepState.Notstarted)
			{
				GoToNextState();
			}
			else if (questTaskTransition == QuestTaskTransition.Complete && state == QuestStepState.Inprogress)
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
			int itemDefinitionID = base.questStepDefinition.ItemDefinitionID;
			MignetteBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MignetteBuilding>(itemDefinitionID);
			if (firstInstanceByDefinitionId == null)
			{
				logger.Fatal(FatalCode.PS_MISSING_MIGNETTE, "Mignette instance not found!");
			}
			else
			{
				base.questStep.TrackedID = firstInstanceByDefinitionId.ID;
			}
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("mignetteAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			BuildingDefinition buildingDefinition = defService.Get<BuildingDefinition>(base.questStepDefinition.ItemDefinitionID);
			string @string = localService.GetString(buildingDefinition.LocalizedKey);
			return localService.GetString("mignetteTaskDescWrap", @string, localService.GetString("mignetteTaskDesc*", base.questStepDefinition.ItemAmount));
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			BuildingDefinition buildingDefinition = defService.Get<BuildingDefinition>(base.questStepDefinition.ItemDefinitionID);
			mainSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Image);
			maskSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Mask);
		}
	}
}
