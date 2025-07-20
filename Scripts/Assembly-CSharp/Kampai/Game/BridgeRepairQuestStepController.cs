using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class BridgeRepairQuestStepController : QuestStepController
	{
		private IDefinitionService definitionService;

		private Environment environment;

		public BridgeRepairQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, IDefinitionService definitionService, ICrossContextCapable gameContext, IKampaiLogger logger, Environment environment)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
			this.environment = environment;
			this.definitionService = definitionService;
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			if (base.questStep.TrackedID == building.ID)
			{
				if (questTaskTransition == QuestTaskTransition.Start && base.questStep.state == QuestStepState.Notstarted)
				{
					GoToNextState();
				}
				if (questTaskTransition == QuestTaskTransition.Complete)
				{
					GoToTaskState(QuestStepState.Complete);
				}
			}
		}

		public override void SetupTracking()
		{
			Definition definition = definitionService.Get(base.questStepDefinition.ItemDefinitionID);
			if (definition == null)
			{
				logger.Fatal(FatalCode.DS_NO_BRIDGE_DEF, "Bridge definition not found");
				return;
			}
			BridgeDefinition bridgeDefinition = definition as BridgeDefinition;
			if (bridgeDefinition != null)
			{
				Building building = environment.GetBuilding(bridgeDefinition.location.x, bridgeDefinition.location.y);
				if (building == null)
				{
					logger.Fatal(FatalCode.QS_BUILDING_MISSING, "Building not found in environment");
				}
				base.questStep.TrackedID = building.ID;
			}
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("repairAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			return localService.GetString("repairBridgeDesc");
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			ItemDefinition itemDefinition = defService.Get<ItemDefinition>(base.questStepDefinition.ItemDefinitionID);
			mainSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
		}
	}
}
