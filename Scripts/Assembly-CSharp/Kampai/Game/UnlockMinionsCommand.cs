using System;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game.View;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class UnlockMinionsCommand : Command
	{
		private bool isLevelUnlock = true;

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PhilBeginIntroLoopSignal beginIntroLoopSignal { get; set; }

		[Inject]
		public UnleashCharacterAtShoreSignal unleashCharacterAtShoreSignal { get; set; }

		[Inject]
		public PromptReceivedSignal promptReceivedSignal { get; set; }

		[Inject]
		public CameraZoomBeachSignal cameraZoomBeachSignal { get; set; }

		[Inject]
		public UnlockCharacterModel characterModel { get; set; }

		[Inject]
		public CreateNamedCharacterViewSignal createCharacterSignal { get; set; }

		[Inject]
		public CreateMinionSignal createMinionSignal { get; set; }

		[Inject]
		public CreateInnerTubeSignal createInnerTubeSignal { get; set; }

		[Inject]
		public EnablePartyFunMeterSignal enablePartyFunMeter { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public CameraAutoPanCompleteSignal cameraAutoPanCompleteSignal { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject namedCharacterManagerView { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		public override void Execute()
		{
			if (characterModel.characterUnlocks.Count == 0 && characterModel.minionUnlocks.Count == 0)
			{
				return;
			}
			List<Character> characterList;
			if (characterModel.characterUnlocks.Count > 0)
			{
				characterList = new List<Character>(characterModel.characterUnlocks);
				characterModel.characterUnlocks.Clear();
			}
			else
			{
				characterList = new List<Character>(characterModel.minionUnlocks);
				characterModel.minionUnlocks.Clear();
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			bool waitForCamera = false;
			string key = string.Empty;
			if (quantity == 0)
			{
				key = string.Format("AnnounceMinionSet{0}", 1);
			}
			SetupCharacters(characterList, ref key, ref waitForCamera);
			bool showDialog = !string.IsNullOrEmpty(key);
			bool flag = playerService.IsMinionPartyUnlocked();
			characterModel.stuartFirstTimeHonor = IsFirstStuartHonorGuestParty(characterList);
			if (!flag)
			{
				cameraZoomBeachSignal.Dispatch();
			}
			if (waitForCamera)
			{
				cameraAutoPanCompleteSignal.AddOnce(delegate
				{
					enablePartyFunMeter.Dispatch(false);
					BeginIntroLoop(showDialog, key, characterList);
					HandlePrompt(showDialog, characterList);
					StuartHonorDialog();
				});
			}
			else
			{
				enablePartyFunMeter.Dispatch(false);
				BeginIntroLoop(showDialog, key, characterList);
				HandlePrompt(showDialog, characterList);
				StuartHonorDialog();
			}
		}

		private bool IsFirstStuartHonorGuestParty(IList<Character> characterList)
		{
			StuartCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StuartCharacter>(70001);
			if (firstInstanceByDefinitionId == null)
			{
				return false;
			}
			if (firstInstanceByDefinitionId.WasHonorGuest)
			{
				return false;
			}
			foreach (Character character in characterList)
			{
				if (character is StuartCharacter)
				{
					return false;
				}
			}
			firstInstanceByDefinitionId.WasHonorGuest = true;
			return true;
		}

		private void StuartHonorDialog()
		{
			if (!characterModel.stuartFirstTimeHonor)
			{
				return;
			}
			Action<int, int> PrestigeDialog = delegate
			{
				characterModel.stuartFirstTimeHonor = false;
				if (characterModel.characterUnlocks.Count == 0 && characterModel.minionUnlocks.Count == 0)
				{
					foreach (QuestDialogSetting item in characterModel.dialogQueue)
					{
						base.injectionBinder.GetInstance<ShowDialogSignal>().Dispatch("AlertPrePrestige", item, new Tuple<int, int>(0, 0));
					}
					characterModel.dialogQueue.Clear();
				}
			};
			Action<bool> action = delegate
			{
				QuestDialogSetting type = new QuestDialogSetting
				{
					type = QuestDialogType.MINIONREWARD
				};
				Tuple<int, int> type2 = new Tuple<int, int>(-1, -1);
				base.injectionBinder.GetInstance<ShowDialogSignal>().Dispatch("GOHStuartFirstTime", type, type2);
				promptReceivedSignal.AddOnce(PrestigeDialog);
			};
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			if (minionPartyInstance.IsPartyHappening)
			{
				base.injectionBinder.GetInstance<EndMinionPartySignal>().AddOnce(action);
			}
			else
			{
				action(true);
			}
		}

		private void SetupCharacters(IList<Character> characterList, ref string key, ref bool waitForCamera)
		{
			foreach (Character character in characterList)
			{
				Minion minion = character as Minion;
				NamedCharacter namedCharacter = character as NamedCharacter;
				NamedCharacterObject namedCharacterObject = null;
				Prestige prestige = null;
				if (minion != null)
				{
					MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
					if (minionPartyInstance.IsPartyHappening)
					{
						minion.IsInMinionParty = true;
					}
					prestige = prestigeService.GetPrestigeFromMinionInstance(minion);
					createMinionSignal.Dispatch(minion);
					if (prestige != null)
					{
						MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
						MinionObject minionObject = component.Get(minion.ID);
						minionObject.EnableRenderers(false);
						waitForCamera = true;
						isLevelUnlock = false;
						key = string.Format("AnnounceMinion{0}", prestige.Definition.LocalizedKey);
					}
				}
				else if (namedCharacter != null)
				{
					createCharacterSignal.Dispatch(namedCharacter);
					isLevelUnlock = false;
					prestige = prestigeService.GetPrestigeFromMinionInstance(namedCharacter);
					key = string.Format("AnnounceCharacter{0}", character.Name);
					namedCharacterObject = namedCharacterManagerView.GetComponent<NamedCharacterManagerView>().Get(namedCharacter.ID);
				}
				if (prestige != null)
				{
					if (namedCharacterObject != null)
					{
						namedCharacterObject.EnableRenderers(false);
						waitForCamera = true;
					}
					uiContext.injectionBinder.GetInstance<ShowBuddyWelcomePanelUISignal>().Dispatch(new Boxed<Prestige>(prestige), CharacterWelcomeState.Welcome, 0);
				}
				base.injectionBinder.GetInstance<UnveilCharacterSignal>().Dispatch(character);
			}
		}

		private void HandlePrompt(bool showDialog, IList<Character> characterList)
		{
			Action<int, int> action = delegate
			{
				foreach (Character character in characterList)
				{
					unleashCharacterAtShoreSignal.Dispatch(character, characterModel.routeIndex);
				}
				if (!isLevelUnlock)
				{
					characterModel.routeIndex = -1;
				}
				base.injectionBinder.GetInstance<DeselectAllMinionsSignal>().Dispatch();
			};
			if (showDialog)
			{
				promptReceivedSignal.AddOnce(action);
			}
			else
			{
				action(0, 0);
			}
		}

		private void BeginIntroLoop(bool showDialog, string key, IList<Character> characterList)
		{
			QuestDialogSetting questDialogSetting = new QuestDialogSetting();
			questDialogSetting.type = QuestDialogType.MINIONREWARD;
			QuestDialogSetting type = questDialogSetting;
			Tuple<int, int> type2 = new Tuple<int, int>(-1, -1);
			if (showDialog)
			{
				base.injectionBinder.GetInstance<ShowDialogSignal>().Dispatch(key, type, type2);
			}
			bool type3 = false;
			int num = 0;
			int num2 = 3;
			foreach (Character character in characterList)
			{
				Minion minion = character as Minion;
				if (isLevelUnlock && num < num2)
				{
					createInnerTubeSignal.Dispatch(num);
				}
				if (character is NamedCharacter)
				{
					type3 = true;
					NamedCharacterObject namedCharacterObject = namedCharacterManagerView.GetComponent<NamedCharacterManagerView>().Get(character.ID);
					if (namedCharacterObject != null)
					{
						namedCharacterObject.EnableRenderers(true);
					}
				}
				else if (minion != null && minion.HasPrestige)
				{
					type3 = true;
					MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
					MinionObject minionObject = component.Get(character.ID);
					minionObject.EnableRenderers(true);
				}
				base.injectionBinder.GetInstance<BeginCharacterIntroLoopSignal>().Dispatch(character);
				num++;
			}
			if (showDialog)
			{
				beginIntroLoopSignal.Dispatch(type3);
			}
		}
	}
}
