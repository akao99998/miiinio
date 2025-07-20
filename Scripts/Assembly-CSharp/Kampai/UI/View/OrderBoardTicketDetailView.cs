using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class OrderBoardTicketDetailView : KampaiView
	{
		private const string requiredItemPrefabPath = "cmp_OrderBoardTicketRequiredItem";

		public LocalizeView TicketName;

		public LocalizeView OrderInstruction;

		public RectTransform ScrollWindow;

		public RectTransform ScrollView;

		public GameObject RequiredItemListBackground;

		public RectTransform RewardPanel;

		public GameObject OrderPanel;

		public GameObject PrestigePanel;

		public GameObject GlowAnimation;

		public RectTransform PrestigeProgressBarFill;

		public Text ProgressBarText;

		public Text XPReward;

		public Text GrindReward;

		public Text PrestigeLevel;

		public MinionSlotModal MinionSlot;

		public KampaiImage FTUEBacking;

		public GameObject FunIcon;

		[Header("Buff Activated")]
		public GameObject BuffInfoGroup;

		public Text BuffMultiplierAmt;

		public KampaiImage BuffTypeIcon;

		public GameObject BuffRewardPanel;

		public Text BuffRewardAmt;

		public GameObject RewardsPanelOutline;

		private bool shouldShowBuff;

		private GameObject requiredItemPrefab;

		private DummyCharacterObject dummyCharacterObject;

		private float height;

		private List<OrderBoardRequiredItemView> itemList;

		private int count;

		private ILocalizationService localizationService;

		private Vector3 minionSlotScale;

		public void Init(ILocalizationService localService)
		{
			localizationService = localService;
			requiredItemPrefab = KampaiResources.Load("cmp_OrderBoardTicketRequiredItem") as GameObject;
			RectTransform rectTransform = requiredItemPrefab.transform as RectTransform;
			height = rectTransform.sizeDelta.y;
			itemList = new List<OrderBoardRequiredItemView>();
			dummyCharacterObject = null;
			minionSlotScale = MinionSlot.transform.localScale;
		}

		internal void ClearDummyObject()
		{
			if (dummyCharacterObject != null)
			{
				dummyCharacterObject.RemoveCoroutine();
				Object.Destroy(dummyCharacterObject.gameObject);
				dummyCharacterObject = null;
			}
		}

		internal List<OrderBoardRequiredItemView> GetItemList()
		{
			return itemList;
		}

		internal void SetFTUEText(string title)
		{
			PrestigePanel.SetActive(false);
			OrderInstruction.text = title;
			OrderInstruction.gameObject.SetActive(true);
			TicketName.gameObject.SetActive(false);
			FTUEBacking.gameObject.SetActive(false);
		}

		internal void SetSlotFullText(string messageLocKey)
		{
			OrderInstruction.LocKey = messageLocKey;
		}

		internal void SetTitle(string title)
		{
			TicketName.text = title;
		}

		internal void SetPrestigeProgress(float currentPrestigePoint, int neededPrestigePoints)
		{
			float num = currentPrestigePoint / (float)neededPrestigePoints;
			num = ((!(num > 1f)) ? num : 1f);
			if (PrestigeProgressBarFill != null && ProgressBarText != null)
			{
				PrestigeProgressBarFill.anchorMax = new Vector2(num, 1f);
				ProgressBarText.text = localizationService.GetString("PrestigeProgress", Mathf.FloorToInt(currentPrestigePoint), neededPrestigePoints);
			}
		}

		internal void SetReward(int grind, int xp, int additionalBuffGrind)
		{
			XPReward.text = xp.ToString();
			GrindReward.text = UIUtils.FormatLargeNumber(grind);
			SetBuffRewards(additionalBuffGrind);
		}

		internal void SetCharacter(DummyCharacterObject characterObject)
		{
			if (dummyCharacterObject != null)
			{
				dummyCharacterObject.RemoveCoroutine();
				Object.Destroy(dummyCharacterObject.gameObject);
			}
			dummyCharacterObject = characterObject;
		}

		internal void SetPanelState(bool isPrestige, int prestigeLevel = 0, Prestige character = null, bool orderInstructionEnabled = false)
		{
			OrderInstruction.gameObject.SetActive(orderInstructionEnabled);
			TicketName.gameObject.SetActive(true);
			FTUEBacking.gameObject.SetActive(true);
			if (isPrestige)
			{
				string title;
				if (prestigeLevel > 0)
				{
					title = ((character == null) ? localizationService.GetString("RePrestigeText") : localizationService.GetString("RePrestigeText", localizationService.GetString(character.Definition.LocalizedKey)));
					PrestigeLevel.text = (prestigeLevel + 1).ToString();
				}
				else
				{
					title = ((character == null) ? localizationService.GetString("PrestigeText") : localizationService.GetString("PrestigeText", localizationService.GetString(character.Definition.LocalizedKey)));
					PrestigeLevel.text = (prestigeLevel + 1).ToString();
				}
				SetTitle(title);
			}
			OrderPanel.SetActive(!isPrestige);
			PrestigePanel.SetActive(isPrestige);
			ScrollWindow.gameObject.SetActive(!orderInstructionEnabled);
			RewardPanel.gameObject.SetActive(!orderInstructionEnabled);
			RequiredItemListBackground.SetActive(!orderInstructionEnabled);
		}

		internal OrderBoardRequiredItemView CreateRequiredItem(int index, uint itemQuantity, uint itemInInventory, Sprite icon, Sprite mask)
		{
			GameObject gameObject = Object.Instantiate(requiredItemPrefab);
			OrderBoardRequiredItemView component = gameObject.GetComponent<OrderBoardRequiredItemView>();
			component.transform.SetParent(ScrollWindow, false);
			int num = index / 2;
			int num2 = index % 2;
			RectTransform rectTransform = component.transform as RectTransform;
			rectTransform.anchorMin = new Vector2(0.5f * (float)num2, 0f);
			rectTransform.anchorMax = new Vector2(0.5f * (float)(num2 + 1), 0f);
			rectTransform.offsetMin = new Vector2(0f, height * (float)count - height * (float)(num + 1));
			rectTransform.offsetMax = new Vector2(0f, height * (float)count - height * (float)num);
			rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);
			rectTransform.localScale = Vector3.one;
			bool flag = itemQuantity <= itemInInventory;
			component.CheckMark.SetActive(flag);
			component.XMark.SetActive(!flag);
			component.ItemCount.text = string.Format("{0}/{1}", itemInInventory, itemQuantity);
			component.ItemIcon.sprite = icon;
			component.ItemIcon.maskSprite = mask;
			component.playerHasEnoughItems = flag;
			itemList.Add(component);
			return component;
		}

		internal void SetupItemCount(int count)
		{
			if (itemList.Count != 0)
			{
				foreach (OrderBoardRequiredItemView item in itemList)
				{
					Object.Destroy(item.gameObject);
				}
				itemList.Clear();
			}
			this.count = count;
			ScrollWindow.offsetMin = new Vector2(0f, (0f - height) * (float)count);
			ScrollWindow.offsetMax = new Vector2(0f, 0f);
			if (count < 5)
			{
				ScrollView.GetComponent<ScrollRect>().enabled = false;
			}
			else
			{
				ScrollView.GetComponent<ScrollRect>().enabled = true;
			}
		}

		private void SetBuffRewards(int additionalGrind)
		{
			bool flag = additionalGrind > 0;
			BuffRewardPanel.SetActive(flag);
			SetBuffRewardsPanelGlow(flag);
			if (flag)
			{
				BuffRewardAmt.text = additionalGrind.ToString();
			}
		}

		internal void SetBuffRewardsPanelGlow(bool show)
		{
			RewardsPanelOutline.SetActive(show);
		}

		internal void ActivateBuffIcons(bool newShouldShowBuff, float modifier)
		{
			if (shouldShowBuff != newShouldShowBuff)
			{
				shouldShowBuff = newShouldShowBuff;
				BuffInfoGroup.SetActive(shouldShowBuff);
				if (shouldShowBuff)
				{
					BuffMultiplierAmt.text = localizationService.GetString("partyBuffMultiplier", modifier);
				}
			}
		}

		internal void DeactivateAllBuffVisuals()
		{
			RewardsPanelOutline.SetActive(false);
			BuffInfoGroup.SetActive(false);
			BuffRewardPanel.SetActive(false);
			shouldShowBuff = false;
		}

		internal void toggleMinionSlot(bool active)
		{
			MinionSlot.transform.localScale = ((!active) ? Vector3.zero : minionSlotScale);
		}
	}
}
