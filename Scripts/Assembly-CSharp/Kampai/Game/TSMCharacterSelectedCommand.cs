using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Trigger;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class TSMCharacterSelectedCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("TSMCharacterSelectedCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public PromptReceivedSignal promptReceivedSignal { get; set; }

		[Inject]
		public ShowDialogSignal showDialog { get; set; }

		[Inject]
		public ITriggerService triggerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public DisplayTreasureTeaserSignal displayTreasureTeaserSignal { get; set; }

		[Inject]
		public ShowProceduralQuestPanelSignal showProceduralQuestSignal { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		public override void Execute()
		{
			MinionParty minionPartyInstance = PlayerService.GetMinionPartyInstance();
			if (!minionPartyInstance.IsPartyHappening && !minionPartyInstance.IsPartyReady && !SetupTriggerDialog())
			{
				SetupDynamicQuest();
			}
		}

		private bool SetupTriggerDialog()
		{
			TriggerInstance activeTrigger = triggerService.ActiveTrigger;
			if (activeTrigger == null)
			{
				return false;
			}
			ProcessTrigger(activeTrigger);
			return true;
		}

		private void SetupDynamicQuest()
		{
			TSMCharacter firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<TSMCharacter>(70008);
			if (firstInstanceByDefinitionId != null && !firstInstanceByDefinitionId.HasShownIntroNarrative)
			{
				QuestDialogSetting type = new QuestDialogSetting();
				Tuple<int, int> type2 = new Tuple<int, int>(0, 0);
				promptReceivedSignal.AddOnce(DialogDismissed);
				showDialog.Dispatch("TSMNarrative", type, type2);
				firstInstanceByDefinitionId.HasShownIntroNarrative = true;
			}
			else
			{
				DialogDismissed(0, 0);
			}
		}

		private void DialogDismissed(int param1, int param2)
		{
			if (param1 == 0 && param2 == 0)
			{
				Quest firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<Quest>(77777);
				if (firstInstanceByDefinitionId == null)
				{
					logger.Warning("Ignoring tsm selection since there is no quest available, he's probably walking away.");
				}
				else if (firstInstanceByDefinitionId.GetActiveDefinition().SurfaceType == QuestSurfaceType.ProcedurallyGenerated)
				{
					showProceduralQuestSignal.Dispatch(firstInstanceByDefinitionId.ID);
				}
			}
		}

		private void ProcessTrigger(TriggerInstance instance)
		{
			IList<TriggerRewardDefinition> rewards = instance.Definition.rewards;
			if (rewards != null && rewards.Count > 0 && rewards[0].type == TriggerRewardType.Identifier.CaptainTease)
			{
				if (!uiModel.CaptainTeaserModalOpen)
				{
					uiModel.CaptainTeaserModalOpen = true;
					displayTreasureTeaserSignal.Dispatch(instance);
				}
			}
			else
			{
				OpenCaptainTriggerModal("popup_TSM_Gift_Upsell", instance);
			}
		}

		private void OpenCaptainTriggerModal(string prefab, TriggerInstance instance)
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, prefab);
			iGUICommand.skrimScreen = "ProceduralTaskSkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.Args.Add(typeof(TriggerInstance), instance);
			guiService.Execute(iGUICommand);
		}
	}
}
