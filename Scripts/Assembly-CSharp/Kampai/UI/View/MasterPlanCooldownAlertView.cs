using System.Collections;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class MasterPlanCooldownAlertView : PopupMenuView
	{
		public Text timerText;

		public Text rewardCountText;

		public Text rushCostText;

		public ScrollableButtonView rushButton;

		public ButtonView waitButton;

		public GameObject buildingSlot;

		public Transform rewardsPanel;

		public int m_clockAnimationInternval = 1;

		internal Coroutine timerRoutine;

		internal int rushCost;

		private ILocalizationService localizationService;

		private ITimeEventService timeEventService;

		private IFancyUIService fancyUIService;

		private IDefinitionService definitionService;

		private IGUIService guiService;

		private Building building;

		private BuildingObject buildingObj;

		internal void Init(MasterPlan plan, bool HasReceivedFirstReward, ITimeEventService timeEventService, IDefinitionService definitionService, ILocalizationService localService, IFancyUIService fancyUIService, IGUIService guiService)
		{
			base.Init();
			localizationService = localService;
			this.timeEventService = timeEventService;
			this.fancyUIService = fancyUIService;
			this.definitionService = definitionService;
			this.guiService = guiService;
			rushButton.EnableDoubleConfirm();
			MasterPlanDefinition definition = plan.Definition;
			TransactionDefinition coolDownTransactionDef = (HasReceivedFirstReward ? definitionService.Get<TransactionDefinition>(definition.SubsequentCooldownRewardTransactionID) : definitionService.Get<TransactionDefinition>(definition.CooldownRewardTransactionID));
			CreateRewardList(coolDownTransactionDef);
			int timeRemaining = timeEventService.GetTimeRemaining(plan.ID);
			SetTimerValues(timeRemaining);
			timerRoutine = StartCoroutine(TimerCoroutine(timeRemaining));
			base.Open();
		}

		private void CreateRewardList(TransactionDefinition coolDownTransactionDef)
		{
			int outputCount = coolDownTransactionDef.GetOutputCount();
			for (int i = 0; i < outputCount; i++)
			{
				QuantityItem quantityItem = coolDownTransactionDef.Outputs[i];
				if (CreateRewardItem(quantityItem.ID, (int)quantityItem.Quantity))
				{
					break;
				}
			}
		}

		private bool CreateRewardItem(int itemDefID, int itemCount)
		{
			Definition definition = definitionService.Get<Definition>(itemDefID);
			BuildingDefinition buildingDefinition = definition as BuildingDefinition;
			if (buildingDefinition != null)
			{
				buildingSlot.transform.parent.gameObject.SetActive(true);
				buildingObj = fancyUIService.CreateDummyBuildingObject(buildingDefinition, buildingSlot, out building);
				rewardCountText.text = localizationService.GetString(buildingDefinition.LocalizedKey);
				return true;
			}
			ItemDefinition itemDefinition = definition as ItemDefinition;
			if (itemDefinition != null)
			{
				GameObject gameObject = guiService.Execute(GUIOperation.LoadUntrackedInstance, "cmp_MasterPlanCooldownRewardItem");
				MasterPlanCooldownRewardItemView component = gameObject.GetComponent<MasterPlanCooldownRewardItemView>();
				component.SetCount(itemCount);
				UIUtils.SetItemIcon(component.icon, itemDefinition);
				gameObject.transform.SetParent(rewardsPanel);
				gameObject.transform.localScale = Vector3.one;
			}
			return false;
		}

		private IEnumerator TimerCoroutine(int timeRemaining)
		{
			while (timeRemaining > 0)
			{
				yield return new WaitForSeconds(m_clockAnimationInternval);
				timeRemaining -= m_clockAnimationInternval;
				SetTimerValues(timeRemaining);
			}
		}

		private void SetTimerValues(int timeRemaining)
		{
			timerText.text = UIUtils.FormatTime(timeRemaining, localizationService);
			rushCost = timeEventService.CalculateRushCostForTimer(timeRemaining, RushActionType.CONSTRUCTION);
			rushCostText.text = rushCost.ToString();
		}

		internal void Cleanup()
		{
			if (buildingObj != null)
			{
				fancyUIService.ReleaseBuildingObject(buildingObj, building);
			}
		}
	}
}
