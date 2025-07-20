using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game.MasterPlanQuest
{
	public abstract class BuildingTaskQuestStepController : ItemTaskQuestStepController
	{
		public override bool NeedActiveDeliverButton
		{
			get
			{
				return StepState == QuestStepState.Ready || StepState == QuestStepState.WaitComplete;
			}
		}

		public override bool NeedGoToButton
		{
			get
			{
				return !NeedActiveDeliverButton;
			}
		}

		protected abstract string BuildingLocName { get; }

		protected BuildingTaskQuestStepController(Quest quest, int stepIndex, IDefinitionService definitionService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, definitionService, playerService, gameContext, logger)
		{
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			BuildingDefinition definition;
			bool flag = definitionService.TryGet<BuildingDefinition>(taskDefinition.requiredItemId, out definition);
			bool flag2 = taskDefinition.Type == MasterPlanComponentTaskType.EarnSandDollars;
			DisplayableDefinition displayableDefinition = ((!flag) ? definitionService.Get<DisplayableDefinition>((!flag2) ? 2 : 0) : definition);
			if (displayableDefinition == null)
			{
				mainSprite = UIUtils.LoadSpriteFromPath(string.Empty);
				maskSprite = UIUtils.LoadSpriteFromPath(string.Empty, "btn_Main01_mask");
			}
			else
			{
				mainSprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Image);
				maskSprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Mask, "btn_Main01_mask");
			}
		}

		protected override object[] DescriptionArgs(ILocalizationService localizationService)
		{
			return new object[2]
			{
				taskQuestDef.ItemAmount,
				ItemName(localizationService)
			};
		}

		protected override string ItemName(ILocalizationService localizationService)
		{
			BuildingDefinition definition;
			bool flag = definitionService.TryGet<BuildingDefinition>(taskQuestDef.ItemDefinitionID, out definition);
			return localizationService.GetString((!flag) ? BuildingLocName : definition.LocalizedKey);
		}
	}
}
