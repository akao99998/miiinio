using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class LevelUpRewardView : KampaiView
	{
		public float closeDelay = 1.4f;

		public float waitForDooberTimer = 2f;

		public float openForTimer = 3f;

		public Text levelText;

		public Animator animator;

		public RectTransform scrollView;

		public ScrollRect scrollRect;

		public MinionSlotModal minionSlot;

		public ButtonView skrimButton;

		public ButtonView skipButton;

		[Header("Reward Slider Attributes")]
		public float rewardSliderWidth = 80f;

		private IDefinitionService definitionService;

		private DummyCharacterObject dummyPhil;

		private IFancyUIService fancyService;

		private PlayGlobalSoundFXSignal audioSignal;

		internal Signal closeSignal = new Signal();

		internal Signal beginUnlockSignal = new Signal();

		internal Signal closeBuffInfoSignal = new Signal();

		internal int unlockCount;

		private bool partyUnlocked;

		internal Coroutine coroutine;

		internal Coroutine limitator;

		internal float timeTillForceClose;

		private List<RewardSliderView> sliderViews = new List<RewardSliderView>();

		internal void Init(IPlayerService playerService, IDefinitionService definitionService, ILocalizationService localization, IFancyUIService fancyUIService, PlayGlobalSoundFXSignal audioSignal, List<RewardQuantity> rewards, IGuestOfHonorService guestService)
		{
			this.audioSignal = audioSignal;
			this.definitionService = definitionService;
			fancyService = fancyUIService;
			levelText.text = string.Format(localization.GetString("LevelupLevel"), playerService.GetQuantity(StaticItem.LEVEL_ID));
			partyUnlocked = playerService.IsMinionPartyUnlocked();
			Display(rewards);
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			MinionPartyDefinition definition = minionPartyInstance.Definition;
			timeTillForceClose = (float)definition.GetPartyDuration(guestService.PartyShouldProduceBuff()) - openForTimer;
			if (timeTillForceClose < 0f)
			{
				timeTillForceClose = 0f;
			}
		}

		internal void StartAnimation()
		{
			if (unlockCount <= 0)
			{
				closeSignal.Dispatch();
				return;
			}
			foreach (Transform item in base.transform)
			{
				item.gameObject.SetActive(true);
			}
			coroutine = StartCoroutine(PlayAnimationSequence());
			if (timeTillForceClose > 0f)
			{
				limitator = StartCoroutine(Limitator());
			}
		}

		private void Display(List<RewardQuantity> quantityChange)
		{
			GameObject original = KampaiResources.Load("cmp_InspirationSlider") as GameObject;
			foreach (RewardQuantity item in quantityChange)
			{
				if ((item.ID != 21 || partyUnlocked) && !item.IsReward)
				{
					GameObject gameObject = Object.Instantiate(original);
					RewardSliderView component = gameObject.GetComponent<RewardSliderView>();
					LayoutElement layoutElement = gameObject.AddComponent<LayoutElement>();
					layoutElement.preferredWidth = rewardSliderWidth;
					gameObject.transform.SetParent(scrollView, false);
					UnlockDefinition unlockDefinition = definitionService.Get<UnlockDefinition>(item.ID);
					DisplayableDefinition displayableDefinition = definitionService.Get<DisplayableDefinition>(unlockDefinition.ReferencedDefinitionID);
					unlockCount++;
					component.icon.sprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Image);
					component.icon.maskSprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Mask);
					component.scrollRect = scrollRect;
					component.pointerDownSignal.AddListener(PointerDown);
					component.pointerUpSignal.AddListener(PointerUp);
					sliderViews.Add(component);
				}
			}
		}

		private void PointerDown()
		{
			if (limitator != null && coroutine != null)
			{
				StopCoroutine(coroutine);
				coroutine = null;
			}
		}

		private void PointerUp()
		{
			if (limitator != null)
			{
				StartClosing();
			}
		}

		private void StartClosing()
		{
			if (coroutine == null)
			{
				coroutine = StartCoroutine(CloseDelay());
			}
		}

		internal void CleanupListeners()
		{
			foreach (RewardSliderView sliderView in sliderViews)
			{
				sliderView.pointerDownSignal.RemoveListener(PointerDown);
				sliderView.pointerUpSignal.RemoveListener(PointerUp);
			}
		}

		public void SetupMinionSlot()
		{
			int prestigeDefinitionID = 40000;
			DummyCharacterType characterType = fancyService.GetCharacterType(prestigeDefinitionID);
			dummyPhil = fancyService.CreateCharacter(characterType, DummyCharacterAnimationState.Idle, minionSlot.transform, minionSlot.VillainScale, minionSlot.VillainPositionOffset, prestigeDefinitionID);
			dummyPhil.MakePhilDance();
		}

		public void CleanupCoroutine()
		{
			if (dummyPhil != null)
			{
				dummyPhil.RemoveCoroutine();
				Object.Destroy(dummyPhil.gameObject);
			}
		}

		private IEnumerator PlayAnimationSequence()
		{
			yield return new WaitForSeconds(waitForDooberTimer);
			audioSignal.Dispatch("Play_UI_levelUp_first_01");
			closeBuffInfoSignal.Dispatch();
			animator.Play("Open");
			coroutine = StartCoroutine(CloseDelay());
		}

		private IEnumerator Limitator()
		{
			yield return new WaitForSeconds(timeTillForceClose);
			StartClosing();
			limitator = null;
		}

		internal IEnumerator CloseDelay()
		{
			yield return new WaitForSeconds(openForTimer);
			yield return StartCoroutine(CloseDown());
		}

		internal IEnumerator CloseDown()
		{
			if (limitator != null)
			{
				StopCoroutine(limitator);
				limitator = null;
			}
			audioSignal.Dispatch("Play_UI_levelUp_last_01");
			animator.Play("Close");
			beginUnlockSignal.Dispatch();
			yield return new WaitForSeconds(closeDelay);
			closeSignal.Dispatch();
		}
	}
}
