using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class LeisureBuildingMenuView : PopupMenuView
	{
		public Text Title;

		public Text Production;

		public Text ClockTime;

		public Text MinionsNeeded;

		public Text RushCost;

		public Text IdleMinionCount;

		public Text MinionLevelText;

		public GameObject PartyBuffPanel;

		public Text PartyBuffAmt;

		public KampaiImage PartyPointsIcon;

		public KampaiImage LevelArrow;

		public ButtonView PrevBuilding;

		public ButtonView NextBuilding;

		public ScrollableButtonView CallMinions;

		public ButtonView CollectPoints;

		public ScrollableButtonView RushMinions;

		public GameObject ClockPanel;

		public GameObject ClockIcon;

		public GameObject PartyIcon;

		public Transform PartyIconTransform;

		public GameObject AvailableMinionsPanel;

		public GameObject CollectPanel;

		internal int rushCost;

		private int minionsNeeded;

		private int timeRemaining;

		private Vector3 originalScale;

		private ILocalizationService localService;

		private IDefinitionService definitionService;

		private IPlayerService playerService;

		private ITimeEventService timeEventService;

		private GameObject minionManager;

		private MinionParty minionParty;

		public int TimeRemaining
		{
			get
			{
				return timeRemaining;
			}
		}

		internal void Init(ILocalizationService localService, IDefinitionService definitionService, IPlayerService playerService, ITimeEventService timeEventService, GameObject minionManager, BuildingPopupPositionData positionData)
		{
			InitProgrammatic(positionData);
			this.localService = localService;
			this.definitionService = definitionService;
			this.playerService = playerService;
			this.timeEventService = timeEventService;
			this.minionManager = minionManager;
			minionParty = playerService.GetMinionPartyInstance();
			base.Open();
		}

		internal void SetTitle(string titleKey)
		{
			Title.text = localService.GetString(titleKey);
		}

		internal void SetProduction(string productionKey, int partyPoints, string time)
		{
			Production.text = localService.GetString(productionKey, partyPoints, time);
		}

		internal void SetClockTIme(LeisureBuilding building)
		{
			int leisureTimeDuration = building.Definition.LeisureTimeDuration;
			if (timeEventService.HasEventID(building.ID))
			{
				timeRemaining = timeEventService.GetTimeRemaining(building.ID);
			}
			else
			{
				timeRemaining = leisureTimeDuration;
			}
			timeRemaining = Mathf.Min(timeRemaining, leisureTimeDuration);
			ClockTime.text = UIUtils.FormatTime(timeRemaining, localService);
		}

		internal void SetRushCost(LeisureBuilding building)
		{
			if (building != null)
			{
				RushTimeBandDefinition rushTimeBandForTime = definitionService.GetRushTimeBandForTime(timeRemaining);
				rushCost = rushTimeBandForTime.GetCostForRushActionType(RushActionType.LEISURE);
				RushCost.text = rushCost.ToString();
			}
		}

		internal void SetPartyInfo(float boost, string boostString, bool isOn = true)
		{
			PartyBuffAmt.text = boostString;
			bool flag = isOn && (int)(boost * 100f) != 100;
			PartyBuffPanel.SetActive(flag);
			ClockIcon.SetActive(!flag);
			PartyIcon.SetActive(flag);
			if (flag)
			{
				Vector3 vector;
				TweenUtil.Throb(PartyIconTransform, 1.1f, 0.2f, out vector);
				UIUtils.FlashingColor(ClockTime, 0);
				return;
			}
			Go.killAllTweensWithTarget(PartyIconTransform);
			Go.killAllTweensWithTarget(ClockTime);
			ClockTime.color = Color.white;
			PartyIconTransform.localScale = Vector3.one;
		}

		internal void SetMinionsNeeded(int minionsNeeded)
		{
			this.minionsNeeded = minionsNeeded;
			MinionsNeeded.text = localService.GetString("RequiresXMinions*", minionsNeeded);
		}

		public bool IsCallButtonEnabled()
		{
			if (playerService.HasStorageBuilding())
			{
				return true;
			}
			return playerService.GetQuantity(StaticItem.LEVEL_ID) == 0 && minionParty.CurrentPartyPoints == 0;
		}

		public void DisableRushButton()
		{
			RushMinions.Disable();
			RushMinions.GetComponent<Button>().interactable = false;
		}

		internal void SetIdleMinionCount()
		{
			if (minionManager == null)
			{
				return;
			}
			MinionManagerMediator component = minionManager.GetComponent<MinionManagerMediator>();
			if (!(component == null))
			{
				int idleMinionCount = component.GetIdleMinionCount();
				IdleMinionCount.text = idleMinionCount.ToString();
				bool flag = idleMinionCount >= minionsNeeded && IsCallButtonEnabled();
				if (!flag)
				{
					CallMinions.Disable();
				}
				else
				{
					CallMinions.ResetAnim();
				}
				CallMinions.GetComponent<Button>().interactable = flag;
			}
		}

		internal void EnablePartyPoints(bool isEnabled)
		{
			Production.gameObject.SetActive(true);
			PartyPointsIcon.gameObject.SetActive(isEnabled);
		}

		internal void EnableCallMinion()
		{
			CallMinions.gameObject.SetActive(true);
			AvailableMinionsPanel.SetActive(true);
			RushMinions.gameObject.SetActive(false);
			ClockPanel.gameObject.SetActive(false);
			CollectPoints.gameObject.SetActive(false);
			CollectPanel.gameObject.SetActive(false);
		}

		internal void SetCallButtonInfo(int highestLevel)
		{
			LevelArrow.gameObject.SetActive(highestLevel != 0);
			MinionLevelText.text = (highestLevel + 1).ToString();
		}

		internal void EnableRush()
		{
			RushMinions.EnableDoubleConfirm();
			RushMinions.ResetAnim();
			RushMinions.gameObject.SetActive(true);
			ClockPanel.gameObject.SetActive(true);
			CallMinions.gameObject.SetActive(false);
			AvailableMinionsPanel.SetActive(false);
			CollectPoints.gameObject.SetActive(false);
			CollectPanel.gameObject.SetActive(false);
		}

		internal void EnableCollect()
		{
			CollectPoints.gameObject.SetActive(true);
			CollectPanel.gameObject.SetActive(true);
			RushMinions.gameObject.SetActive(false);
			ClockPanel.gameObject.SetActive(false);
			CallMinions.gameObject.SetActive(false);
			AvailableMinionsPanel.SetActive(false);
		}

		internal void SetArrowsInteractable(bool isInteractable)
		{
			PrevBuilding.GetComponent<Button>().interactable = isInteractable;
			NextBuilding.GetComponent<Button>().interactable = isInteractable;
		}

		internal void SetArrowsActive(bool isActive)
		{
			PrevBuilding.gameObject.SetActive(isActive);
			NextBuilding.gameObject.SetActive(isActive);
		}

		internal void Throb(ScrollableButtonView button, bool throb)
		{
			if (!button.enabled || minionManager == null)
			{
				return;
			}
			int idleMinionCount = minionManager.GetComponent<MinionManagerMediator>().GetIdleMinionCount();
			if (idleMinionCount < minionsNeeded)
			{
				return;
			}
			Animator[] componentsInChildren = button.GetComponentsInChildren<Animator>();
			if (throb)
			{
				Animator[] array = componentsInChildren;
				foreach (Animator animator in array)
				{
					animator.enabled = false;
				}
				TweenUtil.Throb(button.transform, 0.85f, 0.5f, out originalScale);
			}
			else if (originalScale != Vector3.zero)
			{
				Go.killAllTweensWithTarget(button.transform);
				button.transform.localScale = originalScale;
				Animator[] array2 = componentsInChildren;
				foreach (Animator animator2 in array2)
				{
					animator2.enabled = true;
				}
			}
		}

		internal void Cleanup()
		{
			Go.killAllTweensWithTarget(PartyIconTransform);
			Go.killAllTweensWithTarget(ClockTime.transform);
			Go.killAllTweensWithTarget(CallMinions.transform);
			Go.killAllTweensWithTarget(RushMinions.transform);
		}
	}
}
