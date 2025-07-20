using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game.MasterPlanQuest
{
	public abstract class ItemTaskQuestStepController : MasterPlanQuestStepController
	{
		public override int AmountNeeded
		{
			get
			{
				return (int)task.remainingQuantity;
			}
		}

		public override bool NeedActiveDeliverButton
		{
			get
			{
				QuestStepState stepState = StepState;
				return stepState == QuestStepState.Inprogress || stepState == QuestStepState.Ready;
			}
		}

		protected ItemTaskQuestStepController(Quest quest, int stepIndex, IDefinitionService definitionService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, definitionService, playerService, gameContext, logger)
		{
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			int buildingDefintionIDFromItemDefintionID = defService.GetBuildingDefintionIDFromItemDefintionID(taskDefinition.requiredItemId);
			BuildingDefinition buildingDefinition = definitionService.Get<BuildingDefinition>(buildingDefintionIDFromItemDefintionID);
			if (buildingDefinition == null)
			{
				mainSprite = UIUtils.LoadSpriteFromPath(string.Empty);
				maskSprite = UIUtils.LoadSpriteFromPath(string.Empty, "btn_Main01_mask");
			}
			else
			{
				mainSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Image);
				maskSprite = UIUtils.LoadSpriteFromPath(buildingDefinition.Mask, "btn_Main01_mask");
			}
		}

		protected override object[] DescriptionArgs(ILocalizationService localizationService)
		{
			return new object[1] { ItemName(localizationService) };
		}

		protected virtual string ItemName(ILocalizationService localizationService)
		{
			ItemDefinition definition;
			definitionService.TryGet<ItemDefinition>(taskQuestDef.ItemDefinitionID, out definition);
			return localizationService.GetString(definition.LocalizedKey + "*", task.Definition.requiredQuantity);
		}
	}
}
