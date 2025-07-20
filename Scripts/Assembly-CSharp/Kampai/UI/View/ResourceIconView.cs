using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class ResourceIconView : WorldToGlassView
	{
		internal Signal RemoveResourceIconSignal = new Signal();

		internal int ItemDefID;

		protected KampaiImage m_image;

		protected Text m_text;

		protected KampaiImage m_textBackground;

		protected KampaiImage m_backing;

		protected KampaiImage m_backingStroke;

		private Vector3 localScale;

		private bool hasText;

		private Vector3 iconIndexOffset;

		private bool isBonus;

		private IDefinitionService definitionService;

		internal LeisureBuilding leisureBuilding;

		private MIBBuilding mibBuilding;

		public Signal ClickedSignal { get; private set; }

		public float IconIndex { get; private set; }

		protected override string UIName
		{
			get
			{
				return "ResourceIcon";
			}
		}

		internal void Init(ICrossContextCapable gameContext, IKampaiLogger logger, IPlayerService playerService, IDefinitionService definitionService, IPositionService positionService, ILocalizationService localizationService)
		{
			this.definitionService = definitionService;
			Init(positionService, gameContext, logger, playerService, localizationService);
			localScale = m_transform.localScale;
		}

		protected override void LoadModalData(WorldToGlassUIModal modal)
		{
			ResourceIconModal resourceIconModal = modal as ResourceIconModal;
			if (resourceIconModal == null)
			{
				logger.Error("Resource modal doesn't exist!");
				return;
			}
			ResourceIconSettings resourceIconSettings = resourceIconModal.Settings as ResourceIconSettings;
			ClickedSignal = resourceIconModal.ClickedSignal;
			m_image = resourceIconModal.Image;
			m_text = resourceIconModal.Text;
			m_textBackground = resourceIconModal.TextBackground;
			m_backing = resourceIconModal.Backing;
			m_backingStroke = resourceIconModal.BackingStroke;
			UpdateIconCount(resourceIconSettings.Count);
			ItemDefID = resourceIconSettings.ItemDefId;
			isBonus = resourceIconSettings.isRare;
			Building byInstanceId = playerService.GetByInstanceId<Building>(m_trackedId);
			leisureBuilding = byInstanceId as LeisureBuilding;
			mibBuilding = byInstanceId as MIBBuilding;
			ItemDefinition definition;
			definitionService.TryGet<ItemDefinition>(ItemDefID, out definition);
			if (leisureBuilding != null)
			{
				UpdateBuildingView("img_throwparty_fill", "img_throwparty_mask");
				resourceIconModal.EnablePartyIcon();
			}
			else if (mibBuilding != null)
			{
				UpdateBuildingView("icn_MessageInABottle_fill", "icn_MessageInABottle_mask");
				UIOffset = mibBuilding.Definition.HarvestableIconOffset;
			}
			else if (byInstanceId != null && definition != null)
			{
				UpdateView(byInstanceId.Definition, definition);
			}
			else
			{
				logger.Warning("Unable to find building with id: {0} or itemDef with id: {1} ", m_trackedId, ItemDefID);
			}
		}

		private void UpdateView(BuildingDefinition buildingDef, ItemDefinition itemDef)
		{
			if (buildingDef is DebrisBuildingDefinition)
			{
				HideText();
				hasText = false;
			}
			else
			{
				m_image.color = Color.white;
				m_image.sprite = UIUtils.LoadSpriteFromPath(itemDef.Image);
				m_image.maskSprite = UIUtils.LoadSpriteFromPath(itemDef.Mask);
				hasText = true;
			}
			TaskableBuildingDefinition taskableBuildingDefinition = buildingDef as TaskableBuildingDefinition;
			CraftingBuildingDefinition craftingBuildingDefinition = buildingDef as CraftingBuildingDefinition;
			VillainLairEntranceBuildingDefinition villainLairEntranceBuildingDefinition = buildingDef as VillainLairEntranceBuildingDefinition;
			if (taskableBuildingDefinition != null)
			{
				UIOffset = taskableBuildingDefinition.HarvestableIconOffset;
			}
			else if (craftingBuildingDefinition != null)
			{
				UIOffset = craftingBuildingDefinition.HarvestableIconOffset;
			}
			else if (villainLairEntranceBuildingDefinition != null)
			{
				UIOffset = villainLairEntranceBuildingDefinition.HarvestableIconOffset;
			}
			if (isBonus)
			{
				m_backingStroke.gameObject.SetActive(false);
				m_backing.sprite = UIUtils.LoadSpriteFromPath("img_BonusDoober_fill");
				m_backing.maskSprite = UIUtils.LoadSpriteFromPath("img_BonusDoober_mask");
				RectTransform rectTransform = m_image.transform as RectTransform;
				rectTransform.offsetMin = new Vector2(7f, -4f);
				rectTransform.offsetMax = new Vector2(-7f, 4f);
			}
		}

		private void UpdateBuildingView(string spriteName, string maskName)
		{
			HideText();
			m_image.color = Color.white;
			m_image.sprite = UIUtils.LoadSpriteFromPath(spriteName);
			m_image.maskSprite = UIUtils.LoadSpriteFromPath(maskName);
		}

		public void UpdateIconIndex(float iconIndex)
		{
			IconIndex = iconIndex;
			iconIndexOffset = new Vector3(1f * IconIndex, 0f, 0f);
		}

		internal void UpdateIconCount(int count)
		{
			string text;
			switch (count)
			{
			case 0:
				RemoveResourceIconSignal.Dispatch();
				return;
			case 1:
				text = "x 1";
				break;
			default:
				text = string.Format("x {0}", count);
				break;
			}
			m_text.text = text;
		}

		internal void HighlightHarvest(bool isHighlighted)
		{
			if (isHighlighted)
			{
				TweenUtil.Throb(m_image.transform, 0.85f, 0.5f, out localScale);
				return;
			}
			Go.killAllTweensWithTarget(m_image.transform);
			m_image.transform.localScale = localScale;
		}

		internal override void TargetObjectNullResponse()
		{
			logger.Warning("Removing Resource Icon with id: {0} since the target object does not exist anymore!", m_trackedId);
			RemoveResourceIconSignal.Dispatch();
		}

		public override Vector3 GetIndicatorPosition()
		{
			BuildingObject buildingObject = targetObject as BuildingObject;
			if (buildingObject != null)
			{
				return buildingObject.ResourceIconPosition;
			}
			CharacterObject characterObject = targetObject as CharacterObject;
			if (characterObject != null)
			{
				return characterObject.GetIndicatorPosition();
			}
			return Vector3.zero;
		}

		internal override void OnUpdatePosition(PositionData positionData)
		{
			m_transform.position = positionData.WorldPositionInUI + iconIndexOffset;
			m_transform.localPosition = VectorUtils.ZeroZ(m_transform.localPosition);
		}

		protected override void OnHide()
		{
			base.OnHide();
			HideText();
		}

		protected override void OnShow()
		{
			base.OnShow();
			ShowText();
		}

		private void ShowText()
		{
			if (leisureBuilding != null || mibBuilding != null)
			{
				m_text.enabled = false;
				m_textBackground.enabled = false;
				m_image.enabled = true;
			}
			else if (hasText)
			{
				m_text.enabled = true;
				m_image.enabled = true;
				m_textBackground.enabled = true;
			}
		}

		private void HideText()
		{
			m_text.enabled = false;
			m_image.enabled = false;
			m_textBackground.enabled = false;
		}

		public void Close()
		{
			Object.Destroy(base.gameObject);
		}

		public void Cleanup()
		{
			HighlightHarvest(false);
		}
	}
}
