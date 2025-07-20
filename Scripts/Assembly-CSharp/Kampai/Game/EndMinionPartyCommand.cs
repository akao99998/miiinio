using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class EndMinionPartyCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("EndMinionPartyCommand") as IKampaiLogger;

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public PostMinionPartyEndSignal postMinionPartyEndSignal { get; set; }

		[Inject]
		public RestoreTaskingMinionsFromPartySignal restoreTaskingMinionFromPartySignal { get; set; }

		[Inject]
		public RestoreLeisureMinionsFromPartySignal restoreLeisureMinionsFromPartySignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ShowHUDSignal showHudSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public CameraMoveToCustomPositionSignal customCameraPositionSignal { get; set; }

		[Inject]
		public IPartyFavorAnimationService partyFavorService { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public PlayGlobalMusicSignal musicSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IPartyService partyService { get; set; }

		[Inject]
		public StartPartyBuffTimerSignal startPartyBuff { get; set; }

		[Inject]
		public UnlockCharacterModel characterModel { get; set; }

		[Inject]
		public ShowDialogSignal showDialogSignal { get; set; }

		[Inject]
		public EndTownhallMinionPartyAnimationSignal endTownhallMinionPartyAnimationSignal { get; set; }

		[Inject]
		public RemoveCharacterFromPartyStageSignal removeStageCharacters { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayfindersSignal { get; set; }

		[Inject]
		public UpdateAdHUDSignal updateAdHUDSignal { get; set; }

		[Inject]
		public bool isSkipping { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			logger.Debug("Minion Party Sequence Ending");
			startPartyBuff.Dispatch();
			if (!characterModel.stuartFirstTimeHonor && characterModel.characterUnlocks.Count == 0 && characterModel.minionUnlocks.Count == 0)
			{
				foreach (QuestDialogSetting item in characterModel.dialogQueue)
				{
					showDialogSignal.Dispatch("AlertPrePrestige", item, new Tuple<int, int>(0, 0));
				}
				characterModel.dialogQueue.Clear();
			}
			if (partyService.IsInspiredParty)
			{
				uiContext.injectionBinder.GetInstance<ShouldRateAppSignal>().Dispatch(ConfigurationDefinition.RateAppAfterEvent.LevelUp);
			}
			if (isSkipping)
			{
				telemetryService.Send_Telemetry_EVT_PARTY_SKIPPED();
			}
			ShitThatNeedsToBeDoneOnPartyEnd();
		}

		private void ShitThatNeedsToBeDoneOnPartyEnd()
		{
			restoreTaskingMinionFromPartySignal.Dispatch();
			restoreLeisureMinionsFromPartySignal.Dispatch();
			partyFavorService.RemoveAllPartyFavorAnimations();
			showAllWayfindersSignal.Dispatch();
			endTownhallMinionPartyAnimationSignal.Dispatch();
			removeStageCharacters.Dispatch();
			showHudSignal.Dispatch(true);
			UIModel instance = uiContext.injectionBinder.GetInstance<UIModel>();
			if (!instance.LevelUpUIOpen)
			{
				uiContext.injectionBinder.GetInstance<ShowAllWayFindersSignal>().Dispatch();
			}
			showStoreSignal.Dispatch(true);
			questService.UpdateAllQuestsWithQuestStepType(QuestStepType.ThrowParty, QuestTaskTransition.Complete);
			customCameraPositionSignal.Dispatch(60006, new Boxed<Action>(OnFinalCameraPanComplete));
			postMinionPartyEndSignal.Dispatch();
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			dictionary.Add("endParty", 1f);
			Dictionary<string, float> type = dictionary;
			musicSignal.Dispatch("Play_partyMeterMusic_01", type);
			routineRunner.StartCoroutine(WaitAndResumeBGM());
		}

		private void OnFinalCameraPanComplete()
		{
			logger.Debug("Setting MinionParty.IsPartyHappening to false and releasing block.");
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			minionPartyInstance.IsPartyHappening = false;
			prestigeService.UpdateEligiblePrestigeList();
			updateAdHUDSignal.Dispatch();
		}

		private IEnumerator WaitAndResumeBGM()
		{
			yield return new WaitForSeconds(5.7f);
			Dictionary<string, float> parameters = new Dictionary<string, float>();
			musicSignal.Dispatch("Play_backGroundMusic_01", parameters);
		}
	}
}
