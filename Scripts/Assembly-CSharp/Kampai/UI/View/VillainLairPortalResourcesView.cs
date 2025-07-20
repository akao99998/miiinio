using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class VillainLairPortalResourcesView : PopupMenuView
	{
		public ButtonView enterLair;

		public Text resourceProductionDescription;

		public Text resourceItemAmount;

		public KampaiImage resourceItemImage;

		public GameObject partyPanel;

		public Text partyBoostText;

		public ScrollRect scrollView;

		public GameObject LockStateGameObject;

		private ILocalizationService localizationService;

		private IDefinitionService definitionService;

		private IPlayerService playerService;

		private List<MonoBehaviour> sliderViews;

		private VillainLair lair;

		internal void Init(VillainLair lair, List<VillainLairResourcePlot> plots, ILocalizationService localizationService, IDefinitionService definitionService, IPlayerService playerService, ModalSettings modalSettings, BuildingPopupPositionData buildingPopupPositionData)
		{
			InitProgrammatic(buildingPopupPositionData);
			this.localizationService = localizationService;
			this.definitionService = definitionService;
			this.playerService = playerService;
			this.lair = lair;
			sliderViews = new List<MonoBehaviour>();
			SetupModalInfo(plots, modalSettings);
			base.Open();
		}

		internal void SetEnterLairButtonActive(bool active)
		{
			enterLair.GetComponent<Button>().interactable = active;
		}

		internal void SetupModalInfo(List<VillainLairResourcePlot> plots, ModalSettings modalSettings)
		{
			SetUpResourceInformation();
			int count = plots.Count;
			GameObject original = KampaiResources.Load("cmp_Resource_Lair") as GameObject;
			for (int i = 0; i < count; i++)
			{
				VillainLairResourcePlot villainLairResourcePlot = plots[i];
				if (sliderViews.Count < count)
				{
					if (villainLairResourcePlot.State != BuildingState.Inaccessible)
					{
						GameObject gameObject = Object.Instantiate(original);
						MinionSliderView component = gameObject.GetComponent<MinionSliderView>();
						sliderViews.Add(component);
						component.transform.SetParent(scrollView.content, false);
						UpdateView(i, modalSettings, plots[i]);
						continue;
					}
					GameObject gameObject2 = Object.Instantiate(LockStateGameObject, Vector3.zero, Quaternion.identity) as GameObject;
					if (gameObject2 == null)
					{
						continue;
					}
					gameObject2.SetActive(true);
					gameObject2.transform.SetParent(scrollView.content, false);
					sliderViews.Add(gameObject2.GetComponent<KampaiImage>());
				}
				if (villainLairResourcePlot.State != BuildingState.Inaccessible)
				{
					UpdateView(i, modalSettings, plots[i]);
				}
			}
		}

		internal void SetPartyInfo(float boost, string boostString, bool isOn = true)
		{
			partyBoostText.text = boostString;
			bool flag = isOn && (int)(boost * 100f) != 100;
			partyPanel.SetActive(isOn && flag);
			foreach (MonoBehaviour sliderView in sliderViews)
			{
				MinionSliderView minionSliderView = sliderView as MinionSliderView;
				if (minionSliderView != null)
				{
					minionSliderView.isCorrectBuffType = flag;
				}
			}
		}

		internal void UpdateDisplay()
		{
			resourceItemAmount.text = playerService.GetQuantityByDefinitionId(lair.Definition.ResourceItemID).ToString();
		}

		internal void UpdateView(int index, ModalSettings modalSettings, VillainLairResourcePlot currentPlot)
		{
			MinionSliderView minionSliderView = sliderViews[index] as MinionSliderView;
			if (!(minionSliderView == null))
			{
				minionSliderView.SetModalSettings(modalSettings);
				minionSliderView.resourcePlot = currentPlot;
				minionSliderView.isResourcePlotSlider = true;
				minionSliderView.identifier = index;
				minionSliderView.playerService = playerService;
				minionSliderView.buttonImage.sprite = resourceItemImage.sprite;
				minionSliderView.buttonImage.maskSprite = resourceItemImage.maskSprite;
				minionSliderView.UpdateHarvestTime();
				bool flag = currentPlot.State == BuildingState.Harvestable;
				bool flag2 = currentPlot.MinionIsTaskedToBuilding();
				if (flag)
				{
					minionSliderView.minionID = -1;
					minionSliderView.SetMinionSliderState(MinionSliderState.Harvestable);
				}
				else if (flag2)
				{
					minionSliderView.minionID = currentPlot.MinionIDInBuilding;
					minionSliderView.SetRushCost();
					minionSliderView.startTime = currentPlot.UTCLastTaskingTimeStarted;
					minionSliderView.SetMinionSliderState(MinionSliderState.Working);
				}
				else
				{
					minionSliderView.minionID = -1;
					minionSliderView.SetMinionSliderState(MinionSliderState.Available);
				}
			}
		}

		internal void SetUpResourceInformation()
		{
			IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(lair.Definition.ResourceItemID);
			int secondsToHarvest = lair.Definition.SecondsToHarvest;
			int quantityByDefinitionId = (int)playerService.GetQuantityByDefinitionId(ingredientsItemDefinition.ID);
			resourceProductionDescription.text = localizationService.GetString("ResourceProd", localizationService.GetString(ingredientsItemDefinition.LocalizedKey, 1), UIUtils.FormatTime(secondsToHarvest, localizationService));
			resourceItemAmount.text = quantityByDefinitionId.ToString();
			resourceItemImage.sprite = UIUtils.LoadSpriteFromPath(ingredientsItemDefinition.Image);
			resourceItemImage.maskSprite = UIUtils.LoadSpriteFromPath(ingredientsItemDefinition.Mask);
		}
	}
}
