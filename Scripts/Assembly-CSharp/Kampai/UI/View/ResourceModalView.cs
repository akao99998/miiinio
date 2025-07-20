using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class ResourceModalView : PopupMenuView
	{
		public RectTransform scrollViewContent;

		public KampaiImage IconImage;

		public ButtonView LeftArrow;

		public ButtonView RightArrow;

		public GameObject PartyPanel;

		public Text modalName;

		public Text descriptionText;

		public Text ownedResourceText;

		public Text partyBoostText;

		private ResourceBuilding building;

		private List<MinionSliderView> views;

		private ILocalizationService localService;

		private IDefinitionService definitionService;

		private IPlayerService playerService;

		private int transactionId;

		internal Signal<int> resetDoubleTapSignal = new Signal<int>();

		internal void Init(ResourceBuilding building, List<Minion> minions, ILocalizationService localService, IDefinitionService definitionService, IPlayerService playerService, ModalSettings modalSettings, BuildingPopupPositionData buildingPopupPositionData)
		{
			InitProgrammatic(buildingPopupPositionData);
			this.localService = localService;
			this.definitionService = definitionService;
			this.playerService = playerService;
			views = new List<MinionSliderView>();
			SetupModalInfo(building, minions, modalSettings);
			base.Open();
		}

		internal void RecreateModal(ResourceBuilding building, List<Minion> minions, ModalSettings modalSettings)
		{
			SetupModalInfo(building, minions, modalSettings);
		}

		private void SetupModalInfo(ResourceBuilding building, List<Minion> minions, ModalSettings modalSettings)
		{
			this.building = building;
			ResourceBuildingDefinition definition = building.Definition;
			modalName.text = localService.GetString(definition.LocalizedKey);
			int harvestTimeForTaskableBuilding = BuildingUtil.GetHarvestTimeForTaskableBuilding(building, definitionService);
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(definition.ItemId);
			descriptionText.text = localService.GetString("ResourceProd", localService.GetString(itemDefinition.LocalizedKey, 1), UIUtils.FormatTime(harvestTimeForTaskableBuilding, localService));
			transactionId = building.GetTransactionID(definitionService);
			int harvestItemDefinitionIdFromTransactionId = definitionService.GetHarvestItemDefinitionIdFromTransactionId(transactionId);
			ItemDefinition itemDefinition2 = definitionService.Get<ItemDefinition>(harvestItemDefinitionIdFromTransactionId);
			IconImage.sprite = UIUtils.LoadSpriteFromPath(itemDefinition2.Image);
			IconImage.maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition2.Mask);
			UpdateDisplay();
			GameObject gameObject = KampaiResources.Load("cmp_buildInfo") as GameObject;
			RectTransform rectTransform = gameObject.transform as RectTransform;
			float y = rectTransform.sizeDelta.y;
			for (int i = 0; i < building.GetMaxSlotCount(); i++)
			{
				if (views.Count < building.GetMaxSlotCount())
				{
					MinionSliderView item = CreateView(i, y, gameObject);
					views.Add(item);
				}
				UpdateView(i, minions, modalSettings);
			}
			scrollViewContent.offsetMin = new Vector2(0f, (float)building.GetMaxSlotCount() * (0f - y));
		}

		internal void SetPartyInfo(float boost, string boostString, bool isOn = true)
		{
			partyBoostText.text = boostString;
			bool flag = isOn && (int)(boost * 100f) != 100;
			PartyPanel.SetActive(isOn && flag);
			foreach (MinionSliderView view in views)
			{
				view.isCorrectBuffType = flag;
			}
		}

		internal void SetArrowButtonState(bool enable)
		{
			LeftArrow.GetComponent<Button>().interactable = enable;
			RightArrow.GetComponent<Button>().interactable = enable;
		}

		private MinionSliderView CreateView(int index, float height, GameObject prefab)
		{
			GameObject gameObject = Object.Instantiate(prefab);
			Transform transform = gameObject.transform;
			MinionSliderView component = gameObject.GetComponent<MinionSliderView>();
			component.Init(localService, definitionService);
			float paddingInPixels = component.PaddingInPixels;
			transform.SetParent(scrollViewContent, false);
			RectTransform rectTransform = transform as RectTransform;
			rectTransform.localPosition = Vector3.zero;
			rectTransform.offsetMin = new Vector2(0f, (0f - (height + paddingInPixels)) * (float)index - height);
			rectTransform.offsetMax = new Vector2(0f, (0f - (height + paddingInPixels)) * (float)index);
			rectTransform.localScale = Vector3.one;
			return component;
		}

		private void UpdateView(int index, List<Minion> minions, ModalSettings modalSettings)
		{
			MinionSliderView minionSliderView = views[index];
			minionSliderView.SetModalSettings(modalSettings);
			minionSliderView.building = building;
			minionSliderView.identifier = index;
			minionSliderView.playerService = playerService;
			minionSliderView.buttonImage.sprite = IconImage.sprite;
			minionSliderView.buttonImage.maskSprite = IconImage.maskSprite;
			minionSliderView.UpdateHarvestTime();
			int count = minions.Count;
			int availableHarvest = building.GetAvailableHarvest();
			int num = index - availableHarvest;
			bool flag = index < availableHarvest;
			bool flag2 = num < count && num >= 0;
			bool flag3 = index < building.MinionSlotsOwned;
			if (flag && flag3)
			{
				minionSliderView.minionID = -1;
				minionSliderView.SetMinionSliderState(MinionSliderState.Harvestable);
				return;
			}
			if (flag2 && flag3)
			{
				minionSliderView.minionID = minions[num].ID;
				minionSliderView.SetRushCost();
				minionSliderView.costText.text = string.Format("{0}", building.Definition.RushCost);
				minionSliderView.startTime = minions[num].UTCTaskStartTime;
				minionSliderView.SetMinionSliderState(MinionSliderState.Working);
				return;
			}
			minionSliderView.minionID = -1;
			if (flag3)
			{
				minionSliderView.SetMinionSliderState(MinionSliderState.Available);
				return;
			}
			minionSliderView.lockedText.text = localService.GetString("BRBLockedSlot", building.GetSlotUnlockLevelByIndex(index));
			minionSliderView.lockedCostText.text = string.Format("{0}", building.GetSlotCostByIndex(index));
			minionSliderView.UpdateLockedButton();
			minionSliderView.SetMinionSliderState(MinionSliderState.Locked);
		}

		public void LevelUpUnlock(uint playerLevel)
		{
			if (views == null)
			{
				return;
			}
			foreach (MinionSliderView view in views)
			{
				if (view.identifier >= building.MinionSlotsOwned)
				{
					int slotUnlockLevelByIndex = building.GetSlotUnlockLevelByIndex(view.identifier);
					if (playerLevel < slotUnlockLevelByIndex)
					{
						view.lockedButton.GetComponent<Button>().interactable = true;
						break;
					}
					view.SetMinionSliderState(MinionSliderState.Available);
				}
			}
		}

		internal void UpdateDisplay()
		{
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(transactionId);
			if (transactionDefinition.Outputs[0] != null)
			{
				ownedResourceText.text = playerService.GetQuantityByDefinitionId(transactionDefinition.Outputs[0].ID).ToString();
			}
		}

		public void ResetRushButtonsState()
		{
			foreach (MinionSliderView view in views)
			{
				view.rushButton.ResetAnim();
			}
		}
	}
}
