using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class DeliveryQuestStepController : QuestStepController
	{
		private IDefinitionService definitionService;

		public override string DeliverButtonLocKey
		{
			get
			{
				return "Deliver";
			}
		}

		public override bool NeedActiveDeliverButton
		{
			get
			{
				QuestStepState stepState = StepState;
				return stepState == QuestStepState.Inprogress || stepState == QuestStepState.Notstarted || stepState == QuestStepState.Ready;
			}
		}

		public override int AmountNeeded
		{
			get
			{
				uint quantityByDefinitionId = playerService.GetQuantityByDefinitionId(base.questStepDefinition.ItemDefinitionID);
				base.questStep.AmountCompleted = (int)quantityByDefinitionId;
				return base.questStepDefinition.ItemAmount - (int)quantityByDefinitionId;
			}
		}

		public DeliveryQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, IDefinitionService definitionService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
			this.definitionService = definitionService;
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			base.questStep.AmountCompleted = (int)playerService.GetQuantityByDefinitionId(base.questStepDefinition.ItemDefinitionID);
			if (base.questStep.state == QuestStepState.Notstarted)
			{
				GoToNextState();
			}
			else if (base.questStep.AmountCompleted < base.questStepDefinition.ItemAmount)
			{
				GoToTaskState(QuestStepState.Inprogress);
			}
			else if (base.questStep.state != QuestStepState.Ready)
			{
				GoToTaskState(QuestStepState.Ready);
			}
		}

		public override void SetupTracking()
		{
			base.questStep.TrackedID = definitionService.GetBuildingDefintionIDFromItemDefintionID(base.questStepDefinition.ItemDefinitionID);
			if (base.questStep.TrackedID == 0)
			{
				logger.Fatal(FatalCode.QS_NO_SUCH_TRACKED_STEP_ID, "Item definition id not found for Delivery Type quests");
			}
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("deliveryAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			ItemDefinition itemDefinition = defService.Get<ItemDefinition>(base.questStepDefinition.ItemDefinitionID);
			string @string = localService.GetString(itemDefinition.LocalizedKey + "*", base.questStepDefinition.ItemAmount);
			return localService.GetString("deliverTaskDesc", @string);
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			int buildingDefintionIDFromItemDefintionID = defService.GetBuildingDefintionIDFromItemDefintionID(base.questStepDefinition.ItemDefinitionID);
			BuildingDefinition buildingDefinition = definitionService.Get<BuildingDefinition>(buildingDefintionIDFromItemDefintionID);
			mainSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Image);
			maskSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Mask);
		}
	}
}
