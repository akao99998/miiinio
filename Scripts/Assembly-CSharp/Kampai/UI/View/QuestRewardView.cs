using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class QuestRewardView : PopupMenuView
	{
		public KampaiImage questGiverIcon;

		public MinionSlotModal MinionSlot;

		public List<Animator> objectiveAnimators = new List<Animator>();

		public List<Text> objectiveTexts = new List<Text>();

		public Text rewardDescription;

		public ButtonView collectButton;

		public ButtonView adVideoButton;

		public RectTransform scrollViewTransform;

		public GameObject collectButtonPanel;

		public float padding = 10f;

		public float animataionDelay = 1f;

		internal IDefinitionService definitionService;

		private ILocalizationService localizedService;

		private Vector3 originalScale;

		private DummyCharacterObject dummyCharacterObject;

		internal Coroutine rewardDisplay;

		internal TransactionDefinition transactionDefinition;

		internal List<GameObject> viewList = new List<GameObject>();

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		internal void Init(Quest quest, ILocalizationService localService, IDefinitionService defService, IPlayerService playerService, ModalSettings modalSettings, IFancyUIService fancyUIService, IQuestService questService)
		{
			base.Init();
			definitionService = defService;
			localizedService = localService;
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			SetupCharImage(quest, fancyUIService);
			IQuestController questController = questService.GetQuestControllerByDefinitionID(quest.GetActiveDefinition().ID) ?? questService.GetQuestControllerByInstanceID(quest.ID);
			SetupAndAnimateObjectives(questController);
			SetQuestDescription(quest, questService);
			transactionDefinition = quest.GetActiveDefinition().GetReward(definitionService);
			if (transactionDefinition != null)
			{
				SetupScrollView(quest, RewardUtil.GetRewardQuantityFromTransaction(transactionDefinition, definitionService, playerService));
			}
			originalScale = collectButton.transform.localScale;
			base.Open();
			if (modalSettings.enableCollectThrob)
			{
				HighlightCollect(true);
			}
		}

		public void EnableAdButton(bool adEnabled)
		{
			adVideoButton.gameObject.SetActive(adEnabled);
		}

		internal void CloseView()
		{
			if (dummyCharacterObject != null)
			{
				dummyCharacterObject.RemoveCoroutine();
				Object.Destroy(dummyCharacterObject.gameObject);
			}
			base.Close();
		}

		private void SetQuestDescription(Quest quest, IQuestService questService)
		{
			int questLineID = quest.GetActiveDefinition().QuestLineID;
			int completeCount = 0;
			if (questService.GetQuestLines().ContainsKey(questLineID))
			{
				CheckQuestProgress(questService.GetQuestLines()[questLineID], quest.GetActiveDefinition().ID, out completeCount);
			}
			string @string = localizedService.GetString("questRewardDescription");
			rewardDescription.text = string.Format(@string, questService.GetQuestName(quest.GetActiveDefinition().LocalizedKey), completeCount);
		}

		private void CheckQuestProgress(QuestLine questLine, int questDefinitionID, out int completeCount)
		{
			completeCount = 0;
			for (int num = questLine.Quests.Count - 1; num >= 0; num--)
			{
				QuestDefinition questDefinition = questLine.Quests[num];
				if (questDefinition.ID == questDefinitionID)
				{
					completeCount++;
					break;
				}
				if (questDefinition.QuestVersion != -1 && questDefinition.SurfaceType != QuestSurfaceType.Automatic)
				{
					completeCount++;
				}
			}
		}

		private void SetupAndAnimateObjectives(IQuestController questController)
		{
			int stepCount = questController.StepCount;
			for (int i = 0; i < stepCount; i++)
			{
				IQuestStepController stepController = questController.GetStepController(i);
				objectiveTexts[i].text = stepController.GetStepAction(localizedService) + " " + stepController.GetStepDescription(localizedService, definitionService);
			}
			rewardDisplay = StartCoroutine(DisplayRewards(stepCount));
		}

		private IEnumerator DisplayRewards(int count)
		{
			yield return null;
			if (dummyCharacterObject != null)
			{
				dummyCharacterObject.gameObject.SetActive(true);
				dummyCharacterObject.transform.localPosition = Vector3.zero;
			}
			yield return new WaitForSeconds(animataionDelay);
			for (int i = 0; i < count; i++)
			{
				soundFXSignal.Dispatch("Play_quest_checkMark_01");
				objectiveAnimators[i].Play("Open");
				yield return new WaitForSeconds(animataionDelay);
			}
		}

		public void SetupVillainAtlas()
		{
			questGiverIcon.maskSprite = UIUtils.LoadSpriteFromPath("icn_nav_villians_mask");
		}

		private void SetupCharImage(Quest quest, IFancyUIService fancyUIService)
		{
			int surfaceID = quest.GetActiveDefinition().SurfaceID;
			DummyCharacterType characterType = fancyUIService.GetCharacterType(surfaceID);
			dummyCharacterObject = fancyUIService.CreateCharacter(characterType, DummyCharacterAnimationState.SelectedHappy, MinionSlot.transform, MinionSlot.VillainScale, MinionSlot.VillainPositionOffset, surfaceID);
			dummyCharacterObject.gameObject.SetActive(false);
		}

		internal void SetupScrollView(Quest quest, List<RewardQuantity> quantityChange)
		{
			GameObject gameObject = KampaiResources.Load("cmp_QuestRewardSlider") as GameObject;
			float x = (gameObject.transform as RectTransform).sizeDelta.x;
			int count = quantityChange.Count;
			int rewardDisplayCount = quest.GetActiveDefinition().RewardDisplayCount;
			count = ((count <= rewardDisplayCount) ? count : rewardDisplayCount);
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject2 = Object.Instantiate(gameObject);
				gameObject2.transform.SetParent(scrollViewTransform, false);
				RectTransform rectTransform = gameObject2.transform as RectTransform;
				rectTransform.localPosition = Vector3.zero;
				rectTransform.localScale = Vector3.one;
				rectTransform.offsetMin = new Vector2(x * (float)i + padding * (float)i, 0f);
				rectTransform.offsetMax = new Vector2(x * (float)(i + 1) + padding * (float)i, 0f);
				QuestRewardSliderView component = gameObject2.GetComponent<QuestRewardSliderView>();
				UnlockDefinition definition;
				DisplayableDefinition displayableDefinition = ((quantityChange[i].ID == 0 || quantityChange[i].ID == 1 || quantityChange[i].ID == 5 || !definitionService.TryGet<UnlockDefinition>(quantityChange[i].ID, out definition)) ? definitionService.Get<DisplayableDefinition>(quantityChange[i].ID) : definitionService.Get<DisplayableDefinition>(definition.ReferencedDefinitionID));
				component.description.text = localizedService.GetString(displayableDefinition.LocalizedKey);
				component.icon.sprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Image);
				component.icon.maskSprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Mask);
				component.itemQuantity.text = UIUtils.FormatLargeNumber(quantityChange[i].Quantity);
				viewList.Add(component.icon.gameObject);
			}
			int num = 3 * (int)x;
			int num2 = count * ((int)x + (int)padding);
			num2 -= (int)padding;
			int num3 = 0;
			if (num2 > num)
			{
				num3 = (num2 - num) / 2;
			}
			scrollViewTransform.sizeDelta = new Vector2(num2, 0f);
			scrollViewTransform.localPosition = new Vector2((float)num3 / 2f, scrollViewTransform.localPosition.y);
		}

		internal void HighlightCollect(bool isHighlighted)
		{
			Animator[] componentsInChildren = collectButton.GetComponentsInChildren<Animator>();
			if (isHighlighted)
			{
				Animator[] array = componentsInChildren;
				foreach (Animator animator in array)
				{
					animator.enabled = false;
				}
				TweenUtil.Throb(collectButton.transform, 0.85f, 0.5f, out originalScale);
				return;
			}
			Go.killAllTweensWithTarget(collectButton.transform);
			collectButton.transform.localScale = originalScale;
			Animator[] array2 = componentsInChildren;
			foreach (Animator animator2 in array2)
			{
				animator2.enabled = true;
			}
		}
	}
}
