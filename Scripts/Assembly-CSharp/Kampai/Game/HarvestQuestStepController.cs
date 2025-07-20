using System.Collections.Generic;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class HarvestQuestStepController : QuestStepController
	{
		private IDefinitionService definitionService;

		public override bool NeedActiveDeliverButton
		{
			get
			{
				if (StepState == QuestStepState.Inprogress)
				{
					return true;
				}
				return false;
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

		public HarvestQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, IDefinitionService definitionService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
			this.definitionService = definitionService;
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			QuestStepDefinition questStepDefinition = base.questStepDefinition;
			QuestStep questStep = base.questStep;
			if (itemDefId != 0 && questStepDefinition.ItemDefinitionID != itemDefId)
			{
				return;
			}
			questStep.AmountCompleted = (int)playerService.GetQuantityByDefinitionId(questStepDefinition.ItemDefinitionID);
			questStep.AmountReady = GetHarvestableCount(questStepDefinition.ItemDefinitionID);
			if (questTaskTransition == QuestTaskTransition.Start && questStep.state == QuestStepState.Notstarted)
			{
				GoToNextState();
				if (questStep.AmountCompleted >= questStepDefinition.ItemAmount)
				{
					GoToTaskState(QuestStepState.WaitComplete);
				}
			}
			else if (questTaskTransition == QuestTaskTransition.Harvestable && questStep.state == QuestStepState.Inprogress)
			{
				if (questStep.AmountReady + questStep.AmountCompleted >= questStepDefinition.ItemAmount)
				{
					GoToNextState();
				}
			}
			else if (questTaskTransition == QuestTaskTransition.Complete && (questStep.state == QuestStepState.Inprogress || questStep.state == QuestStepState.Ready))
			{
				if (questStep.AmountCompleted >= questStepDefinition.ItemAmount)
				{
					GoToNextState(true);
				}
				else if (questStep.AmountReady + questStep.AmountCompleted >= questStepDefinition.ItemAmount && questStep.state == QuestStepState.Inprogress)
				{
					GoToNextState();
				}
			}
			else if (questTaskTransition == QuestTaskTransition.Start && questStep.AmountCompleted >= questStepDefinition.ItemAmount && questStep.state != QuestStepState.WaitComplete)
			{
				GoToNextState(true);
			}
		}

		private int GetHarvestableCount(int itemDefinitionId)
		{
			int num = 0;
			List<ResourceBuilding> instancesByType = playerService.GetInstancesByType<ResourceBuilding>();
			List<CraftingBuilding> instancesByType2 = playerService.GetInstancesByType<CraftingBuilding>();
			int i = 0;
			for (int count = instancesByType.Count; i < count; i++)
			{
				ResourceBuilding resourceBuilding = instancesByType[i];
				if (resourceBuilding.Definition.ItemId == itemDefinitionId)
				{
					num += resourceBuilding.AvailableHarvest;
				}
			}
			int j = 0;
			for (int count2 = instancesByType2.Count; j < count2; j++)
			{
				CraftingBuilding craftingBuilding = instancesByType2[j];
				IList<int> completedCrafts = craftingBuilding.CompletedCrafts;
				int k = 0;
				for (int count3 = completedCrafts.Count; k < count3; k++)
				{
					if (completedCrafts[k] == itemDefinitionId)
					{
						num++;
					}
				}
			}
			return num;
		}

		public override void SetupTracking()
		{
			base.questStep.TrackedID = definitionService.GetBuildingDefintionIDFromItemDefintionID(base.questStepDefinition.ItemDefinitionID);
			if (base.questStep.TrackedID == 0)
			{
				logger.Fatal(FatalCode.QS_NO_SUCH_TRACKED_HARVEST_ID, "Item definition id not found for Delivery Type quests");
			}
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("haveAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			ItemDefinition itemDefinition = defService.Get<ItemDefinition>(base.questStepDefinition.ItemDefinitionID);
			string @string = localService.GetString(itemDefinition.LocalizedKey + "*", base.questStepDefinition.ItemAmount);
			return localService.GetString("harvestTaskDesc", @string);
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
