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
	[RequireComponent(typeof(Animator))]
	public class MinionUpgradeView : PopupMenuView
	{
		private const string LevelupAnimationKey = "Levelup";

		private const string LevelupValueAnimationKey = "LevelupValue";

		private const string WaitAnimAbilitiesAnimationKey = "WaitAnimAbilities";

		public ButtonView Skrim;

		[Header("Level Selection")]
		public ScrollRect MinionLevelsScrollView;

		public GameObject levelSelectButtonOverlay;

		[Header("Minion Slot")]
		public Transform MinionSlot;

		public KampaiImage SilhouetteImage;

		public ButtonView LeftArrow;

		public ButtonView RightArrow;

		public Text CurrentMinionLevelDisplay;

		public ParticleSystem LevelUpParticleSystem;

		[Header("Abilities")]
		public ScrollRect BenefitScrollView;

		[Header("Goals")]
		public ScrollRect PopulationScrollView;

		public LocalizeView GoalDescriptionText;

		[Header("Levelup Token")]
		public GameObject LevelUpButtonPanel;

		public DoubleConfirmButtonView RushButton;

		public DoubleConfirmButtonView UpgradeButton;

		public Text UpgradeTokenCount;

		public Text RequiredTokenCountToLevel;

		public Text TokenRushCost;

		[Header("Controls")]
		public ButtonView CloseButton;

		internal int currentMinionLevelSelected;

		internal List<int> selectedMinionIndex;

		private readonly IList<MinionBenefitView> minionBenefitViews = new List<MinionBenefitView>();

		private readonly IList<MinionLevelSelectorView> minionLevelSelectorViews = new List<MinionLevelSelectorView>();

		private readonly IList<PopulationBenefitView> populationBenefitViews = new List<PopulationBenefitView>();

		private List<Minion> currentMinionList;

		private IDefinitionService definitionService;

		private DummyCharacterObject displayedMinion;

		private IFancyUIService fancyUIService;

		private MinionBenefitLevelBandDefintion levelBandDef;

		private ILocalizationService localizationService;

		private IPlayerService playerService;

		private List<PopulationBenefitDefinition> populationDefinitions;

		private PlayGlobalSoundFXSignal soundFxSignal;

		private bool isAnimating;

		private bool isWaiting;

		private float waitAnimTime;

		private Coroutine waitForAnimAbilitiesFinishCoroutine;

		public RefreshAllOfTypeArgsSignal refreshAllOfTypeArgsSignal { get; set; }

		public RefreshFromIndexArgsSignal refreshFromIndexArgsSignal { get; set; }

		public int rushCost { get; set; }

		public int tokensToLevel { get; set; }

		public int GetCurrentMinionDefinitionID()
		{
			Minion minion = currentMinionList[selectedMinionIndex[currentMinionLevelSelected]];
			return minion.Definition.ID;
		}

		public void Init(IPlayerService playerService, IDefinitionService defService, IFancyUIService fancyService, ILocalizationService localService, PlayGlobalSoundFXSignal soundFxSignal)
		{
			base.Init();
			this.playerService = playerService;
			definitionService = defService;
			fancyUIService = fancyService;
			localizationService = localService;
			this.soundFxSignal = soundFxSignal;
			levelBandDef = definitionService.Get<MinionBenefitLevelBandDefintion>(89898);
			populationDefinitions = definitionService.GetAll<PopulationBenefitDefinition>();
			currentMinionList = playerService.GetMinionsByLevel(currentMinionLevelSelected);
			SetupSelectionIndexing();
			SetUpgradeTokenAmount();
			PopulateMinionLevelButtons();
			UpdateUpgradeButton();
			levelSelectButtonOverlay.SetActive(false);
			Open();
		}

		internal void DecrementIndex()
		{
			List<int> list;
			List<int> list2 = (list = selectedMinionIndex);
			int index;
			int index2 = (index = currentMinionLevelSelected);
			index = list[index];
			list2[index2] = index - 1;
			if (selectedMinionIndex[currentMinionLevelSelected] < 0)
			{
				selectedMinionIndex[currentMinionLevelSelected] = currentMinionList.Count - 1;
			}
			UpdateVisuals();
		}

		internal int GetCurrentMinionID()
		{
			return currentMinionList[selectedMinionIndex[currentMinionLevelSelected]].ID;
		}

		internal void IncrementIndex()
		{
			List<int> list;
			List<int> list2 = (list = selectedMinionIndex);
			int index;
			int index2 = (index = currentMinionLevelSelected);
			index = list[index];
			list2[index2] = index + 1;
			if (selectedMinionIndex[currentMinionLevelSelected] >= currentMinionList.Count)
			{
				selectedMinionIndex[currentMinionLevelSelected] = 0;
			}
			UpdateVisuals();
		}

		internal void LevelSelected(int indexSelected)
		{
			if (base.isOpened && !isAnimating)
			{
				int lowestLevel = MinionLevelSelectorView.GetLowestLevel(playerService, definitionService);
				currentMinionLevelSelected = ((indexSelected < lowestLevel) ? lowestLevel : indexSelected);
				if (currentMinionLevelSelected != indexSelected)
				{
					UpdateMinionLevelButton();
				}
				SetMinionDisplay(false);
				PopulatePopulationBenefits(false);
				UpdateMinionAbilities();
				UpdateUpgradeButton();
			}
		}

		private void UpdateMinionLevelButton()
		{
			refreshFromIndexArgsSignal.Dispatch(typeof(MinionLevelSelectorView), currentMinionLevelSelected, new GUIArguments(logger)
			{
				arguments = { 
				{
					typeof(int),
					(object)currentMinionLevelSelected
				} }
			});
		}

		internal void Release()
		{
			CleanupMinion();
			UIUtils.SafeDestoryViews(minionLevelSelectorViews);
			UIUtils.SafeDestoryViews(populationBenefitViews);
			UIUtils.SafeDestoryViews(minionBenefitViews);
		}

		internal void SetUpgradeTokenAmount()
		{
			UpgradeTokenCount.text = localizationService.GetString("QuantityItemFormat", playerService.GetQuantity(StaticItem.MINION_LEVEL_TOKEN));
		}

		internal void CleanupMinion()
		{
			if (!(displayedMinion == null))
			{
				displayedMinion.RemoveCoroutine();
				Object.Destroy(displayedMinion.gameObject);
			}
		}

		private void DisplayMinion(bool leveledUp)
		{
			selectedMinionIndex[currentMinionLevelSelected] = Mathf.Min(selectedMinionIndex[currentMinionLevelSelected], currentMinionList.Count - 1);
			Minion minion = currentMinionList[selectedMinionIndex[currentMinionLevelSelected]];
			int iD = minion.Definition.ID;
			CleanupMinion();
			displayedMinion = fancyUIService.BuildMinion(iD, leveledUp ? DummyCharacterAnimationState.Happy : DummyCharacterAnimationState.Idle, MinionSlot, true, true, minion.Level);
		}

		private void SetMinionDisplay(bool leveledUp)
		{
			currentMinionList = playerService.GetMinionsByLevel(currentMinionLevelSelected);
			LeftArrow.gameObject.SetActive(currentMinionList.Count > 1);
			RightArrow.gameObject.SetActive(currentMinionList.Count > 1);
			if (currentMinionList.Count >= 1)
			{
				DisplayMinion(leveledUp);
				SetMinionSelectionText();
				SilhouetteImage.gameObject.SetActive(false);
				return;
			}
			CleanupMinion();
			CurrentMinionLevelDisplay.gameObject.SetActive(false);
			if (currentMinionLevelSelected < levelBandDef.minionBenefitLevelBands.Count)
			{
				MinionBenefitLevel minionBenefitLevel = levelBandDef.minionBenefitLevelBands[currentMinionLevelSelected];
				string text = minionBenefitLevel.image;
				if (string.IsNullOrEmpty(text))
				{
					text = "btn_Main01_fill";
				}
				SilhouetteImage.sprite = UIUtils.LoadSpriteFromPath(text);
				string text2 = minionBenefitLevel.mask;
				if (string.IsNullOrEmpty(text2))
				{
					text2 = "btn_Main01_mask";
				}
				SilhouetteImage.maskSprite = UIUtils.LoadSpriteFromPath(text2);
			}
			SilhouetteImage.gameObject.SetActive(true);
		}

		private void SetMinionSelectionText()
		{
			CurrentMinionLevelDisplay.gameObject.SetActive(true);
			CurrentMinionLevelDisplay.text = string.Format("{0} / {1}", selectedMinionIndex[currentMinionLevelSelected] + 1, currentMinionList.Count);
		}

		private void SetScrollViewContentTransform(RectTransform rect)
		{
			if (!(rect == null))
			{
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.one;
				rect.localScale = Vector3.one;
				rect.localPosition = Vector3.zero;
			}
		}

		private void SetupSelectionIndexing()
		{
			selectedMinionIndex = new List<int>();
			for (int i = 0; i < levelBandDef.minionBenefitLevelBands.Count; i++)
			{
				selectedMinionIndex.Add(0);
			}
		}

		private void UpdateUpgradeButton()
		{
			if (currentMinionLevelSelected == levelBandDef.minionBenefitLevelBands.Count - 1 || currentMinionList.Count == 0)
			{
				LevelUpButtonPanel.gameObject.SetActive(false);
				return;
			}
			UpgradeButton.ResetTapState();
			RushButton.ResetTapState();
			LevelUpButtonPanel.gameObject.SetActive(true);
			tokensToLevel = levelBandDef.minionBenefitLevelBands[currentMinionLevelSelected].tokensToLevel;
			RequiredTokenCountToLevel.text = string.Format("x{0}", tokensToLevel);
			int quantity = (int)playerService.GetQuantity(StaticItem.MINION_LEVEL_TOKEN);
			if (quantity < tokensToLevel)
			{
				UpgradeButton.gameObject.SetActive(false);
				RushButton.gameObject.SetActive(true);
				rushCost = (tokensToLevel - quantity) * (int)definitionService.Get<ItemDefinition>(50).BasePremiumCost;
				TokenRushCost.text = rushCost.ToString();
				RushButton.ResetTapState();
			}
			else
			{
				UpgradeButton.gameObject.SetActive(true);
				RushButton.gameObject.SetActive(false);
				UpgradeButton.StartButtonPulse(true);
			}
		}

		private void UpdateVisuals()
		{
			SetMinionSelectionText();
			DisplayMinion(false);
		}

		private PopulationBenefitView CreatePopulationBenefit(int i)
		{
			if (i < populationBenefitViews.Count)
			{
				PopulationBenefitView populationBenefitView = populationBenefitViews[i];
				populationBenefitView.gameObject.SetActive(currentMinionLevelSelected != 0);
				return populationBenefitView;
			}
			GameObject original = KampaiResources.Load("cmp_PopulationBenefits") as GameObject;
			GameObject gameObject = Object.Instantiate(original);
			if (gameObject == null)
			{
				return null;
			}
			PopulationBenefitView component = gameObject.GetComponent<PopulationBenefitView>();
			component.transform.SetParent(PopulationScrollView.content, false);
			populationBenefitViews.Add(component);
			return component;
		}

		private void PopulatePopulationBenefits(bool checkDoober)
		{
			int count = populationDefinitions.Count;
			int num = 0;
			if (currentMinionLevelSelected == 0)
			{
				for (int i = 0; i < populationBenefitViews.Count; i++)
				{
					PopulationBenefitView populationBenefitView = populationBenefitViews[i];
					populationBenefitView.gameObject.SetActive(false);
				}
			}
			else
			{
				for (int j = 0; j < count; j++)
				{
					PopulationBenefitDefinition populationBenefitDefinition = populationDefinitions[j];
					if (currentMinionLevelSelected == populationBenefitDefinition.minionLevelRequired)
					{
						PopulationBenefitView populationBenefitView2 = CreatePopulationBenefit(num++);
						populationBenefitView2.benefitDefinitionID = populationBenefitDefinition.ID;
						populationBenefitView2.UpdateView(checkDoober);
					}
				}
			}
			SetScrollViewContentTransform(PopulationScrollView.content);
			PopulationScrollView.vertical = populationBenefitViews.Count > 3;
			GoalDescriptionText.gameObject.SetActive(currentMinionLevelSelected == 0);
		}

		private void PopulateMinionLevelButtons()
		{
			int count = levelBandDef.minionBenefitLevelBands.Count;
			if (minionLevelSelectorViews.Count >= count)
			{
				return;
			}
			GameObject original = KampaiResources.Load("cmp_MinionLevels") as GameObject;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = Object.Instantiate(original);
				if (!(gameObject == null))
				{
					MinionLevelSelectorView component = gameObject.GetComponent<MinionLevelSelectorView>();
					component.index = i;
					component.SetLevelText(localizationService.GetString("MinionUpgradeLevel", i + 1));
					component.transform.SetParent(MinionLevelsScrollView.content, false);
					minionLevelSelectorViews.Add(component);
				}
			}
			SetScrollViewContentTransform(MinionLevelsScrollView.content);
			MinionLevelsScrollView.horizontal = count > 4;
		}

		private void PopulateMinionBenefits()
		{
			GameObject original = KampaiResources.Load("cmp_MinionBenefit") as GameObject;
			int count = levelBandDef.benefitDescriptions.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = Object.Instantiate(original);
				if (!(gameObject == null))
				{
					MinionBenefitView component = gameObject.GetComponent<MinionBenefitView>();
					component.category = levelBandDef.benefitDescriptions[i].type;
					component.transform.SetParent(BenefitScrollView.content, false);
					minionBenefitViews.Add(component);
				}
			}
			SetScrollViewContentTransform(BenefitScrollView.content);
			BenefitScrollView.vertical = count > 3;
		}

		public void UpdateMinionAbilities()
		{
			refreshAllOfTypeArgsSignal.Dispatch(typeof(MinionBenefitView), new GUIArguments(logger)
			{
				arguments = { 
				{
					typeof(int),
					(object)currentMinionLevelSelected
				} }
			});
		}

		public void AnimAbilities()
		{
			waitAnimTime = 0f;
			isWaiting = false;
			Signal<float, Tuple<int, int>> signal = new Signal<float, Tuple<int, int>>();
			signal.AddListener(WaitForAnimAbilities);
			refreshAllOfTypeArgsSignal.Dispatch(typeof(MinionBenefitView), new GUIArguments(logger)
			{
				arguments = 
				{
					{
						typeof(int),
						(object)currentMinionLevelSelected
					},
					{
						typeof(bool),
						(object)true
					},
					{
						typeof(Signal<float, Tuple<int, int>>),
						(object)signal
					}
				}
			});
			signal.RemoveListener(WaitForAnimAbilities);
			base.animator.SetBool("WaitAnimAbilities", isWaiting);
			if (isWaiting)
			{
				if (waitForAnimAbilitiesFinishCoroutine != null)
				{
					StopCoroutine(waitForAnimAbilitiesFinishCoroutine);
					waitForAnimAbilitiesFinishCoroutine = null;
				}
				waitForAnimAbilitiesFinishCoroutine = StartCoroutine(WaitForAnimAbilitiesFinish(waitAnimTime));
			}
			else
			{
				AnimPopulationGoals();
			}
		}

		public void AnimLevelUpButton()
		{
			refreshFromIndexArgsSignal.Dispatch(typeof(MinionLevelSelectorView), currentMinionLevelSelected, new GUIArguments(logger)
			{
				arguments = 
				{
					{
						typeof(int),
						(object)currentMinionLevelSelected
					},
					{
						typeof(bool),
						(object)true
					}
				}
			});
			if (currentMinionLevelSelected >= 1)
			{
				GoalDescriptionText.gameObject.SetActive(false);
			}
		}

		public void StartVFX()
		{
			LevelUpParticleSystem.Play();
		}

		public void LevelMinion(int levelTo)
		{
			currentMinionLevelSelected = levelTo;
			selectedMinionIndex[currentMinionLevelSelected] = playerService.GetMinionsByLevel(currentMinionLevelSelected).Count - 1;
			LevelUpButtonPanel.gameObject.SetActive(false);
			isAnimating = true;
			base.animator.Play("Levelup");
			base.animator.SetInteger("LevelupValue", levelTo);
			levelSelectButtonOverlay.SetActive(true);
			if (currentMinionLevelSelected != 1)
			{
				PopulatePopulationBenefits(false);
			}
		}

		public void LoadMinionOnAnimationOpen()
		{
			SetMinionDisplay(false);
			PopulateMinionBenefits();
			PopulatePopulationBenefits(true);
			LevelSelected(MinionLevelSelectorView.GetLowestLevel(playerService, definitionService));
			soundFxSignal.Dispatch("Play_main_menu_open_01");
		}

		public void OnLevelupAnimationFinished()
		{
			isAnimating = false;
			isWaiting = false;
			PopulationScrollView.content.gameObject.SetActive(true);
			UpdateUpgradeButton();
			PopulatePopulationBenefits(true);
			LevelUpParticleSystem.Stop();
			levelSelectButtonOverlay.SetActive(false);
		}

		public void UpdateMinionCustom()
		{
			SetMinionDisplay(true);
		}

		private MinionBenefitView GetAbilitiesView(int index)
		{
			return minionBenefitViews[index];
		}

		public void WaitForAnimAbilities(float animTime, Tuple<int, int> index)
		{
			if (!(animTime < 0.01f))
			{
				isWaiting = true;
				StartCoroutine(WaitForAnimAbilitiesFinish(waitAnimTime, index));
				waitAnimTime += animTime;
			}
		}

		private IEnumerator WaitForAnimAbilitiesFinish(float delayTime, Tuple<int, int> index)
		{
			yield return new WaitForSeconds(delayTime);
			MinionBenefitView abilityView = GetAbilitiesView(index.Item1);
			if (!(abilityView == null))
			{
				abilityView.AnimateLevelBar(index.Item2);
			}
		}

		private IEnumerator WaitForAnimAbilitiesFinish(float time)
		{
			if (currentMinionLevelSelected == 1)
			{
				PopulationScrollView.content.gameObject.SetActive(false);
			}
			yield return new WaitForSeconds(time);
			AnimPopulationGoals();
			waitForAnimAbilitiesFinishCoroutine = null;
		}

		private void AnimPopulationGoals()
		{
			if (currentMinionLevelSelected == 1)
			{
				PopulationScrollView.content.gameObject.SetActive(false);
				base.animator.Play("Show Goals");
				PopulatePopulationBenefits(false);
			}
			else
			{
				OnLevelupAnimationFinished();
			}
		}

		internal uint GetTokensForCurrentMinion()
		{
			return (uint)levelBandDef.minionBenefitLevelBands[currentMinionLevelSelected].tokensToLevel;
		}
	}
}
