using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.UI;
using Kampai.UI.View.UpSell;
using Kampai.Util;
using UnityEngine;

namespace Kampai.BuildingsSizeToolbox
{
	public class BuildingsSizeToolboxManagerView : KampaiView
	{
		public class UIPositionInfo
		{
			public Vector3 Position;

			public float Scale;

			public UIPositionInfo()
			{
			}

			public UIPositionInfo(Vector3 pos, float scale)
			{
				Position = pos;
				Scale = scale;
			}
		}

		public BuildingsSizeToolkitValueEditView XInputView;

		public BuildingsSizeToolkitValueEditView YInputView;

		public BuildingsSizeToolkitValueEditView ZInputView;

		public BuildingsSizeToolkitValueEditView SInputView;

		public IKampaiLogger logger = LogManager.GetClassLogger("BuildingsSizeToolboxManagerView") as IKampaiLogger;

		private UpSellModalView currentView;

		private UpSellItemView[] upsellItemViews;

		private BuildingDefinition currentBuildingDefinition;

		private Dictionary<int, UIPositionInfo> resetInfo = new Dictionary<int, UIPositionInfo>();

		private Dictionary<int, UIPositionInfo> buildingsInfo = new Dictionary<int, UIPositionInfo>();

		[Inject]
		public NewUpsellScreenSelectedSignal upsellScreenSelectedSignal { get; set; }

		[Inject]
		public BuildingSelectedSignal buildingSelectedSignal { get; set; }

		[Inject]
		public BuildingModifiedSignal buildingModifiedSignal { get; set; }

		[Inject]
		public BuildingsStateSavedSignal buildingStateAppliedSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public MoveAudioListenerSignal moveAudioListenerSignal { get; set; }

		protected override void Start()
		{
			base.Start();
			upsellScreenSelectedSignal.AddListener(onUpsellScreenSelectedSignal);
			buildingSelectedSignal.AddListener(onBuildingSelectedSignal);
			XInputView.ValueChangedSignal.AddListener(positionXChanged);
			YInputView.ValueChangedSignal.AddListener(positionYChanged);
			ZInputView.ValueChangedSignal.AddListener(positionZChanged);
			SInputView.ValueChangedSignal.AddListener(scaleChanged);
		}

		private void onUpsellScreenSelectedSignal(UpSellModalView view)
		{
			upsellItemViews = null;
			currentView = view;
			StartCoroutine(updateUpsellViewState());
		}

		private void setupValueFields(BuildingDefinition def)
		{
			Vector3 uiPosition = def.UiPosition;
			XInputView.CurrentValue = uiPosition.x;
			YInputView.CurrentValue = uiPosition.y;
			ZInputView.CurrentValue = uiPosition.z;
			SInputView.CurrentValue = def.UiScale;
		}

		private void onBuildingSelectedSignal(BuildingDefinition def)
		{
			if (def != currentBuildingDefinition)
			{
				currentBuildingDefinition = def;
				StartCoroutine(updateUpsellViewState());
				setupValueFields(def);
				if (!resetInfo.ContainsKey(def.ID))
				{
					resetInfo.Add(def.ID, new UIPositionInfo(def.UiPosition, def.UiScale));
				}
			}
		}

		private IEnumerator updateUpsellViewState(int framesToWait = 1)
		{
			for (int j = 0; j < framesToWait; j++)
			{
				yield return null;
			}
			if (currentBuildingDefinition != null && !(currentView == null))
			{
				if (upsellItemViews != null)
				{
					resetPreviousState();
				}
				Transform[] itemParents = currentView.itemTransforms;
				GameObject itemViewPrefab = currentView.itemViewPrefab;
				upsellItemViews = new UpSellItemView[itemParents.Length];
				for (int i = 0; i < itemParents.Length; i++)
				{
					UpSellItemView itemView = createItemView(itemViewPrefab, itemParents[i]);
					Rect firstItemTrasformRect = (itemParents[0].transform as RectTransform).rect;
					Rect parentTransformRect = (itemView.transform.parent.transform as RectTransform).rect;
					itemView.AdditionalUIScale = Mathf.Min(parentTransformRect.width / firstItemTrasformRect.width, parentTransformRect.height / firstItemTrasformRect.height);
					itemView.Item = new QuantityItem(currentBuildingDefinition.ID, 1u);
					itemView.Init(localizationService, fancyUIService, definitionService, logger, moveAudioListenerSignal);
					upsellItemViews[i] = itemView;
				}
			}
		}

		private void resetPreviousState()
		{
			UpSellItemView[] array = upsellItemViews;
			foreach (UpSellItemView upSellItemView in array)
			{
				Object.Destroy(upSellItemView.gameObject);
			}
			upsellItemViews = null;
		}

		private UpSellItemView createItemView(GameObject prefab, Transform parent)
		{
			GameObject gameObject = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
			if (gameObject == null)
			{
				logger.Error("Could not create UpSellItemView from prefab");
				return null;
			}
			UpSellItemView component = gameObject.GetComponent<UpSellItemView>();
			if (component == null)
			{
				logger.Error("Could not get UpSellItemView from prefab");
				return null;
			}
			component.transform.SetParent(parent);
			setUpItemTransform(component.transform as RectTransform);
			return component;
		}

		private void setUpItemTransform(RectTransform rect)
		{
			if (!(rect == null))
			{
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.one;
				rect.sizeDelta = Vector2.zero;
				rect.localScale = Vector3.one;
				rect.localPosition = Vector3.zero;
			}
		}

		private void positionXChanged(float v)
		{
			if (currentBuildingDefinition != null)
			{
				Vector3 uiPosition = currentBuildingDefinition.UiPosition;
				uiPosition.x = v;
				currentBuildingDefinition.UiPosition = uiPosition;
				updateBuilding();
			}
		}

		private void positionYChanged(float v)
		{
			if (currentBuildingDefinition != null)
			{
				Vector3 uiPosition = currentBuildingDefinition.UiPosition;
				uiPosition.y = v;
				currentBuildingDefinition.UiPosition = uiPosition;
				updateBuilding();
			}
		}

		private void positionZChanged(float v)
		{
			if (currentBuildingDefinition != null)
			{
				Vector3 uiPosition = currentBuildingDefinition.UiPosition;
				uiPosition.z = v;
				currentBuildingDefinition.UiPosition = uiPosition;
				updateBuilding();
			}
		}

		private void scaleChanged(float v)
		{
			if (currentBuildingDefinition != null)
			{
				currentBuildingDefinition.UiScale = v;
				updateBuilding();
			}
		}

		private void updateBuilding()
		{
			buildingModifiedSignal.Dispatch(currentBuildingDefinition);
			buildingsInfo[currentBuildingDefinition.ID] = new UIPositionInfo(currentBuildingDefinition.UiPosition, currentBuildingDefinition.UiScale);
			if (upsellItemViews != null)
			{
				UpSellItemView[] array = upsellItemViews;
				foreach (UpSellItemView upSellItemView in array)
				{
					Transform transform = upSellItemView.buildingSlot.transform;
					float additionalUIScale = upSellItemView.AdditionalUIScale;
					float num = currentBuildingDefinition.UiScale * additionalUIScale;
					transform.localScale = new Vector3(num, num, num);
					transform.localPosition = currentBuildingDefinition.UiPosition * additionalUIScale;
				}
			}
		}

		public void ResetState()
		{
			UIPositionInfo value;
			if (currentBuildingDefinition != null && resetInfo.TryGetValue(currentBuildingDefinition.ID, out value))
			{
				currentBuildingDefinition.UiPosition = value.Position;
				currentBuildingDefinition.UiScale = value.Scale;
				setupValueFields(currentBuildingDefinition);
				updateBuilding();
			}
		}

		public void SaveAll()
		{
			new DefinitionsUpdater().Update(buildingsInfo);
			buildingsInfo.Clear();
			buildingStateAppliedSignal.Dispatch();
		}
	}
}
