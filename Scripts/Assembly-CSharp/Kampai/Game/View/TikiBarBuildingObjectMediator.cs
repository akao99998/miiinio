using System;
using System.Collections;
using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class TikiBarBuildingObjectMediator : EventMediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("TikiBarBuildingObjectMediator") as IKampaiLogger;

		private Signal<CharacterObject, int> addToTikiBarSignal;

		[Inject]
		public TikiBarBuildingObjectView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public RestoreMinionAtTikiBarSignal restoreMinionSignal { get; set; }

		[Inject]
		public MinionStateChangeSignal minionChangeStateSignal { get; set; }

		[Inject]
		public ShowQuestPanelSignal showPanelSignal { get; set; }

		[Inject]
		public ShowQuestRewardSignal showQuestRewardSignal { get; set; }

		[Inject]
		public PathCharacterToTikiBarSignal pathCharacterToTikibarSignal { get; set; }

		[Inject]
		public TeleportCharacterToTikiBarSignal teleportCharacterToTikibarSignal { get; set; }

		[Inject]
		public UnveilCharacterObjectSignal unveilCharacterSignal { get; set; }

		[Inject]
		public BeginCharacterLoopAnimationSignal characterLoopAnimationSignal { get; set; }

		[Inject]
		public PopUnleashedCharacterToTikiBarSignal popUnleashedCharacterToTikibarSignal { get; set; }

		[Inject]
		public ReleaseMinionFromTikiBarSignal releaseMinionFromTikiBarSignal { get; set; }

		[Inject]
		public NamedCharacterRemovedFromTikiBarSignal removedFromTikibarSignal { get; set; }

		[Inject]
		public EndCharacterIntroSignal endCharacterIntroSignal { get; set; }

		[Inject]
		public CharacterIntroCompleteSignal introCompleteSignal { get; set; }

		[Inject]
		public CharacterDrinkingCompleteSignal drinkingCompleteSignal { get; set; }

		[Inject]
		public ToggleStickerbookGlowSignal glowSignal { get; set; }

		[Inject]
		public GetNewQuestSignal getNewQuestSignal { get; set; }

		[Inject]
		public ToggleHitboxSignal toggleHitboxSignal { get; set; }

		[Inject]
		public TikiBarSetAnimParamSignal setAnimParamSignal { get; set; }

		[Inject]
		public TikiBarResetAnimParamSignal resetAnimParamSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public UnlockMinionsSignal unlockMinionsSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void OnRegister()
		{
			addToTikiBarSignal = new Signal<CharacterObject, int>();
			unveilCharacterSignal.AddListener(UnveilCharacter);
			characterLoopAnimationSignal.AddListener(BeginCharacterIntroLoop);
			popUnleashedCharacterToTikibarSignal.AddListener(UnleashCharacterToTikiBar);
			pathCharacterToTikibarSignal.AddListener(PathCharacterToTikiBar);
			teleportCharacterToTikibarSignal.AddListener(TeleportCharacterToTikiBar);
			endCharacterIntroSignal.AddListener(EndCharacterIntro);
			drinkingCompleteSignal.AddListener(CharacterDrinkingComplete);
			releaseMinionFromTikiBarSignal.AddListener(ReleaseMinionFromTikiBar);
			restoreMinionSignal.AddListener(RestoreMinion);
			StartCoroutine(Init());
			toggleHitboxSignal.AddListener(ToggleHitbox);
			setAnimParamSignal.AddListener(PlayAnimation);
			glowSignal.AddListener(ToggleStickerbookGlow);
			addToTikiBarSignal.AddListener(AddCharacterToTikiBar);
			resetAnimParamSignal.AddListener(ResetAnimationParameters);
			view.SetupInjections(minionChangeStateSignal, removedFromTikibarSignal, introCompleteSignal, drinkingCompleteSignal);
		}

		public override void OnRemove()
		{
			unveilCharacterSignal.RemoveListener(UnveilCharacter);
			characterLoopAnimationSignal.RemoveListener(BeginCharacterIntroLoop);
			popUnleashedCharacterToTikibarSignal.RemoveListener(UnleashCharacterToTikiBar);
			pathCharacterToTikibarSignal.RemoveListener(PathCharacterToTikiBar);
			endCharacterIntroSignal.RemoveListener(EndCharacterIntro);
			drinkingCompleteSignal.RemoveListener(CharacterDrinkingComplete);
			teleportCharacterToTikibarSignal.RemoveListener(TeleportCharacterToTikiBar);
			releaseMinionFromTikiBarSignal.RemoveListener(ReleaseMinionFromTikiBar);
			restoreMinionSignal.RemoveListener(RestoreMinion);
			toggleHitboxSignal.RemoveListener(ToggleHitbox);
			setAnimParamSignal.RemoveListener(PlayAnimation);
			glowSignal.RemoveListener(ToggleStickerbookGlow);
			addToTikiBarSignal.RemoveListener(AddCharacterToTikiBar);
			resetAnimParamSignal.RemoveListener(ResetAnimationParameters);
		}

		private void ResetAnimationParameters(bool didSkip)
		{
			view.didSkipParty = didSkip;
			view.ResetAnimationParameters();
		}

		private void AddCharacterToTikiBar(CharacterObject characterObject, int routeIndex)
		{
			view.AddCharacterToBuildingActions(characterObject, playerService, routeIndex, prestigeService, getNewQuestSignal);
			if (routeIndex > 0)
			{
				unlockMinionsSignal.Dispatch();
			}
		}

		private void ToggleHitbox(BuildingZoomType zoomBuildingType, bool enable)
		{
			if (zoomBuildingType == BuildingZoomType.TIKIBAR)
			{
				view.ToggleHitbox(enable);
			}
		}

		private void ReleaseMinionFromTikiBar(Character character, bool forceRelease)
		{
			if (character is KevinCharacter || forceRelease)
			{
				CharacterDrinkingComplete(character.ID);
			}
			else
			{
				view.RemoveCharacterFromTikiBar(character.ID);
			}
		}

		private void UnveilCharacter(CharacterObject characterObject)
		{
			view.SetupCharacter(characterObject, playerService, prestigeService);
		}

		private void BeginCharacterIntroLoop(CharacterObject characterObject)
		{
			MinionObject minionObject = characterObject as MinionObject;
			if (!(minionObject != null) || minionObject.GetMinion() == null || minionObject.GetMinion().HasPrestige)
			{
				hideAllWayFindersSignal.Dispatch();
			}
			bool waitForLoop = playerService.GetQuantity(StaticItem.LEVEL_ID) == 0 || minionObject == null || (minionObject != null && minionObject.GetMinion().HasPrestige);
			view.BeginCharacterIntroLoop(waitForLoop, characterObject);
		}

		private void UnleashCharacterToTikiBar(CharacterObject characterObject, int routeIndex)
		{
			MinionObject minionObject = characterObject as MinionObject;
			bool waitForLoop = playerService.GetQuantity(StaticItem.LEVEL_ID) == 0 || minionObject == null || (minionObject != null && minionObject.GetMinion().HasPrestige);
			view.BeginCharacterIntro(waitForLoop, characterObject, routeIndex);
			if (playerService.GetMinionCount() <= 4 && minionObject != null)
			{
				StartCoroutine(PanCameraToBeach());
			}
		}

		private IEnumerator PanCameraToBeach()
		{
			yield return new WaitForSeconds(9.7f);
			gameContext.injectionBinder.GetInstance<CameraAutoMoveToPositionSignal>().Dispatch(new Vector3(140.2076f, 13.75177f, 158.1269f), 0.9557783f, false);
		}

		private void EndCharacterIntro(CharacterObject characterObject, int routeIndex)
		{
			view.EndCharacterIntro(characterObject, routeIndex);
		}

		private void CharacterDrinkingComplete(int instanceID)
		{
			view.UntrackChild(instanceID);
		}

		private void PathCharacterToTikiBar(CharacterObject characterObject, RouteInstructions ri, int routeIndex)
		{
			view.PathCharacterToTikiBar(characterObject, ri.Path, ri.Rotation, routeIndex, addToTikiBarSignal);
		}

		private void TeleportCharacterToTikiBar(CharacterObject characterObject, int routeIndex)
		{
			if (!view.ContainsCharacter(characterObject.ID) && routeIndex <= 2)
			{
				if (characterObject.ID == 78)
				{
					Prestige prestige = prestigeService.GetPrestige(40000);
					prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Questing);
				}
				AddCharacterToTikiBar(characterObject, routeIndex);
			}
		}

		public void RestoreMinion(Character character)
		{
			Prestige prestigeFromMinionInstance = prestigeService.GetPrestigeFromMinionInstance(character);
			if (prestigeFromMinionInstance == null)
			{
				logger.Error("RestoreMinion: Prestige == null for minion ID: {0}", character.ID);
				return;
			}
			int iD = prestigeFromMinionInstance.Definition.ID;
			int minionSlotIndex = view.tikiBar.GetMinionSlotIndex(iD);
			if (minionSlotIndex != -1)
			{
				prestigeService.AddMinionToTikiBarSlot(character, minionSlotIndex, view.tikiBar);
			}
		}

		private IEnumerator Init()
		{
			yield return null;
			if (view.tikiBar.minionQueue.Count == 0)
			{
				view.tikiBar.minionQueue.Add(40000);
				view.tikiBar.minionQueue.Add(-1);
				view.tikiBar.minionQueue.Add(-1);
			}
		}

		public void MinionButtonClicked(int questInstanceID)
		{
			Quest byInstanceId = playerService.GetByInstanceId<Quest>(questInstanceID);
			switch (byInstanceId.state)
			{
			case QuestState.Notstarted:
			case QuestState.RunningStartScript:
			case QuestState.RunningTasks:
			case QuestState.RunningCompleteScript:
				showPanelSignal.Dispatch(questInstanceID);
				break;
			case QuestState.Harvestable:
				showQuestRewardSignal.Dispatch(questInstanceID);
				break;
			}
		}

		private void PlayAnimation(string animation, Type type, object obj)
		{
			view.PlayAnimation(animation, type, obj);
		}

		private void ToggleStickerbookGlow(bool enable)
		{
			view.ToggleStickerbookGlow(enable);
		}
	}
}
