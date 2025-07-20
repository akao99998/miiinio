using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class BuildingRepairQuestStepController : QuestStepController
	{
		public override bool NeedActiveProgressBar
		{
			get
			{
				if (StepState == QuestStepState.Notstarted)
				{
					return false;
				}
				return true;
			}
		}

		public BuildingRepairQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			if ((StepType != QuestStepType.CabanaRepair || base.questStep.TrackedID == building.ID) && questTaskTransition == QuestTaskTransition.Complete)
			{
				GoToTaskState(QuestStepState.Complete);
			}
		}

		public override void SetupTracking()
		{
			int itemDefinitionID = base.questStepDefinition.ItemDefinitionID;
			int trackedID = 0;
			switch (StepType)
			{
			case QuestStepType.CabanaRepair:
			{
				CabanaBuilding firstInstanceByDefinitionId5 = playerService.GetFirstInstanceByDefinitionId<CabanaBuilding>(itemDefinitionID);
				if (firstInstanceByDefinitionId5 == null)
				{
					logger.Fatal(FatalCode.PS_MISSING_CABANA, "Cabana instance not found");
					return;
				}
				trackedID = firstInstanceByDefinitionId5.ID;
				break;
			}
			case QuestStepType.FountainRepair:
			{
				FountainBuilding firstInstanceByDefinitionId3 = playerService.GetFirstInstanceByDefinitionId<FountainBuilding>(itemDefinitionID);
				if (firstInstanceByDefinitionId3 == null)
				{
					logger.Fatal(FatalCode.PS_MISSING_FOUNTAIN, "Fountain instance not found");
					return;
				}
				trackedID = firstInstanceByDefinitionId3.ID;
				break;
			}
			case QuestStepType.StageRepair:
			{
				StageBuilding firstInstanceByDefinitionId6 = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(itemDefinitionID);
				if (firstInstanceByDefinitionId6 == null)
				{
					logger.Fatal(FatalCode.PS_MISSING_STAGE, "Stage instance not found");
					return;
				}
				trackedID = firstInstanceByDefinitionId6.ID;
				break;
			}
			case QuestStepType.StorageRepair:
			{
				StorageBuilding firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<StorageBuilding>(itemDefinitionID);
				if (firstInstanceByDefinitionId2 == null)
				{
					logger.Fatal(FatalCode.PS_MISSING_FOUNTAIN, "Storage instance not found");
					return;
				}
				trackedID = firstInstanceByDefinitionId2.ID;
				break;
			}
			case QuestStepType.WelcomeHutRepair:
			{
				WelcomeHutBuilding firstInstanceByDefinitionId4 = playerService.GetFirstInstanceByDefinitionId<WelcomeHutBuilding>(itemDefinitionID);
				if (firstInstanceByDefinitionId4 == null)
				{
					logger.Fatal(FatalCode.PS_MISSING_WELCOME_HUT, "Welcome hut instance not found");
					return;
				}
				trackedID = firstInstanceByDefinitionId4.ID;
				break;
			}
			case QuestStepType.LairPortalRepair:
			{
				VillainLairEntranceBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<VillainLairEntranceBuilding>(itemDefinitionID);
				if (firstInstanceByDefinitionId == null)
				{
					logger.Fatal(FatalCode.PS_NEGATIVE_VALUE, "Villain Lair Portal instance not found");
					return;
				}
				trackedID = firstInstanceByDefinitionId.ID;
				break;
			}
			case QuestStepType.MinionUpgradeBuildingRepair:
				trackedID = 375;
				break;
			}
			base.questStep.TrackedID = trackedID;
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			if (StepType == QuestStepType.CabanaRepair || StepType == QuestStepType.StageRepair)
			{
				return localService.GetString("buildAction");
			}
			return localService.GetString("repairAction");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			string result = null;
			switch (StepType)
			{
			case QuestStepType.CabanaRepair:
				result = localService.GetString(string.Format("{0}{1}", "CabanaRepair", base.questStepDefinition.ItemAmount));
				break;
			case QuestStepType.FountainRepair:
				result = localService.GetString("repairFountainDesc");
				break;
			case QuestStepType.StageRepair:
				result = localService.GetString("repairStageDesc");
				break;
			case QuestStepType.StorageRepair:
				result = localService.GetString("repairStorageDesc");
				break;
			case QuestStepType.WelcomeHutRepair:
				result = localService.GetString("repairWelcomeDesc");
				break;
			case QuestStepType.LairPortalRepair:
				result = localService.GetString("repairLairPortalDesc");
				break;
			case QuestStepType.MinionUpgradeBuildingRepair:
				result = localService.GetString("repairMinionUpgradeBuildingDesc");
				break;
			}
			return result;
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			BuildingDefinition buildingDefinition = defService.Get<BuildingDefinition>(base.questStepDefinition.ItemDefinitionID);
			mainSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Image);
			maskSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Mask);
		}
	}
}
