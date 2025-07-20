using System;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DisplayMasterPlanIntroDialogCommand : Command
	{
		private bool firstTimeInLair;

		[Inject]
		public VillainLairModel model { get; set; }

		[Inject]
		public SetVillainLairAnimationTriggerSignal setAnimTriggerSignal { get; set; }

		[Inject]
		public PromptReceivedSignal promptReceivedSignal { get; set; }

		[Inject]
		public DisplayVolcanoLairVillainWayfinderSignal displayVolcanoWayfinderSignal { get; set; }

		[Inject]
		public HideFluxWayfinder hideFluxWayfinderSignal { get; set; }

		[Inject]
		public EnableVillainLairHudSignal enableVillainHudSignal { get; set; }

		[Inject]
		public CameraMoveToCustomPositionSignal cameraMoveToCustomPositionSignal { get; set; }

		[Inject]
		public EnableAllVillainLairCollidersSignal enableAllLairCollidersSignal { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public DetermineLairUISignal determineLairUISignal { get; set; }

		public override void Execute()
		{
			firstTimeInLair = !model.currentActiveLair.hasVisited;
			if (firstTimeInLair)
			{
				model.currentActiveLair.hasVisited = true;
				enableVillainHudSignal.Dispatch(true);
				cameraMoveToCustomPositionSignal.Dispatch(60017, new Boxed<Action>(DisplayDialog));
			}
			else
			{
				DisplayDialog();
			}
			setAnimTriggerSignal.Dispatch("OnLoop");
		}

		private void DisplayDialog()
		{
			QuestDialogSetting questDialogSetting = new QuestDialogSetting();
			questDialogSetting.type = QuestDialogType.NORMAL;
			QuestDialogSetting questDialogSetting2 = questDialogSetting;
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			questDialogSetting2.additionalStringParameter = currentMasterPlan.Definition.LocalizedKey;
			Tuple<int, int> type = new Tuple<int, int>(-1, -1);
			base.injectionBinder.GetInstance<ShowDialogSignal>().Dispatch(currentMasterPlan.Definition.IntroDialog, questDialogSetting2, type);
			masterPlanService.CurrentMasterPlan.introHasBeenDisplayed = true;
			promptReceivedSignal.AddOnce(HandlePrompt);
		}

		private void HandlePrompt(int questId, int stepId)
		{
			setAnimTriggerSignal.Dispatch("OnStop");
			displayVolcanoWayfinderSignal.Dispatch();
			hideFluxWayfinderSignal.Dispatch(false);
			enableAllLairCollidersSignal.Dispatch(true);
			if (!firstTimeInLair)
			{
				determineLairUISignal.Dispatch();
			}
		}
	}
}
