using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class GOH_InfoPanelMediator : Mediator
	{
		private PrestigeDefinition prestigeDef;

		private BuffDefinition buffDefinition;

		private GuestOfHonorDefinition GOHDef;

		[Inject]
		public GOH_InfoPanelView view { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public MoveAudioListenerSignal toggleCharacterAudioSignal { get; set; }

		[Inject]
		public GOHCardClickedSignal gohCardClickedSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IGuestOfHonorService guestOfHonorService { get; set; }

		public override void OnRegister()
		{
			view.cooldownRushButton.ClickedSignal.AddListener(RushCooldown);
			view.gohPanelButton.ClickedSignal.AddListener(ClickedCharacterPanel);
			gohCardClickedSignal.AddListener(GOHCardClicked);
			SetUpCharacterInfo();
			SetBuffInfo();
			DetermineAvailability();
			view.Init(localizationService.GetString(prestigeDef.LocalizedKey), gohCardClickedSignal);
			if (view.initiallySelected)
			{
				toggleCharacterAudioSignal.Dispatch(false, view.minionSlot.transform);
				view.RegisterClicked();
			}
		}

		public override void OnRemove()
		{
			view.cooldownRushButton.ClickedSignal.RemoveListener(RushCooldown);
			gohCardClickedSignal.RemoveListener(GOHCardClicked);
			view.gohPanelButton.ClickedSignal.AddListener(ClickedCharacterPanel);
		}

		private void SetUpCharacterInfo()
		{
			prestigeDef = definitionService.Get<PrestigeDefinition>(view.prestigeDefID);
			GOHDef = definitionService.Get<GuestOfHonorDefinition>(prestigeDef.GuestOfHonorDefinitionID);
			buffDefinition = definitionService.Get<BuffDefinition>(GOHDef.buffDefinitionIDs[0]);
		}

		private void SetBuffInfo()
		{
			float buffMultiplierForPrestige = guestOfHonorService.GetBuffMultiplierForPrestige(prestigeDef.ID);
			string @string = localizationService.GetString("partyBuffMultiplier", buffMultiplierForPrestige);
			Sprite buffMaskIcon = UIUtils.LoadSpriteFromPath(buffDefinition.buffSimpleMask);
			string duration = UIUtils.FormatTime(guestOfHonorService.GetBuffDurationForSingleGuestOfHonorOnNextLevel(GOHDef), localizationService);
			view.SetBuffInfo(@string, buffMaskIcon, duration);
		}

		private void DetermineAvailability()
		{
			if (view.isLocked)
			{
				Sprite characterImage = null;
				Sprite characterMask = null;
				prestigeService.GetCharacterImageBasedOnMood(prestigeDef, CharacterImageType.BigAvatarIcon, out characterImage, out characterMask);
				if (guestOfHonorService.ShouldDisplayUnlockAtLevelText(prestigeDef.PreUnlockLevel, (uint)prestigeDef.ID))
				{
					view.SetCharacterLocked(characterMask, localizationService.GetString("InsufficientLevel", prestigeDef.PreUnlockLevel));
				}
				else
				{
					view.SetCharacterLocked(characterMask, localizationService.GetString("GOHUnlockWithOrders"));
				}
				return;
			}
			view.CreateAnimatedCharacter(fancyUIService);
			string @string = localizationService.GetString(buffDefinition.LocalizedKey);
			view.SetAvailabilityText(@string);
			if (view.cooldown != 0)
			{
				string string2 = localizationService.GetString("GOHPartiesNeeded*", view.cooldown);
				Prestige prestige = prestigeService.GetPrestige(prestigeDef.ID);
				string rushCost = guestOfHonorService.GetRushCostForPartyCoolDown(prestige.ID).ToString();
				view.SetCharacterInCooldown(string2, rushCost);
			}
			else
			{
				view.SetCharacterAvailable();
			}
		}

		private void RushCooldown()
		{
			soundFXSignal.Dispatch("Play_button_click_01");
			if (view.cooldownRushButton.isDoubleConfirmed())
			{
				view.RegisterClicked();
				view.rushCallBack.Dispatch();
			}
		}

		private void ClickedCharacterPanel()
		{
			soundFXSignal.Dispatch("Play_button_click_01");
			toggleCharacterAudioSignal.Dispatch(false, view.minionSlot.transform);
			view.RegisterClicked();
		}

		private void GOHCardClicked(int index, bool avail)
		{
			view.IndicateSelected(index == view.myIndex);
		}
	}
}
