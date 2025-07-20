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
	public class MasterPlanRewardView : PopupMenuView
	{
		public ButtonView collectButton;

		public Text collectButtonText;

		public Text content;

		public Text title;

		[Header("Normal MasterPlan Reward Only")]
		public RectTransform scrollViewTransform;

		public ScrollRect scrollRect;

		[Header("CoolDown MasterPlan Reward Only")]
		public RectTransform rewardsPanel;

		public GameObject buildingSlot;

		public Text rewardCountText;

		public Text Description;

		public float padding = 10f;

		internal IDefinitionService definitionService;

		private ILocalizationService localizedService;

		private IFancyUIService fancyUIService;

		private IGUIService guiService;

		internal TransactionDefinition transactionDefinition;

		internal List<GameObject> viewList = new List<GameObject>();

		internal void Init(TransactionDefinition tD, ILocalizationService localService, IDefinitionService defService, IPlayerService playerService, IFancyUIService fancyUIService, IGUIService guiService, int planInstanceID)
		{
			base.Init();
			definitionService = defService;
			localizedService = localService;
			this.fancyUIService = fancyUIService;
			this.guiService = guiService;
			transactionDefinition = tD;
			if (transactionDefinition != null)
			{
				if (planInstanceID == 0)
				{
					SetupScrollView(RewardUtil.GetRewardQuantityFromTransaction(transactionDefinition, definitionService, playerService));
				}
				else
				{
					CreateRewardList(transactionDefinition);
				}
			}
			base.Open();
		}

		internal void SetupScrollView(List<RewardQuantity> quantityChange)
		{
			GameObject original = KampaiResources.Load("cmp_RewardSlider") as GameObject;
			float x = scrollViewTransform.sizeDelta.x;
			int count = quantityChange.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = Object.Instantiate(original);
				gameObject.transform.SetParent(scrollViewTransform, false);
				RewardSliderView component = gameObject.GetComponent<RewardSliderView>();
				component.scrollRect = scrollRect;
				UnlockDefinition definition;
				DisplayableDefinition displayableDefinition = ((quantityChange[i].ID == 0 || quantityChange[i].ID == 1 || quantityChange[i].ID == 5 || !definitionService.TryGet<UnlockDefinition>(quantityChange[i].ID, out definition)) ? definitionService.Get<DisplayableDefinition>(quantityChange[i].ID) : definitionService.Get<DisplayableDefinition>(definition.ReferencedDefinitionID));
				if (displayableDefinition != null)
				{
					component.description.text = localizedService.GetString(displayableDefinition.LocalizedKey);
					component.icon.sprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Image);
					component.icon.maskSprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Mask);
					component.itemQuantity.gameObject.SetActive(true);
					component.itemQuantity.text = UIUtils.FormatLargeNumber(quantityChange[i].Quantity);
					viewList.Add(gameObject);
				}
				else
				{
					logger.Warning("Social reward Item not valid {0}", i);
				}
			}
			int num = 2 * (int)x;
			int num2 = count * (int)x + (int)(padding * (float)count);
			int num3 = 0;
			if (num2 > num)
			{
				num3 = (num2 - num) / 2;
			}
			scrollViewTransform.sizeDelta = new Vector2(num2, x);
			scrollViewTransform.localPosition = new Vector2(num3, scrollViewTransform.localPosition.y);
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
				Building building;
				fancyUIService.CreateDummyBuildingObject(buildingDefinition, buildingSlot, out building);
				rewardCountText.text = localizedService.GetString(buildingDefinition.LocalizedKey);
				viewList.Add(buildingSlot);
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
				viewList.Add(component.gameObject);
			}
			return false;
		}
	}
}
