using System;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class PlayerTrainingView : KampaiView
	{
		public Text trainingTitle;

		public List<Text> cardTitleTexts;

		public List<Text> cardDescriptionTexts;

		public ButtonView confirmButton;

		public Animator animator;

		public RectTransform cardOneCompositePanel;

		public RectTransform cardTwoCompositePanel;

		public RectTransform cardThreeCompositePanel;

		public List<MinionSlotModal> minionSlots;

		public KampaiImage transitionOne;

		public KampaiImage transitionTwo;

		public KampaiImage cardOneSingleImage;

		public KampaiImage cardTwoSingleImage;

		public KampaiImage cardThreeSingleImage;

		public List<KampaiImage> cardOneCompositeImages;

		public List<KampaiImage> cardTwoCompositeImages;

		public List<KampaiImage> cardThreeCompositeImages;

		internal Signal completeSignal = new Signal();

		internal Signal audioSignal = new Signal();

		internal List<int> prestigeDefinitionIDs = new List<int>();

		internal List<int> buildingDefinitionIDs = new List<int>();

		private List<Building> buildings = new List<Building>();

		private IList<MinionObject> minionsList;

		private IDefinitionService definitionService;

		private IFancyUIService fancyUIService;

		private List<DummyCharacterObject> dummyCharacters = new List<DummyCharacterObject>();

		private List<BuildingObject> dummyBuildings = new List<BuildingObject>();

		internal void Init(IDefinitionService defService, IFancyUIService UIService)
		{
			definitionService = defService;
			fancyUIService = UIService;
		}

		internal void SetTitle(string title)
		{
			trainingTitle.text = title;
		}

		internal void SetCardTitles(string titleOne, string titleTwo, string titleThree)
		{
			cardTitleTexts[0].text = titleOne;
			cardTitleTexts[1].text = titleTwo;
			cardTitleTexts[2].text = titleThree;
		}

		internal void SetCardDescriptions(string descriptionOne, string descriptionTwo, string descriptionThree)
		{
			cardDescriptionTexts[0].text = descriptionOne;
			cardDescriptionTexts[1].text = descriptionTwo;
			cardDescriptionTexts[2].text = descriptionThree;
		}

		internal void SetTransitionOne(string mask)
		{
			transitionOne.maskSprite = UIUtils.LoadSpriteFromPath(mask);
		}

		internal void SetTransitionTwo(string mask)
		{
			transitionTwo.maskSprite = UIUtils.LoadSpriteFromPath(mask);
		}

		internal void SetCardOneImages(List<ImageMaskCombo> images)
		{
			PopulateImages(cardOneSingleImage, cardOneCompositeImages, images, cardOneCompositePanel);
		}

		internal void SetCardTwoImages(List<ImageMaskCombo> images)
		{
			PopulateImages(cardTwoSingleImage, cardTwoCompositeImages, images, cardTwoCompositePanel);
		}

		internal void SetCardThreeImages(List<ImageMaskCombo> images)
		{
			PopulateImages(cardThreeSingleImage, cardThreeCompositeImages, images, cardThreeCompositePanel);
		}

		private void PopulateImages(KampaiImage singleImage, List<KampaiImage> compositeImages, List<ImageMaskCombo> images, RectTransform compositePanel)
		{
			if (images.Count == 1)
			{
				if (images[0].image.Equals("img_fill_128"))
				{
					singleImage.color = GameConstants.UI.UI_TEXT_LIGHT_BLUE;
				}
				singleImage.sprite = UIUtils.LoadSpriteFromPath(images[0].image);
				if (!string.IsNullOrEmpty(images[0].mask))
				{
					singleImage.maskSprite = UIUtils.LoadSpriteFromPath(images[0].mask);
				}
				compositePanel.gameObject.SetActive(false);
				return;
			}
			singleImage.gameObject.SetActive(false);
			for (int i = 0; i < images.Count; i++)
			{
				if (images[i].image.Equals("img_fill_128"))
				{
					compositeImages[i].color = GameConstants.UI.UI_TEXT_LIGHT_BLUE;
				}
				compositeImages[i].sprite = UIUtils.LoadSpriteFromPath(images[i].image);
				if (!string.IsNullOrEmpty(images[i].mask))
				{
					compositeImages[i].maskSprite = UIUtils.LoadSpriteFromPath(images[i].mask);
				}
			}
		}

		public void SetMinionCard(int cardNumber)
		{
			int num = prestigeDefinitionIDs[cardNumber];
			int num2 = buildingDefinitionIDs[cardNumber];
			if (num != 0)
			{
				CreateCharacter(num, cardNumber);
			}
			if (num2 != 0)
			{
				CreateBuilding(num2, cardNumber);
			}
		}

		private void CreateCharacter(int prestigeID, int cardNumber)
		{
			if (prestigeID == 99 || prestigeID == 191 || prestigeID == 192 || prestigeID == 193)
			{
				int[] mINION_DEFINITION_IDS = GameConstants.MINION_DEFINITION_IDS;
				System.Random random = new System.Random();
				int num = random.Next(mINION_DEFINITION_IDS.Length);
				int minionLevel = 0;
				switch (prestigeID)
				{
				case 191:
					minionLevel = 1;
					break;
				case 192:
					minionLevel = 2;
					break;
				case 193:
					minionLevel = 3;
					break;
				}
				dummyCharacters.Add(fancyUIService.BuildMinion(mINION_DEFINITION_IDS[num], DummyCharacterAnimationState.Idle, minionSlots[cardNumber].transform, true, true, minionLevel));
			}
			else
			{
				DummyCharacterType characterType = fancyUIService.GetCharacterType(prestigeID);
				dummyCharacters.Add(fancyUIService.CreateCharacter(characterType, DummyCharacterAnimationState.Idle, minionSlots[cardNumber].transform, minionSlots[cardNumber].VillainScale, minionSlots[cardNumber].VillainPositionOffset, prestigeID));
			}
		}

		private void CreateBuilding(int buildingID, int cardNumber)
		{
			BuildingDefinition buildingDefinition = definitionService.Get<BuildingDefinition>(buildingID);
			minionSlots[cardNumber].transform.localScale = new Vector3(buildingDefinition.UiScale, buildingDefinition.UiScale, buildingDefinition.UiScale);
			minionSlots[cardNumber].transform.localPosition = buildingDefinition.UiPosition;
			minionsList = new List<MinionObject>();
			Building building;
			dummyBuildings.Add(fancyUIService.CreateDummyBuildingObject(buildingDefinition, minionSlots[cardNumber].transform.gameObject, out building, minionsList, false));
			buildings.Add(building);
		}

		public void AnimationComplete()
		{
			completeSignal.Dispatch();
		}

		internal void RemoveCoroutine()
		{
			foreach (DummyCharacterObject dummyCharacter in dummyCharacters)
			{
				if (dummyCharacter != null)
				{
					dummyCharacter.RemoveCoroutine();
					UnityEngine.Object.Destroy(dummyCharacter.gameObject);
				}
			}
			for (int i = 0; i < dummyBuildings.Count; i++)
			{
				if (dummyBuildings[i] != null)
				{
					fancyUIService.ReleaseBuildingObject(dummyBuildings[i], buildings[i], minionsList);
				}
			}
		}

		public void TriggerAudio()
		{
			audioSignal.Dispatch();
		}
	}
}
