using System;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class StorageBuildingItemInfoView : PopupMenuView
	{
		public Text ItemText;

		public Text BuildingText;

		public Text CraftingTimeText;

		public KampaiImage ClocKampaiImage;

		public RectTransform downArrow;

		public RectTransform downOverlayArrow;

		public ScrollableButtonView gotoButton;

		internal ItemDefinition ItemDefinition;

		private ILocalizationService localizationService;

		private Camera uiCamera;

		private RectTransform itemTransform;

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		internal void Init(ILocalizationService localizationService, Camera camera)
		{
			base.Init();
			uiCamera = camera;
			this.localizationService = localizationService;
		}

		internal void SetItem(ItemDefinition itemDefinition, RectTransform itemCenter, Vector3 center, IDefinitionService definitionService)
		{
			ItemDefinition = itemDefinition;
			itemTransform = itemCenter;
			RectTransform rectTransform = base.transform as RectTransform;
			if (!(rectTransform == null))
			{
				UpdatePosition(center);
				setItemText();
				setBuildingText(definitionService);
				setCraftingTime(definitionService);
				gotoButton.gameObject.SetActive(itemDefinition is IngredientsItemDefinition);
				base.Open();
			}
		}

		private void UpdatePosition(Vector3 center)
		{
			RectTransform rectTransform = base.transform as RectTransform;
			if (!(rectTransform == null))
			{
				rectTransform.localScale = Vector3.one;
				Vector2 anchorMin = (rectTransform.anchorMax = center);
				rectTransform.anchorMin = anchorMin;
				float x = glassCanvas.GetComponent<CanvasScaler>().referenceResolution.x;
				rectTransform.anchoredPosition3D = Vector2.zero;
				float num = rectTransform.anchorMin.x - rectTransform.sizeDelta.x / x / 2f;
				if (num < 0.015f)
				{
					rectTransform.anchorMin = new Vector2(center.x - num + 0.02f, center.y);
					rectTransform.anchorMax = new Vector2(center.x - num + 0.02f, center.y);
					RectTransform rectTransform2 = downOverlayArrow;
					Vector2 anchoredPosition = new Vector2(downArrow.anchoredPosition.x - num * x, downArrow.anchoredPosition.y);
					downArrow.anchoredPosition = anchoredPosition;
					rectTransform2.anchoredPosition = anchoredPosition;
				}
				rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y + rectTransform.sizeDelta.y / 4f, rectTransform.localPosition.z);
			}
		}

		public Vector3 GetCenter()
		{
			Vector3[] array = new Vector3[4];
			itemTransform.GetWorldCorners(array);
			Vector3 position = default(Vector3);
			Vector3[] array2 = array;
			foreach (Vector3 vector in array2)
			{
				position += vector;
			}
			position /= 4f;
			return uiCamera.WorldToViewportPoint(position);
		}

		internal void setItemText()
		{
			if (ItemDefinition != null)
			{
				ItemText.text = localizationService.GetString(ItemDefinition.LocalizedKey);
			}
		}

		internal void setBuildingText(IDefinitionService definitionService)
		{
			IngredientsItemDefinition ingredientsItemDefinition = ItemDefinition as IngredientsItemDefinition;
			if (ingredientsItemDefinition != null)
			{
				int buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(ingredientsItemDefinition.ID);
				BuildingDefinition definition;
				if (definitionService.TryGet<BuildingDefinition>(buildingDefintionIDFromItemDefintionID, out definition))
				{
					BuildingText.text = string.Format(localizationService.GetString((definition.Type != BuildingType.BuildingTypeIdentifier.CRAFTING) ? "StorageBuildingTooltipHarvestBuilding" : "StorageBuildingTooltipCraftingBuilding"), localizationService.GetString(definition.LocalizedKey));
				}
				ClocKampaiImage.gameObject.SetActive(true);
				CraftingTimeText.gameObject.SetActive(true);
			}
			else
			{
				DropItemDefinition dropItemDefinition = ItemDefinition as DropItemDefinition;
				if (dropItemDefinition != null)
				{
					BuildingText.text = localizationService.GetString("StorageBuildingTooltipRandomDrop");
				}
				ClocKampaiImage.gameObject.SetActive(false);
				CraftingTimeText.gameObject.SetActive(false);
			}
		}

		internal void setCraftingTime(IDefinitionService definitionService)
		{
			IngredientsItemDefinition ingredientsItemDefinition = ItemDefinition as IngredientsItemDefinition;
			if (ingredientsItemDefinition != null)
			{
				uint harvestTimeFromIngredientDefinition = (uint)IngredientsItemUtil.GetHarvestTimeFromIngredientDefinition(ingredientsItemDefinition, definitionService);
				TimeSpan timeSpan = TimeSpan.FromSeconds(harvestTimeFromIngredientDefinition);
				CraftingTimeText.text = string.Format("{0:g}", timeSpan);
			}
		}
	}
}
