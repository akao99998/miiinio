using System;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class MasterPlanSelectComponentView : PopupMenuView
	{
		[Header("Header")]
		public LocalizeView titleText;

		[Header("Selection Buttons")]
		public ButtonView previousButtonView;

		public ButtonView nextButtonView;

		[Header("Content")]
		public Transform componentInfoPanelTransform;

		public Transform componentCompletePanelTransform;

		[Header("Action Button")]
		public ButtonView actionButtonView;

		public LocalizeView actionLocalizeView;

		[Header("Prefabs")]
		public GameObject componentInfoPrefab;

		[Header("Building")]
		public RectTransform BuildingSlot;

		internal Signal<int, Boxed<Action>> PanWithinLairSignal = new Signal<int, Boxed<Action>>();

		private IDefinitionService definitionService;

		private IPlayerService playerService;

		private IGhostComponentService ghostComponentService;

		internal readonly IList<MasterPlanComponentDefinition> componentDefinitions = new List<MasterPlanComponentDefinition>();

		internal List<int> componentCameraPositions = new List<int>();

		private IGUIService guiService;

		internal readonly Signal<Type, int> updateSubViewSignal = new Signal<Type, int>();

		private IMasterPlanService masterPlanService;

		internal MasterPlanDefinition planDefinition { get; private set; }

		internal bool willRelease { get; private set; }

		internal int selectedIndex { get; private set; }

		internal void Init(int definitionID, int componentIDfromPlatform, IPlayerService playerService, IDefinitionService defService, IGUIService guiService, IGhostComponentService ghostService, IMasterPlanService masterPlanService)
		{
			Init();
			this.playerService = playerService;
			this.masterPlanService = masterPlanService;
			definitionService = defService;
			ghostComponentService = ghostService;
			ghostComponentService.ClearGhostComponentBuildings();
			this.guiService = guiService;
			planDefinition = defService.Get<MasterPlanDefinition>(definitionID);
			SetupComponentDefinitions();
			SetComponentIndexFromId(componentIDfromPlatform);
			Open();
		}

		private void SetupComponentDefinitions()
		{
			VillainLairDefinition villainLairDefinition = definitionService.Get<VillainLairDefinition>(StaticItem.VILLAIN_LAIR_DEFINITION_ID);
			componentDefinitions.Clear();
			for (int i = 0; i < planDefinition.ComponentDefinitionIDs.Count; i++)
			{
				int id = planDefinition.ComponentDefinitionIDs[i];
				MasterPlanComponentDefinition item = definitionService.Get<MasterPlanComponentDefinition>(id);
				componentDefinitions.Add(item);
				componentCameraPositions.Add(villainLairDefinition.Platforms[i].customCameraPosID);
			}
		}

		internal void SetComponentIndexFromId(int id)
		{
			int componentIndexFromId = GetComponentIndexFromId(id);
			SelectComponent((componentIndexFromId < componentDefinitions.Count) ? componentIndexFromId : 0);
		}

		private int GetComponentIndexFromId(int id)
		{
			int i;
			for (i = 0; i < componentDefinitions.Count && id != GetMasterplanComponentDefId(i); i++)
			{
			}
			return i;
		}

		internal void SelectComponent(int index)
		{
			selectedIndex = WrapIndex(index, 0, componentDefinitions.Count - 1);
			MasterPlanComponent masterPlanComponent = GetMasterPlanComponent(selectedIndex);
			bool flag = masterPlanComponent != null && (masterPlanComponent.State == MasterPlanComponentState.Complete || masterPlanComponent.State == MasterPlanComponentState.Scaffolding || masterPlanComponent.State == MasterPlanComponentState.Built);
			titleText.LocKey = GetMasterplanComponentBuildingDefLocKey(selectedIndex);
			ToggleComponentInfo(!flag);
			ToggleCompletePanel(flag);
			DisplayBuilding(true);
		}

		internal void NextComponent()
		{
			ghostComponentService.ClearGhostComponentBuildings();
			SelectComponent(++selectedIndex);
		}

		internal void PreviousComponent()
		{
			ghostComponentService.ClearGhostComponentBuildings();
			SelectComponent(--selectedIndex);
		}

		private int WrapIndex(int index, int min, int max)
		{
			return (index < min) ? max : ((index <= max) ? index : min);
		}

		private void ToggleComponentInfo(bool show)
		{
			componentInfoPanelTransform.gameObject.SetActive(show);
			actionButtonView.gameObject.SetActive(show);
			if (!show)
			{
				return;
			}
			MasterPlanComponent activeComponentFromPlanDefinition = masterPlanService.GetActiveComponentFromPlanDefinition(planDefinition.ID);
			if (activeComponentFromPlanDefinition != null)
			{
				actionLocalizeView.LocKey = "MasterPlanTaskSelection";
				actionButtonView.gameObject.SetActive(selectedIndex == GetComponentIndexFromId(activeComponentFromPlanDefinition.Definition.ID));
			}
			else
			{
				actionLocalizeView.LocKey = "MasterPlanSelect";
			}
			MasterPlanComponentInfoView.ItemType[] array = new MasterPlanComponentInfoView.ItemType[2]
			{
				MasterPlanComponentInfoView.ItemType.Requires,
				MasterPlanComponentInfoView.ItemType.Rewards
			};
			if (componentInfoPanelTransform.childCount != array.Length)
			{
				for (int i = 0; i < array.Length; i++)
				{
					IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "cmp_masterplanComponentInfo");
					GUIArguments args = iGUICommand.Args;
					args.Add(typeof(MasterPlanDefinition), planDefinition);
					args.Add(typeof(MasterPlanComponentInfoView.ItemType), array[i]);
					args.Add(typeof(int), selectedIndex);
					GameObject gameObject = guiService.Execute(iGUICommand);
					if (!(gameObject == null))
					{
						gameObject.transform.SetParent(componentInfoPanelTransform, false);
					}
				}
			}
			else
			{
				updateSubViewSignal.Dispatch(typeof(MasterPlanComponentInfoView), selectedIndex);
			}
		}

		internal void ReleaseViews()
		{
			ghostComponentService.ClearGhostComponentBuildings();
		}

		private void ToggleCompletePanel(bool show)
		{
			componentCompletePanelTransform.gameObject.SetActive(show);
		}

		private void CreateBuilding(bool isRegularComponent)
		{
			int componentID = ((!isRegularComponent) ? planDefinition.BuildingDefID : componentDefinitions[selectedIndex].ID);
			ghostComponentService.DisplayZoomedInComponent(componentID, isRegularComponent);
		}

		public override void Close(bool instant = false)
		{
			willRelease = true;
			base.Close(instant);
		}

		private void DisplayBuilding(bool isRegularComponent)
		{
			if (isRegularComponent)
			{
				PanToComponentLocation();
			}
			else
			{
				PanToMasterPlanLocation();
			}
		}

		private void PanToComponentLocation()
		{
			PanWithinLairSignal.Dispatch(componentCameraPositions[selectedIndex], new Boxed<Action>(PanToComponentComplete));
		}

		private void PanToMasterPlanLocation()
		{
			PanWithinLairSignal.Dispatch(planDefinition.BuildingCustomCameraPosID, new Boxed<Action>(PanToMasterPlanBuildingComplete));
		}

		internal void PanToMainLairView()
		{
			PanWithinLairSignal.Dispatch(60017, new Boxed<Action>(null));
		}

		private void PanToComponentComplete()
		{
			if (selectedIndex >= 0 && selectedIndex < componentDefinitions.Count && !willRelease)
			{
				CreateBuilding(true);
			}
		}

		private void PanToMasterPlanBuildingComplete()
		{
			if (!willRelease)
			{
				CreateBuilding(false);
			}
		}

		internal MasterPlanComponentDefinition GetMasterplanComponentDef(int componentIndex)
		{
			return (componentIndex >= 0 && componentIndex < componentDefinitions.Count) ? componentDefinitions[componentIndex] : null;
		}

		internal int GetMasterplanComponentDefId(int componentIndex)
		{
			MasterPlanComponentDefinition masterplanComponentDef = GetMasterplanComponentDef(componentIndex);
			return (masterplanComponentDef != null) ? masterplanComponentDef.ID : (-1);
		}

		internal string GetMasterplanComponentBuildingDefLocKey(int componentIndex)
		{
			MasterPlanComponent masterPlanComponent = GetMasterPlanComponent(componentIndex);
			if (masterPlanComponent != null)
			{
				MasterPlanComponentBuildingDefinition definition = null;
				if (definitionService.TryGet<MasterPlanComponentBuildingDefinition>(masterPlanComponent.buildingDefID, out definition))
				{
					return definition.LocalizedKey;
				}
			}
			return (masterPlanComponent != null) ? componentDefinitions[componentIndex].LocalizedKey : "MISSING LOC KEY";
		}

		internal MasterPlanComponent GetMasterPlanComponent(int index)
		{
			MasterPlanComponentDefinition masterplanComponentDef = GetMasterplanComponentDef(index);
			return (masterplanComponentDef != null) ? playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(masterplanComponentDef.ID) : null;
		}
	}
}
