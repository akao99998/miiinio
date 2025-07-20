using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game.MasterPlanQuest
{
	public class BuildComponentQuestStepController : QuestStepController
	{
		protected MasterPlanQuestType.Component questComponent;

		public override bool NeedActiveDeliverButton
		{
			get
			{
				return questComponent.component != null && questComponent.component.State == MasterPlanComponentState.TasksCollected && gameContext.injectionBinder.GetInstance<VillainLairModel>().currentActiveLair != null;
			}
		}

		public override bool NeedActiveProgressBar
		{
			get
			{
				return false;
			}
		}

		public override bool NeedGoToButton
		{
			get
			{
				return true;
			}
		}

		public BuildComponentQuestStepController(Quest quest, int stepIndex, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, null, playerService, gameContext, logger)
		{
			questComponent = quest as MasterPlanQuestType.Component;
		}

		public override string GetStepAction(ILocalizationService localService)
		{
			return localService.GetString("buildAction");
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition = defService.Get<MasterPlanComponentBuildingDefinition>(questComponent.buildDefId);
			mainSprite = UIUtils.LoadSpriteFromPath(masterPlanComponentBuildingDefinition.Image, "btn_Main01_fill");
			maskSprite = UIUtils.LoadSpriteFromPath(masterPlanComponentBuildingDefinition.Mask, "btn_Main01_mask");
		}

		public override string GetStepDescription(ILocalizationService localService, IDefinitionService defService)
		{
			MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition = defService.Get<MasterPlanComponentBuildingDefinition>(questComponent.buildDefId);
			return localService.GetString(masterPlanComponentBuildingDefinition.LocalizedKey);
		}

		public override void UpdateTask(QuestTaskTransition questTaskTransition, Building building, int buildingDefId, int itemDefId)
		{
		}
	}
}
