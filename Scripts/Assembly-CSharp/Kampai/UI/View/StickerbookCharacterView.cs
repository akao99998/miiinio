using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class StickerbookCharacterView : KampaiView
	{
		public ScrollableButtonView character;

		public ScrollableButtonView lockedCharacter;

		public ScrollableButtonView limitedEvent;

		public MinionSlotModal MinionSlot;

		public KampaiImage lockedImage;

		public KampaiImage limitedImage;

		public RectTransform characterBadge;

		public RectTransform LTEBadge;

		public RectTransform selectionIcon;

		public RectTransform unlockedPanel;

		public RectTransform lockedPanel;

		public Text title;

		public Text characterBadgeText;

		public Text LTEBadgeText;

		private Building building;

		private BuildingObject buildingObj;

		private IList<MinionObject> minionsList;

		private DummyCharacterObject dummyCharacterObject;

		private bool isSelected;

		private IFancyUIService fancyService;

		public int prestigeID { get; set; }

		public int limitedID { get; set; }

		public bool isLimited { get; set; }

		public bool isLocked { get; set; }

		internal void Init(int numNewStickers, IFancyUIService fancyUIService, IPrestigeService prestigeService, ILocalizationService localService, IDefinitionService definitionService)
		{
			fancyService = fancyUIService;
			if (!isLimited)
			{
				PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(prestigeID);
				string @string = localService.GetString(prestigeDefinition.LocalizedKey);
				title.text = @string;
				if (!isLocked)
				{
					if (prestigeID != 0)
					{
						DummyCharacterType characterType = fancyUIService.GetCharacterType(prestigeID);
						dummyCharacterObject = fancyUIService.CreateCharacter(characterType, DummyCharacterAnimationState.Idle, MinionSlot.transform, MinionSlot.VillainScale, MinionSlot.VillainPositionOffset, prestigeID);
					}
					if (numNewStickers > 0)
					{
						characterBadge.gameObject.SetActive(true);
						characterBadgeText.text = numNewStickers.ToString();
					}
				}
				else
				{
					unlockedPanel.gameObject.SetActive(false);
					lockedPanel.gameObject.SetActive(true);
					Sprite characterImage = null;
					Sprite characterMask = null;
					prestigeService.GetCharacterImageBasedOnMood(prestigeDefinition, CharacterImageType.BigAvatarIcon, out characterImage, out characterMask);
					lockedImage.maskSprite = characterMask;
				}
			}
			else
			{
				limitedImage.gameObject.SetActive(false);
				title.text = localService.GetString("StickerbookEventHeader");
				BuildingDefinition buildingDefinition = definitionService.Get<BuildingDefinition>(1000012712);
				MinionSlot.transform.localScale = new Vector3(buildingDefinition.UiScale, buildingDefinition.UiScale, buildingDefinition.UiScale);
				MinionSlot.transform.localPosition = buildingDefinition.UiPosition;
				minionsList = new List<MinionObject>();
				buildingObj = fancyUIService.CreateDummyBuildingObject(buildingDefinition, MinionSlot.transform.gameObject, out building, minionsList, false);
				if (numNewStickers > 0)
				{
					LTEBadge.gameObject.SetActive(true);
					LTEBadgeText.text = numNewStickers.ToString();
				}
			}
		}

		internal void RemoveCoroutine()
		{
			if (dummyCharacterObject != null)
			{
				dummyCharacterObject.RemoveCoroutine();
				Object.Destroy(dummyCharacterObject.gameObject);
			}
			if (buildingObj != null)
			{
				fancyService.ReleaseBuildingObject(buildingObj, building, minionsList);
			}
		}

		internal void UpdateBadge()
		{
			if (isLimited)
			{
				LTEBadge.gameObject.SetActive(false);
			}
			else
			{
				characterBadge.gameObject.SetActive(false);
			}
		}

		internal void OnCharacterClicked(int id, bool isLimitedEvent)
		{
			if (dummyCharacterObject == null && !isLimited)
			{
				return;
			}
			if (!isLimited && id == prestigeID)
			{
				if (!isSelected)
				{
					selectionIcon.gameObject.SetActive(true);
					isSelected = true;
					if (dummyCharacterObject != null)
					{
						dummyCharacterObject.RemoveCoroutine(false);
						dummyCharacterObject.StartingState(DummyCharacterAnimationState.SelectedHappy);
						RectTransform rectTransform = MinionSlot.transform as RectTransform;
						Vector3 anchoredPosition3D = rectTransform.anchoredPosition3D;
						rectTransform.anchoredPosition3D = new Vector3(anchoredPosition3D.x, anchoredPosition3D.y, anchoredPosition3D.z + -900f);
					}
				}
			}
			else if (isLimited && isLimitedEvent)
			{
				if (!isSelected)
				{
					selectionIcon.gameObject.SetActive(true);
					isSelected = true;
					if (dummyCharacterObject != null)
					{
						dummyCharacterObject.RemoveCoroutine(false);
						dummyCharacterObject.StartingState(DummyCharacterAnimationState.SelectedHappy);
						RectTransform rectTransform2 = MinionSlot.transform as RectTransform;
						Vector3 anchoredPosition3D2 = rectTransform2.anchoredPosition3D;
						rectTransform2.anchoredPosition3D = new Vector3(anchoredPosition3D2.x, anchoredPosition3D2.y, anchoredPosition3D2.z + -900f);
					}
				}
			}
			else if (isSelected)
			{
				selectionIcon.gameObject.SetActive(false);
				isSelected = false;
				if (dummyCharacterObject != null)
				{
					RectTransform rectTransform3 = MinionSlot.transform as RectTransform;
					Vector3 anchoredPosition3D3 = rectTransform3.anchoredPosition3D;
					rectTransform3.anchoredPosition3D = new Vector3(anchoredPosition3D3.x, anchoredPosition3D3.y, anchoredPosition3D3.z - -900f);
					dummyCharacterObject.RemoveCoroutine(false);
					dummyCharacterObject.StartingState(DummyCharacterAnimationState.Idle, true);
				}
			}
		}
	}
}
