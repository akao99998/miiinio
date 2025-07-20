using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class MignetteCallMinionsView : PopupMenuView
	{
		public ButtonView leftArrow;

		public ButtonView rightArrow;

		public Text modalName;

		public Text minionsNeeded;

		public Text mignetteDescription;

		public ButtonView callMinionsButton;

		public Text rewardsString;

		public Text availableString;

		public Text minionsAvailable;

		public ProgressBarModal modal;

		public GameObject CallMinionGroup;

		public GameObject RushCooldownGroup;

		public Text XPReward;

		public List<MignetteRuleViewObject> mignetteRulesList;

		private MignetteBuilding mignetteBuilding;

		private ILocalizationService localizationService;

		private IPlayerService playerService;

		private GameObject minionManager;

		private int startTime;

		private int endTime;

		private Vector2 fillPosition;

		private int minionSlots;

		internal void Init(MignetteBuilding building, ILocalizationService localizationService, IPlayerService playerService, GameObject minionManager, BuildingPopupPositionData buildingPopupPositionData)
		{
			InitProgrammatic(buildingPopupPositionData);
			this.localizationService = localizationService;
			this.playerService = playerService;
			this.minionManager = minionManager;
			RecreateModal(building);
			base.Open();
		}

		internal void UpdateTime(int timeRemaining)
		{
			int num = endTime - startTime;
			float num2 = 1f - (float)timeRemaining / (float)num;
			fillPosition.x = num2;
			modal.fillImage.rectTransform.anchorMax = fillPosition;
			modal.percentageText.text = string.Format("{0}%", (int)(num2 * 100f));
			SetTimeRemainingText(timeRemaining);
		}

		private void OrganizeRulesPanel()
		{
			MignetteBuildingDefinition mignetteBuildingDefinition = mignetteBuilding.MignetteBuildingDefinition;
			int count = mignetteRulesList.Count;
			int count2 = mignetteBuildingDefinition.MignetteRules.Count;
			for (int i = 0; i < count; i++)
			{
				if (i < count2)
				{
					mignetteRulesList[i].gameObject.SetActive(true);
					mignetteRulesList[i].RenderRule(mignetteBuildingDefinition.MignetteRules[i]);
					mignetteRulesList[i].AmountLabel.text += localizationService.GetString("MignettePoints");
				}
				else
				{
					mignetteRulesList[i].gameObject.SetActive(false);
				}
			}
		}

		internal void SetArrowButtonsState(bool enable)
		{
			leftArrow.GetComponent<Button>().interactable = enable;
			rightArrow.GetComponent<Button>().interactable = enable;
		}

		internal void SetArrowButtonsVisibleAndActive(bool active)
		{
			leftArrow.gameObject.SetActive(active);
			rightArrow.gameObject.SetActive(active);
		}

		private void SetUpView()
		{
			MignetteBuildingDefinition mignetteBuildingDefinition = mignetteBuilding.MignetteBuildingDefinition;
			modalName.text = localizationService.GetString(mignetteBuildingDefinition.LocalizedKey);
			mignetteDescription.text = localizationService.GetString(mignetteBuildingDefinition.Description);
			XPReward.text = mignetteBuildingDefinition.XPRewardFactor.ToString();
			bool flag = mignetteBuilding.State == BuildingState.Cooldown;
			CallMinionGroup.SetActive(!flag);
			RushCooldownGroup.SetActive(flag);
			if (!playerService.HasPurchasedMinigamePack())
			{
				minionSlots = mignetteBuilding.GetMinionSlotsOwned();
				minionsNeeded.text = minionSlots.ToString();
				availableString.text = localizationService.GetString("MignetteMinionsAvailable");
			}
			UpdateView();
			rewardsString.text = localizationService.GetString("MignetteRewards");
			OrganizeRulesPanel();
		}

		internal void RecreateModal(MignetteBuilding building)
		{
			mignetteBuilding = building;
			SetUpView();
		}

		internal void StartTime(int startTime, int endTime)
		{
			SetTimeRemainingText(endTime - startTime);
			this.startTime = startTime;
			this.endTime = endTime;
			fillPosition = modal.fillImage.rectTransform.anchorMax;
		}

		internal void SetTimeRemainingText(int time)
		{
			int num = time / 3600;
			int num2 = time / 60 % 60;
			int num3 = time % 60;
			modal.timeRemainingText.text = string.Format("{0}:{1}:{2}", num.ToString("00"), num2.ToString("00"), num3.ToString("00"));
		}

		internal void SetRushCost(int rushCost)
		{
			modal.rushText.text = string.Format("{0}", rushCost);
		}

		internal void UpdateView()
		{
			if (minionManager == null)
			{
				return;
			}
			bool flag = playerService.HasStorageBuilding();
			if (!playerService.HasPurchasedMinigamePack())
			{
				int idleMinionCount = minionManager.GetComponent<MinionManagerMediator>().GetIdleMinionCount();
				minionsAvailable.text = idleMinionCount.ToString();
				flag &= idleMinionCount >= minionSlots;
			}
			callMinionsButton.GetComponent<Animator>().enabled = true;
			ScrollableButtonView component = callMinionsButton.GetComponent<ScrollableButtonView>();
			if (!flag)
			{
				component.Disable();
			}
			else
			{
				component.ResetAnim();
				if (playerService.GetHighestFtueCompleted() < 999999)
				{
					StartCoroutine(StartCallButtonPulse());
				}
			}
			callMinionsButton.GetComponent<Button>().interactable = flag;
		}

		private IEnumerator StartCallButtonPulse()
		{
			yield return null;
			callMinionsButton.GetComponent<Animator>().enabled = false;
			Vector3 dummyVector = Vector3.one;
			TweenUtil.Throb(callMinionsButton.transform, 0.85f, 0.5f, out dummyVector);
		}
	}
}
