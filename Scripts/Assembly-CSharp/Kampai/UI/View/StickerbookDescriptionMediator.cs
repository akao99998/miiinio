using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;

namespace Kampai.UI.View
{
	public class StickerbookDescriptionMediator : KampaiMediator
	{
		[Inject]
		public StickerbookDescriptionView view { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public HideItemPopupSignal hideItemPopupSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		public override void OnRegister()
		{
			hideItemPopupSignal.AddListener(Close);
			view.OnMenuClose.AddListener(OnMenuClose);
		}

		public override void OnRemove()
		{
			hideItemPopupSignal.RemoveListener(Close);
			view.OnMenuClose.RemoveListener(OnMenuClose);
		}

		public override void Initialize(GUIArguments args)
		{
			StickerDefinition stickerDef = args.Get<StickerDefinition>();
			bool isLocked = args.Get<bool>();
			Vector3 stickerCenter = args.Get<Vector3>();
			Register(stickerDef, isLocked, stickerCenter);
		}

		private void Register(StickerDefinition stickerDef, bool isLocked, Vector3 stickerCenter)
		{
			bool levelTooLow = false;
			Sticker sticker = null;
			if (!isLocked)
			{
				sticker = playerService.GetFirstInstanceByDefinitionId<Sticker>(stickerDef.ID);
				view.SetTitle(localizationService.GetString(stickerDef.LocalizedKey));
			}
			else if (stickerDef.UnlockLevel > playerService.GetQuantity(StaticItem.LEVEL_ID))
			{
				levelTooLow = true;
				view.SetTitle(localizationService.GetString("StickerbookStickerLockedPart1"));
			}
			else
			{
				view.SetTitle(localizationService.GetString(stickerDef.LocalizedKey));
			}
			SetViewDescription(levelTooLow, isLocked, sticker, stickerDef);
			view.Display(stickerCenter);
		}

		private void SetViewDescription(bool levelTooLow, bool isLocked, Sticker sticker, StickerDefinition stickerDef)
		{
			bool flag = true;
			foreach (KeyValuePair<int, bool> prestigedCharacterState in prestigeService.GetPrestigedCharacterStates(false))
			{
				if (prestigedCharacterState.Key == stickerDef.CharacterID && !prestigedCharacterState.Value)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				string @string = localizationService.GetString(definitionService.Get<PrestigeDefinition>(stickerDef.CharacterID).LocalizedKey);
				view.SetDescription(isLocked, sticker, localizationService.GetString("StickerbookCharLocked", @string), localizationService);
			}
			else if (levelTooLow)
			{
				view.SetDescription(isLocked, sticker, localizationService.GetString("StickerbookStickerLockedPart2", stickerDef.UnlockLevel), localizationService);
			}
			else
			{
				view.SetDescription(isLocked, sticker, localizationService.GetString(stickerDef.Description), localizationService);
			}
		}

		private void Close()
		{
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			view.Close();
		}

		private void OnMenuClose()
		{
			Object.Destroy(base.gameObject);
		}
	}
}
