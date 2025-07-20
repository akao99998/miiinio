using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class StickerbookModalMediator : EventMediator
	{
		[Inject]
		public StickerbookModalView view { get; set; }

		[Inject]
		public UIAddedSignal uiAddedSignal { get; set; }

		[Inject]
		public UIRemovedSignal uiRemovedSignal { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public CharacterClickedSignal characterSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public HideItemPopupSignal hidePopupSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistence { get; set; }

		[Inject]
		public RequestCharacterSelectionSignal requestSignal { get; set; }

		[Inject]
		public MoveAudioListenerSignal toggleCharacterAudioSignal { get; set; }

		[Inject]
		public FadeBackgroundAudioSignal fadeBackgroundAudioSignal { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			soundFXSignal.Dispatch("Play_book_open_01");
			gameContext.injectionBinder.GetInstance<ToggleStickerbookGlowSignal>().Dispatch(false);
			if (localPersistence.HasKeyPlayer("StickerbookGlow"))
			{
				localPersistence.DeleteKeyPlayer("StickerbookGlow");
			}
			view.Init(prestigeService.GetPrestigedCharacterStates(), definitionService, playerService);
			view.OnMenuClose.AddListener(OnMenuClose);
			characterSignal.AddListener(CharacterClicked);
			uiAddedSignal.Dispatch(view.gameObject, OnHardwareBackButton);
			toggleCharacterAudioSignal.Dispatch(false, view.stickerPanel.transform);
			requestSignal.AddListener(SelectInitialCharacter);
			FadeBackgroundAudio(true);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			view.OnMenuClose.RemoveListener(OnMenuClose);
			characterSignal.RemoveListener(CharacterClicked);
			uiRemovedSignal.Dispatch(view.gameObject);
			requestSignal.RemoveListener(SelectInitialCharacter);
			FadeBackgroundAudio(false);
		}

		private void SelectInitialCharacter()
		{
			List<Sticker> instancesByType = playerService.GetInstancesByType<Sticker>();
			if (instancesByType.Count > 0)
			{
				StickerDefinition definition = instancesByType[instancesByType.Count - 1].Definition;
				if (definition.IsLimitedTime)
				{
					characterSignal.Dispatch(definition.EventDefinitionID, true);
				}
				else
				{
					characterSignal.Dispatch(definition.CharacterID, false);
				}
			}
			else
			{
				characterSignal.Dispatch(40000, false);
			}
		}

		private void CharacterClicked(int relevantID, bool isLimited)
		{
			if (view.lastSelectedID == relevantID)
			{
				return;
			}
			view.lastSelectedID = relevantID;
			soundFXSignal.Dispatch("Play_button_click_01");
			List<Sticker> list = new List<Sticker>();
			List<StickerDefinition> list2 = new List<StickerDefinition>();
			if (!isLimited)
			{
				foreach (StickerDefinition item in definitionService.GetAll<StickerDefinition>())
				{
					if (item.IsLimitedTime || item.CharacterID != relevantID)
					{
						continue;
					}
					Sticker firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Sticker>(item.ID);
					if (firstInstanceByDefinitionId != null)
					{
						if (firstInstanceByDefinitionId.isNew)
						{
							firstInstanceByDefinitionId.isNew = false;
						}
						list.Add(firstInstanceByDefinitionId);
					}
					else if (!item.deprecated && !item.IsLimitedTime)
					{
						list2.Add(item);
					}
				}
			}
			else
			{
				foreach (StickerDefinition item2 in definitionService.GetAll<StickerDefinition>())
				{
					if (!item2.IsLimitedTime)
					{
						continue;
					}
					Sticker firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<Sticker>(item2.ID);
					if (firstInstanceByDefinitionId2 != null)
					{
						if (firstInstanceByDefinitionId2.isNew)
						{
							firstInstanceByDefinitionId2.isNew = false;
						}
						list.Add(firstInstanceByDefinitionId2);
					}
					else if (!item2.deprecated)
					{
						SpecialEventItem firstInstanceByDefinitionId3 = playerService.GetFirstInstanceByDefinitionId<SpecialEventItem>(item2.EventDefinitionID);
						if (firstInstanceByDefinitionId3 != null)
						{
							list2.Add(item2);
						}
					}
				}
			}
			List<int> stickerList = SortStickers(list, list2);
			string stickerPageName = GetStickerPageName(isLimited, relevantID);
			view.PopulateStickersForCurrentCharacter(list.Count, stickerList);
			view.SetCharacterStrings(stickerPageName);
		}

		private List<int> SortStickers(List<Sticker> unlockedStickers, List<StickerDefinition> lockedStickers)
		{
			unlockedStickers.Sort((Sticker x, Sticker y) => y.UTCTimeEarned.CompareTo(x.UTCTimeEarned));
			List<int> list = new List<int>();
			foreach (Sticker unlockedSticker in unlockedStickers)
			{
				list.Add(unlockedSticker.Definition.ID);
			}
			foreach (StickerDefinition lockedSticker in lockedStickers)
			{
				list.Add(lockedSticker.ID);
			}
			return list;
		}

		private string GetStickerPageName(bool isLimited, int relevantID)
		{
			if (!isLimited)
			{
				return localService.GetString(definitionService.Get<PrestigeDefinition>(relevantID).CollectionTitle);
			}
			return localService.GetString("StickerbookEventName");
		}

		private void CloseButton()
		{
			soundFXSignal.Dispatch("Play_book_close_01");
			toggleCharacterAudioSignal.Dispatch(true, null);
			view.Close();
		}

		private void OnHardwareBackButton()
		{
			CloseButton();
		}

		private void OnMenuClose()
		{
			hidePopupSignal.Dispatch();
			hideSkrim.Dispatch("StickerBookSkrim");
			guiService.Execute(GUIOperation.Unload, "screen_Stickerbook");
		}

		private void FadeBackgroundAudio(bool fade)
		{
			if (zoomCameraModel.ZoomedIn && !zoomCameraModel.ZoomInProgress)
			{
				fadeBackgroundAudioSignal.Dispatch(fade, "Play_tikiBar_snapshotDuck_01");
			}
		}
	}
}
