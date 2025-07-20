using Kampai.Game;
using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class StickerbookCharacterMediator : Mediator
	{
		[Inject]
		public StickerbookCharacterView view { get; set; }

		[Inject]
		public CharacterClickedSignal characterSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public RequestCharacterSelectionSignal requestSignal { get; set; }

		[Inject]
		public MoveAudioListenerSignal toggleCharacterAudioSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(GetNumNewStickers(), fancyUIService, prestigeService, localService, definitionService);
			view.character.ClickedSignal.AddListener(CharacterClicked);
			view.lockedCharacter.ClickedSignal.AddListener(LockedClicked);
			view.limitedEvent.ClickedSignal.AddListener(LimitedClicked);
			characterSignal.AddListener(OnCharacterClicked);
			requestSignal.Dispatch();
		}

		public override void OnRemove()
		{
			view.character.ClickedSignal.RemoveListener(CharacterClicked);
			view.lockedCharacter.ClickedSignal.RemoveListener(LockedClicked);
			view.limitedEvent.ClickedSignal.RemoveListener(LimitedClicked);
			characterSignal.RemoveListener(OnCharacterClicked);
		}

		private int GetNumNewStickers()
		{
			int num = 0;
			foreach (Sticker item in playerService.GetInstancesByType<Sticker>())
			{
				if (view.isLimited)
				{
					if (item.isNew && item.Definition.IsLimitedTime)
					{
						num++;
					}
				}
				else if (item.isNew && !item.Definition.IsLimitedTime && item.Definition.CharacterID == view.prestigeID)
				{
					num++;
				}
			}
			return num;
		}

		private void OnCharacterClicked(int id, bool isLimited)
		{
			view.OnCharacterClicked(id, isLimited);
		}

		private void CharacterClicked()
		{
			view.UpdateBadge();
			toggleCharacterAudioSignal.Dispatch(false, view.MinionSlot.transform);
			characterSignal.Dispatch(view.prestigeID, false);
		}

		private void LockedClicked()
		{
			playSFXSignal.Dispatch("Play_action_locked_01");
		}

		private void LimitedClicked()
		{
			view.UpdateBadge();
			characterSignal.Dispatch(view.limitedID, true);
		}
	}
}
