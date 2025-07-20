using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class MoveBuildingMenuView : WorldToGlassView
	{
		internal ButtonView InventoryButton;

		internal ButtonView AcceptButton;

		internal ButtonView CloseButton;

		private Button inventoryButton;

		private Button acceptButton;

		private Button closeButton;

		private MoveBuildingModal myModal;

		private Vector3 originalScale = new Vector3(1f, 1f, 1f);

		protected override string UIName
		{
			get
			{
				return "MoveBuildingMenu";
			}
		}

		protected override void LoadModalData(WorldToGlassUIModal modal)
		{
			myModal = modal as MoveBuildingModal;
			MoveBuildingSetting moveBuildingSetting = myModal.Settings as MoveBuildingSetting;
			if (targetObject == null)
			{
				BuildingManagerMediator component = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER).GetComponent<BuildingManagerMediator>();
				targetObject = component.GetCurrentDummyBuilding();
			}
			InventoryButton = myModal.InventoryButton;
			AcceptButton = myModal.AcceptButton;
			CloseButton = myModal.CloseButton;
			inventoryButton = InventoryButton.GetComponent<Button>();
			acceptButton = AcceptButton.GetComponent<Button>();
			closeButton = CloseButton.GetComponent<Button>();
			myModal.gameObject.transform.SetAsLastSibling();
			if (moveBuildingSetting.Mask == 1)
			{
				inventoryButton.interactable = false;
			}
			if (moveBuildingSetting.pulseAcceptButton)
			{
				PulseAcceptButton(true);
			}
			ToggleCostPanel(moveBuildingSetting.ShowCostPanel);
		}

		internal void ReloadModal(WorldToGlassUIModal modal)
		{
			BuildingManagerMediator component = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER).GetComponent<BuildingManagerMediator>();
			targetObject = component.GetCurrentDummyBuilding();
			LoadModalData(modal);
		}

		public override Vector3 GetIndicatorPosition()
		{
			BuildingDefinitionObject buildingDefinitionObject = targetObject as BuildingDefinitionObject;
			if (buildingDefinitionObject != null)
			{
				return buildingDefinitionObject.ResourceIconPosition;
			}
			return Vector3.zero;
		}

		internal void DisableInventory()
		{
			inventoryButton.interactable = false;
		}

		internal void SetButtonState(int mask)
		{
			if ((mask & 1) > 0)
			{
				inventoryButton.interactable = false;
			}
			else
			{
				inventoryButton.interactable = true;
			}
			if ((mask & 4) > 0)
			{
				acceptButton.interactable = false;
			}
			else
			{
				acceptButton.interactable = true;
			}
			if ((mask & 8) > 0)
			{
				closeButton.interactable = false;
			}
			else
			{
				closeButton.interactable = true;
			}
		}

		internal void UpdateValidity(bool enable)
		{
			acceptButton.interactable = enable;
		}

		internal void ToggleCostPanel(bool enable)
		{
			if (!enable)
			{
				return;
			}
			Scaffolding instance = gameContext.injectionBinder.GetInstance<Scaffolding>();
			if (instance.Building != null)
			{
				myModal.ItemCostPanel.SetActive(false);
				return;
			}
			myModal.ItemCostPanel.SetActive(true);
			IDefinitionService instance2 = gameContext.injectionBinder.GetInstance<IDefinitionService>();
			BuildingDefinition definition = instance.Definition;
			ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(definition.ID);
			int num = byDefinitionId.Count * definition.IncrementalCost;
			foreach (QuantityItem input in instance.Transaction.Inputs)
			{
				if (input.ID == 0 || input.ID == 1)
				{
					ItemDefinition itemDefinition = instance2.Get<ItemDefinition>(input.ID);
					int num2 = (int)input.Quantity + num;
					SetItemCost(num2, itemDefinition);
					int quantityByDefinitionId = (int)playerService.GetQuantityByDefinitionId(input.ID);
					if (quantityByDefinitionId < num2)
					{
						myModal.ItemBacking.color = GameConstants.UI.UI_DARK_RED;
					}
					break;
				}
			}
		}

		internal void SetItemCost(int cost, ItemDefinition itemDefinition)
		{
			myModal.ItemCost.text = cost.ToString();
			UIUtils.SetItemIcon(myModal.ItemIcon, itemDefinition);
			myModal.ItemIcon.gameObject.SetActive(true);
			myModal.ItemCost.gameObject.SetActive(true);
		}

		internal void SetInventoryCount(int inventoryCount)
		{
			if (inventoryCount != 0)
			{
				myModal.InventoryCount.text = inventoryCount.ToString();
				myModal.ItemIcon.gameObject.SetActive(false);
				myModal.ItemCost.gameObject.SetActive(false);
				myModal.InventoryBacking.SetActive(true);
				myModal.InventoryCount.gameObject.SetActive(true);
			}
		}

		private void PulseAcceptButton(bool enablePulse)
		{
			if (AcceptButton.enabled)
			{
				Animator component = AcceptButton.GetComponent<Animator>();
				component.enabled = false;
				Animator[] componentsInChildren = AcceptButton.GetComponentsInChildren<Animator>();
				Animator[] array = componentsInChildren;
				foreach (Animator animator in array)
				{
					animator.enabled = false;
				}
				if (enablePulse)
				{
					TweenUtil.Throb(AcceptButton.transform, 0.85f, 0.5f, out originalScale);
					return;
				}
				Go.killAllTweensWithTarget(AcceptButton.transform);
				AcceptButton.transform.localScale = originalScale;
			}
		}
	}
}
