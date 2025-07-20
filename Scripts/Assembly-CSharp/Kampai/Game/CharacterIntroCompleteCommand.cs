using System.Collections;
using Kampai.Game.View;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CharacterIntroCompleteCommand : Command
	{
		[Inject]
		public CharacterObject characterObject { get; set; }

		[Inject]
		public int routeIndex { get; set; }

		[Inject]
		public EnablePartyFunMeterSignal enablePartyFunMeterSignal { get; set; }

		[Inject]
		public EndCharacterIntroSignal endCharacterIntroSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public ConfirmStartNewMinionPartySignal confirmStartNewMinionPartySignal { get; set; }

		[Inject]
		public DisplayLevelUpRewardSignal displayLevelUpRewardSignal { get; set; }

		[Inject]
		public ShowDialogSignal showDialogSignal { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChange { get; set; }

		[Inject]
		public CameraAutoMoveToPositionSignal cameraAutoMoveToPositionSignal { get; set; }

		[Inject]
		public SetKevinAnimatorCullingModeSignal setKevinAnimatorCullingModeSignal { get; set; }

		[Inject]
		public FrolicSignal frolicSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public UnlockCharacterModel characterModel { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		public override void Execute()
		{
			enablePartyFunMeterSignal.Dispatch(true);
			endCharacterIntroSignal.Dispatch(characterObject, routeIndex);
			if (!playerService.GetMinionPartyInstance().IsPartyHappening)
			{
				showAllWayFindersSignal.Dispatch();
			}
			Character byInstanceId = playerService.GetByInstanceId<Character>(characterObject.ID);
			Definition definition = byInstanceId.Definition;
			if (definition.ID == 70003)
			{
				HandleSpecialCharacter(byInstanceId, definition);
			}
			else if (routeIndex >= 0)
			{
				TikiBarBuilding byInstanceId2 = playerService.GetByInstanceId<TikiBarBuilding>(313);
				prestigeService.AddMinionToTikiBarSlot(byInstanceId, routeIndex, byInstanceId2);
				if (characterModel.minionUnlocks.Count > 0)
				{
					displayLevelUpRewardSignal.Dispatch(true);
				}
			}
			UpdateMinionParty();
		}

		private void HandleSpecialCharacter(Character character, Definition charDef)
		{
			Prestige prestigeFromMinionInstance = prestigeService.GetPrestigeFromMinionInstance(character);
			bool flag = false;
			if (prestigeFromMinionInstance != null)
			{
				Minion minion = character as Minion;
				PrestigeState targetState = PrestigeState.Questing;
				MinionState param = MinionState.Questing;
				prestigeService.ChangeToPrestigeState(prestigeFromMinionInstance, targetState);
				if (minion != null)
				{
					minion.PrestigeId = prestigeFromMinionInstance.ID;
					minionStateChange.Dispatch(minion.ID, param);
				}
			}
			KevinCharacterDefinition kevinCharacterDefinition = charDef as KevinCharacterDefinition;
			if (kevinCharacterDefinition != null)
			{
				Transform transform = characterObject.gameObject.transform;
				transform.position = (Vector3)kevinCharacterDefinition.Location;
				transform.rotation = Quaternion.Euler(kevinCharacterDefinition.RotationEulers);
				cameraAutoMoveToPositionSignal.Dispatch(transform.position, 0.9f, true);
				routineRunner.StartCoroutine(ToggleAnimatorMode());
				flag = true;
			}
			if (flag)
			{
				frolicSignal.Dispatch(character.ID);
			}
		}

		private void UpdateMinionParty()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			if (minionPartyInstance.CharacterUnlocking && minionPartyInstance.IsPartyReady)
			{
				confirmStartNewMinionPartySignal.Dispatch(true);
			}
			minionPartyInstance.CharacterUnlocking = false;
			if (minionPartyInstance.IsPartyHappening || characterModel.stuartFirstTimeHonor || characterModel.characterUnlocks.Count != 0 || characterModel.minionUnlocks.Count != 0)
			{
				return;
			}
			foreach (QuestDialogSetting item in characterModel.dialogQueue)
			{
				showDialogSignal.Dispatch("AlertPrePrestige", item, new Tuple<int, int>(0, 0));
			}
			characterModel.dialogQueue.Clear();
		}

		private IEnumerator ToggleAnimatorMode()
		{
			setKevinAnimatorCullingModeSignal.Dispatch(AnimatorCullingMode.AlwaysAnimate);
			yield return new WaitForSeconds(1f);
			setKevinAnimatorCullingModeSignal.Dispatch(AnimatorCullingMode.CullUpdateTransforms);
		}
	}
}
