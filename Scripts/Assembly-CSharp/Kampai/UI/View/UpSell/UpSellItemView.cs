using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View.UpSell
{
	public class UpSellItemView : KampaiView
	{
		public GameObject Backing;

		public GameObject buildingStencil;

		public KampaiImage itemSlot;

		public GameObject buildingSlot;

		public MinionSlotModal minionSlot;

		public LocalizeView MTXItemQuantity;

		public LocalizeView MTXItemTitle;

		public Text PartyPointsTitle;

		public KampaiImage MTXGlowImage;

		public GameObject MTXPanel;

		public GameObject Exclusive;

		public LocalizeView itemQuantity;

		private int showMtxID;

		private Building building;

		private BuildingObject buildingObj;

		private IList<MinionObject> minionsList;

		private DummyCharacterObject dummyCharacterObject;

		private bool isAudible;

		private bool isSet;

		protected ILocalizationService localizationService;

		protected IDefinitionService definitionService;

		protected IKampaiLogger logger;

		protected IFancyUIService fancyUiService;

		protected MoveAudioListenerSignal moveAudioListenerSignal;

		private float additionalScale = 1f;

		public QuantityItem Item;

		public float AdditionalUIScale
		{
			get
			{
				return additionalScale;
			}
			set
			{
				additionalScale = value;
			}
		}

		internal virtual void Init(ILocalizationService localizationService, IFancyUIService fancyUiService, IDefinitionService definitionService, IKampaiLogger logger, MoveAudioListenerSignal moveAudioListenerSignal)
		{
			this.localizationService = localizationService;
			this.fancyUiService = fancyUiService;
			this.definitionService = definitionService;
			this.logger = logger;
			this.moveAudioListenerSignal = moveAudioListenerSignal;
			SetupItem();
		}

		public void SetupItem()
		{
			if (isSet || Item == null || definitionService == null)
			{
				return;
			}
			if (Backing != null)
			{
				Backing.SetActive(true);
			}
			Definition definition = definitionService.Get(Item.ID);
			BuildingDefinition buildingDefinition = definition as BuildingDefinition;
			if (buildingDefinition != null)
			{
				SetupBuildingItem(buildingDefinition);
			}
			MinionDefinition minionDefinition = definition as MinionDefinition;
			if (minionDefinition != null)
			{
				SetupMinion(minionDefinition);
			}
			ItemDefinition itemDefinition = definition as ItemDefinition;
			CurrencyItemDefinition currencyItemDefinition = null;
			if (itemDefinition != null)
			{
				if (showMtxID > 0 && (Item.ID == 0 || Item.ID == 1))
				{
					currencyItemDefinition = definitionService.Get<CurrencyItemDefinition>(showMtxID);
				}
				SetupItem(itemDefinition, currencyItemDefinition);
			}
			isSet = true;
			if (Item.Quantity > 1)
			{
				if (currencyItemDefinition != null)
				{
					MTXPanel.SetActive(true);
					MTXItemQuantity.text = Item.Quantity.ToString();
				}
				else
				{
					itemQuantity.gameObject.SetActive(true);
					itemQuantity.text = Item.Quantity.ToString();
				}
			}
		}

		public void ShowMtxID(int showMtxId)
		{
			showMtxID = showMtxId;
		}

		internal void SetAudible(bool isAudible)
		{
			this.isAudible = isAudible;
		}

		internal void Release()
		{
			if (fancyUiService != null)
			{
				fancyUiService.ReleaseBuildingObject(buildingObj, building, minionsList);
				moveAudioListenerSignal.Dispatch(true, null);
			}
			if (!(dummyCharacterObject == null))
			{
				dummyCharacterObject.RemoveCoroutine();
				Object.Destroy(dummyCharacterObject.gameObject);
			}
		}

		private void SetupItem(DisplayableDefinition itemDefinition, CurrencyItemDefinition currencyItemDefinition)
		{
			itemSlot.gameObject.SetActive(true);
			string iconPath = string.Empty;
			string maskPath = string.Empty;
			if (currencyItemDefinition != null)
			{
				MTXGlowImage.gameObject.SetActive(true);
				iconPath = currencyItemDefinition.Image;
				maskPath = currencyItemDefinition.Mask;
				MTXItemTitle.LocKey = currencyItemDefinition.LocalizedKey;
				GameObject original = KampaiResources.Load(currencyItemDefinition.VFX) as GameObject;
				GameObject gameObject = Object.Instantiate(original);
				gameObject.transform.SetParent(itemSlot.transform, false);
			}
			else if (itemDefinition != null)
			{
				int iD = itemDefinition.ID;
				if (iD == 0 || iD == 1)
				{
					MTXGlowImage.gameObject.SetActive(true);
					iconPath = itemDefinition.Image;
					maskPath = itemDefinition.Mask;
					MTXItemTitle.LocKey = itemDefinition.LocalizedKey;
					RectTransform rectTransform = MTXGlowImage.gameObject.transform as RectTransform;
					if (rectTransform != null)
					{
						rectTransform.localScale = new Vector3(1.1f, 1.1f, 1f);
					}
				}
				else
				{
					iconPath = itemDefinition.Image;
					maskPath = itemDefinition.Mask;
				}
			}
			fancyUiService.SetKampaiImage(itemSlot, iconPath, maskPath);
		}

		private void SetupMinion(MinionDefinition minionDefinition)
		{
			if (minionDefinition == null)
			{
				logger.Error("Minion Definition is null for item {0}", Item.ID);
				return;
			}
			minionSlot.gameObject.SetActive(true);
			dummyCharacterObject = fancyUiService.BuildMinion(minionDefinition.ID, DummyCharacterAnimationState.SelectedIdle, minionSlot.transform, true, isAudible);
			int childCount = dummyCharacterObject.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = dummyCharacterObject.transform.GetChild(i);
				if (child.name.Contains("LOD"))
				{
					SkinnedMeshRenderer component = child.GetComponent<SkinnedMeshRenderer>();
					component.updateWhenOffscreen = true;
				}
			}
			if (isAudible)
			{
				moveAudioListenerSignal.Dispatch(false, dummyCharacterObject.transform);
			}
		}

		private void SetupBuildingItem(BuildingDefinition buildingDef)
		{
			if (buildingDef == null)
			{
				logger.Error("Building definition is null {0}", Item.ID);
				return;
			}
			float num = buildingDef.UiScale;
			if (num < float.Epsilon)
			{
				num = 100f;
			}
			buildingSlot.transform.localScale = new Vector3(num, num, num);
			buildingSlot.transform.localPosition = buildingDef.UiPosition;
			SetupBuildingObject(buildingDef, num);
			buildingStencil.SetActive(true);
			buildingSlot.gameObject.SetActive(true);
			minionsList = new List<MinionObject>();
			buildingObj = fancyUiService.CreateDummyBuildingObject(buildingDef, buildingSlot, out building, minionsList, isAudible);
			fancyUiService.SetStenciledShaderOnBuilding(buildingSlot);
			if (buildingObj == null)
			{
				logger.Error("Failed to create a building object from building def {0}", buildingDef.ID);
				return;
			}
			if (isAudible)
			{
				moveAudioListenerSignal.Dispatch(false, buildingObj.transform);
			}
			if (!string.IsNullOrEmpty(buildingDef.PartyPointsLocalizedKey) && building != null)
			{
				LeisureBuildingDefintiion leisureBuildingDefintiion = building.Definition as LeisureBuildingDefintiion;
				if (leisureBuildingDefintiion != null)
				{
					PartyPointsTitle.transform.parent.gameObject.SetActive(true);
					PartyPointsTitle.text = localizationService.GetString(buildingDef.PartyPointsLocalizedKey, leisureBuildingDefintiion.PartyPointsReward);
				}
			}
		}

		private void SetupBuildingObject(BuildingDefinition buildingDef, float uiScale)
		{
			if (additionalScale < 1f)
			{
				buildingSlot.transform.localScale = new Vector3(uiScale, uiScale, uiScale) * additionalScale;
				buildingSlot.transform.localPosition = buildingDef.UiPosition * additionalScale;
			}
		}

		public void SetIsExclusive(bool isExclusive)
		{
			if (!(Exclusive == null))
			{
				Exclusive.SetActive(isExclusive);
			}
		}
	}
}
