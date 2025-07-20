using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game.MasterPlanQuest
{
	public class CompleteOrdersQuestStepController : MasterPlanQuestStepController
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

		protected override string DescriptionLocKey
		{
			get
			{
				return "MasterPlanTaskCompleteOrders";
			}
		}

		public CompleteOrdersQuestStepController(Quest quest, int stepIndex, IDefinitionService definitionService, IPlayerService playerService, ICrossContextCapable gameContext, IKampaiLogger logger)
			: base(quest, stepIndex, definitionService, playerService, gameContext, logger)
		{
		}

		public override void GetStepDescIcon(IDefinitionService defService, out Sprite mainSprite, out Sprite maskSprite)
		{
			mainSprite = UIUtils.LoadSpriteFromPath("img_orderboard_item_fill");
			maskSprite = UIUtils.LoadSpriteFromPath("img_orderboard_item_mask", "btn_Main01_mask");
		}

		protected override object[] DescriptionArgs(ILocalizationService localizationService)
		{
			return new object[1] { taskQuestDef.ItemAmount };
		}
	}
}
