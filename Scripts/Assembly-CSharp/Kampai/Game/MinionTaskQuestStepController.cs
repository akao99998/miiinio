using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class MinionTaskQuestStepController : QuestStepController
	{
		public int RequiredMinionLevel
		{
			get
			{
				return base.questStepDefinition.UpgradeLevel;
			}
		}

		public MinionTaskQuestStepController(Quest quest, int stepIndex, IQuestScriptService questScriptService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, questScriptService, playerService, gameContext, logger)
		{
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
			QuestStep questStep = base.questStep;
			if ((questStep.TrackedID != 0 && questStep.TrackedID != buildingDefId) || itemDefId < RequiredMinionLevel)
			{
				return;
			}
			if (base.questStep.state == QuestStepState.Notstarted)
			{
				GoToNextState();
			}
			if (questTaskTransition == QuestTaskTransition.Complete && base.questStep.state != QuestStepState.RunningStartScript)
			{
				questStep.AmountCompleted++;
				if (questStep.AmountCompleted >= base.questStepDefinition.ItemAmount)
				{
					GoToNextState(true);
				}
			}
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("PlayerTrainingTask");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			return GetDescription(localService, RequiredMinionLevel, base.questStepDefinition);
		}

		public static string GetDescription(ILocalizationService localService, int level, QuestStepDefinition questStepDefinition)
		{
			int itemAmount = questStepDefinition.ItemAmount;
			if (itemAmount > 1)
			{
				return localService.GetString("minionUpgradeTaskWithLevelMultiple", questStepDefinition.ItemAmount, level + 1);
			}
			return localService.GetString("minionUpgradeTaskWithLevel", level + 1);
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			BuildingDefinition buildingDefinition = defService.Get<BuildingDefinition>((base.questStep.TrackedID != 0) ? base.questStep.TrackedID : 3002);
			mainSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Image);
			maskSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Mask);
		}

		public override void SetupTracking()
		{
			base.questStep.TrackedID = base.questStepDefinition.ItemDefinitionID;
		}
	}
}
