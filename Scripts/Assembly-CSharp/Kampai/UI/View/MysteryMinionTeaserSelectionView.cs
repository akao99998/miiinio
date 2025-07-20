using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class MysteryMinionTeaserSelectionView : PopupMenuView
	{
		public ButtonView confirmButton;

		[Header("Minion Effects")]
		public KampaiImage Minion2DImage;

		public ParticleSystem RevealVFX;

		public MinionSlotModal MinionSlot;

		[Header("Reward 1 Panel")]
		public ButtonView choice1Button;

		public Animator choice1PulseAnimator;

		public KampaiImage choice1_icon1;

		public Text choice1_icon1_amt;

		public KampaiImage choice1_icon2;

		public Text choice1_icon2_amt;

		[Header("Reward 1 Selected")]
		public GameObject choice1_SelectedActivePanel;

		[Header("Reward 1 DeSelected")]
		public GameObject choice1_ButtonDeselectedInitialColor;

		public GameObject choice1_DeSelectedActivePanel;

		[Header("Reward 2 Panel")]
		public ButtonView choice2Button;

		public Animator choice2PulseAnimator;

		public KampaiImage choice2_icon1;

		public Text choice2_icon1_amt;

		public KampaiImage choice2_icon2;

		public Text choice2_icon2_amt;

		[Header("Reward 2 Selected")]
		public GameObject choice2_SelectedActivePanel;

		[Header("Reward 2 DeSelected")]
		public GameObject choice2_ButtonDeselectedInitialColor;

		public GameObject choice2_DeSelectedActivePanel;

		private bool initialSelectionMade;

		private IFancyUIService fancyUIService;

		private PlayGlobalSoundFXSignal playAudioSignal;

		private DummyCharacterObject dummyCharacterObject;

		public void Initialize(IFancyUIService fancyService, PlayGlobalSoundFXSignal playSFXSignal)
		{
			Minion2DImage.gameObject.SetActive(true);
			base.Init();
			base.Open();
			fancyUIService = fancyService;
			playAudioSignal = playSFXSignal;
			confirmButton.gameObject.SetActive(false);
		}

		public void SetUpRewardIconDisplayable(KampaiImage icon, DisplayableDefinition def, Text amtText, uint amt)
		{
			icon.sprite = UIUtils.LoadSpriteFromPath(def.Image);
			icon.maskSprite = UIUtils.LoadSpriteFromPath(def.Mask);
			amtText.text = amt.ToString();
		}

		public void PlayerSelectedFirstReward(bool oneSelected)
		{
			if (!initialSelectionMade)
			{
				SetInitialSelection();
			}
			EnableSelectedButtonOnFirstReward(oneSelected);
		}

		internal void SetInitialSelection()
		{
			initialSelectionMade = true;
			choice1_ButtonDeselectedInitialColor.SetActive(false);
			choice2_ButtonDeselectedInitialColor.SetActive(false);
			DisableSelectPulse();
			confirmButton.gameObject.SetActive(true);
			confirmButton.GetComponent<Button>().interactable = true;
		}

		internal void PulseSelectButtons()
		{
			choice1Button.StartButtonPulse(true);
			choice2Button.StartButtonPulse(true);
		}

		internal void DisableSelectPulse()
		{
			choice1PulseAnimator.Stop();
			choice2PulseAnimator.Stop();
		}

		public void TriggerVFXReveal()
		{
			playAudioSignal.Dispatch("Play_captain_poofReveal_01");
			RevealVFX.Play();
		}

		public void SpawnMinion()
		{
			Minion2DImage.gameObject.SetActive(false);
			dummyCharacterObject = fancyUIService.CreateCharacter(DummyCharacterType.NamedCharacter, DummyCharacterAnimationState.SelectedHappy, MinionSlot.transform, MinionSlot.VillainScale, MinionSlot.VillainPositionOffset, 40014);
			playAudioSignal.Dispatch("Play_captainReveal_stinger_01");
		}

		internal void Release()
		{
			if (dummyCharacterObject != null && dummyCharacterObject.gameObject != null)
			{
				Object.Destroy(dummyCharacterObject.gameObject);
				dummyCharacterObject = null;
			}
		}

		private void EnableSelectedButtonOnFirstReward(bool enableFirstReward)
		{
			choice1_SelectedActivePanel.SetActive(enableFirstReward);
			choice2_SelectedActivePanel.SetActive(!enableFirstReward);
			choice1_DeSelectedActivePanel.SetActive(!enableFirstReward);
			choice2_DeSelectedActivePanel.SetActive(enableFirstReward);
		}
	}
}
