using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class BuildingPickController : Command
	{
		[Inject]
		public int pickEvent { get; set; }

		[Inject]
		public Vector3 inputPosition { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public SelectBuildingSignal selectBuildingSignal { get; set; }

		[Inject]
		public DeselectBuildingSignal deselectBuildingSignal { get; set; }

		[Inject]
		public RevealBuildingSignal revealBuildingSignal { get; set; }

		[Inject]
		public DeselectTaskedMinionsSignal deselectTaskedMinionsSignal { get; set; }

		[Inject]
		public DragAndDropPickSignal dragAndDropSignal { get; set; }

		[Inject]
		public StartMinionTaskSignal startMinionTaskSignal { get; set; }

		[Inject]
		public TryHarvestBuildingSignal tryHarvestSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject]
		public DeselectAllMinionsSignal deselectMinionsSignal { get; set; }

		[Inject]
		public SelectMinionSignal selectMinionSignal { get; set; }

		[Inject]
		public ShowNeedXMinionsSignal showNeedXMinionsSignal { get; set; }

		[Inject]
		public RepairBuildingSignal repairBuildingSignal { get; set; }

		[Inject]
		public ShowQuestPanelSignal showQuestPanel { get; set; }

		[Inject]
		public ShowQuestRewardSignal showQuestRewardSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public OpenBuildingMenuSignal openBuildingMenuSignal { get; set; }

		[Inject]
		public DisplayInaccessibleMessageSignal displayInaccessibleMessageSignal { get; set; }

		[Inject]
		public ClickedVillainLairGhostedComponentBuildingSignal clickedGhostComponentSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public ILocalizationService localeService { get; set; }

		[Inject]
		public ShowActivitySpinnerSignal showActivitySpinnerSignal { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		public override void Execute()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			if (minionPartyInstance.IsPartyHappening || uiModel.LevelUpUIOpen || uiModel.WelcomeBuddyOpen)
			{
				return;
			}
			switch (pickEvent)
			{
			case 2:
			{
				if (model.CurrentMode != PickControllerModel.Mode.Building || !(model.StartHitObject != null))
				{
					break;
				}
				BuildingObject component = model.StartHitObject.GetComponent<BuildingObject>();
				if (component == null)
				{
					break;
				}
				Building byInstanceId = playerService.GetByInstanceId<Building>(component.ID);
				if (byInstanceId == null)
				{
					break;
				}
				if (!model.DetectedMovement)
				{
					if (!model.activitySpinnerExists && byInstanceId.Definition.Movable)
					{
						model.activitySpinnerExists = true;
						showActivitySpinnerSignal.Dispatch(true, component.transform.position);
					}
				}
				else if (model.activitySpinnerExists)
				{
					model.activitySpinnerExists = false;
					showActivitySpinnerSignal.Dispatch(false, Vector3.zero);
				}
				BuildingState state = byInstanceId.State;
				if (state != BuildingState.Construction && state != 0 && state != BuildingState.Complete && !model.IsInstanceIgnored(byInstanceId.ID))
				{
					TrySelectBuilding(component, component.ID);
				}
				break;
			}
			case 3:
				PickEnd();
				break;
			}
		}

		private void PickEnd()
		{
			if (model.activitySpinnerExists)
			{
				showActivitySpinnerSignal.Dispatch(false, Vector3.zero);
				model.activitySpinnerExists = false;
			}
			if (!(model.EndHitObject != null) || !(model.StartHitObject == model.EndHitObject) || model.DetectedMovement)
			{
				return;
			}
			globalSFXSignal.Dispatch("Play_button_click_01");
			BuildingObject component = model.EndHitObject.GetComponent<BuildingObject>();
			if (component != null)
			{
				IScaffoldingPart scaffoldingPart = component as IScaffoldingPart;
				if (scaffoldingPart != null)
				{
					Building byInstanceId = playerService.GetByInstanceId<Building>(component.ID);
					revealBuildingSignal.Dispatch(byInstanceId);
				}
				else
				{
					PickEndBuilding(component);
				}
			}
		}

		private void PickEndBuilding(BuildingObject endHitObject)
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(endHitObject.ID);
			if (byInstanceId != null)
			{
				if (model.IsInstanceIgnored(byInstanceId.ID))
				{
					return;
				}
				CabanaBuilding cabanaBuilding = byInstanceId as CabanaBuilding;
				if (cabanaBuilding != null && cabanaBuilding.Quest != null)
				{
					OpenCabanaQuest(cabanaBuilding.Quest);
				}
				if (byInstanceId.State == BuildingState.Broken)
				{
					repairBuildingSignal.Dispatch(byInstanceId);
					return;
				}
				if (byInstanceId.State == BuildingState.Inaccessible)
				{
					displayInaccessibleMessageSignal.Dispatch(endHitObject, byInstanceId);
					return;
				}
				if (InputUtils.touchCount < 2)
				{
					openBuildingMenuSignal.Dispatch(endHitObject, byInstanceId);
				}
				TrySendMinions(endHitObject, byInstanceId);
				VillainLairEntranceBuilding villainLairEntranceBuilding = byInstanceId as VillainLairEntranceBuilding;
				if (villainLairEntranceBuilding == null)
				{
					tryHarvestSignal.Dispatch(endHitObject, delegate
					{
					}, false);
				}
				return;
			}
			MasterPlanComponentBuildingObject masterPlanComponentBuildingObject = endHitObject as MasterPlanComponentBuildingObject;
			if (masterPlanComponentBuildingObject != null)
			{
				clickedGhostComponentSignal.Dispatch(masterPlanComponentBuildingObject);
				return;
			}
			MignetteBuildingObject mignetteBuildingObject = endHitObject as MignetteBuildingObject;
			if (mignetteBuildingObject != null)
			{
				int id = mignetteBuildingObject.ID * -1;
				AspirationalBuildingDefinition aspirationalBuildingDefinition = definitionService.Get<AspirationalBuildingDefinition>(id);
				int buildingDefinitionID = aspirationalBuildingDefinition.BuildingDefinitionID;
				MignetteBuildingDefinition mignetteBuildingDefinition = definitionService.Get<MignetteBuildingDefinition>(buildingDefinitionID);
				int levelUnlocked = mignetteBuildingDefinition.LevelUnlocked;
				string aspirationalMessage = mignetteBuildingDefinition.AspirationalMessage;
				globalSFXSignal.Dispatch("Play_action_locked_01");
				popupMessageSignal.Dispatch(localeService.GetString(aspirationalMessage, levelUnlocked), PopupMessageType.NORMAL);
			}
		}

		private void OpenCabanaQuest(Quest q)
		{
			switch (q.state)
			{
			case QuestState.Notstarted:
			case QuestState.RunningStartScript:
			case QuestState.RunningTasks:
			case QuestState.RunningCompleteScript:
				showQuestPanel.Dispatch(q.ID);
				break;
			case QuestState.Harvestable:
				showQuestRewardSignal.Dispatch(q.ID);
				break;
			}
		}

		private void TrySelectBuilding(BuildingObject bo, int id)
		{
			if (model.SelectedBuilding.HasValue || model.DetectedMovement || !(model.HeldTimer >= 0.75f) || !(bo != null))
			{
				return;
			}
			Building byInstanceId = playerService.GetByInstanceId<Building>(bo.ID);
			if (byInstanceId == null)
			{
				return;
			}
			BuildingDefinition definition = byInstanceId.Definition;
			if (!definition.Movable)
			{
				return;
			}
			StorageBuilding storageBuilding = byInstanceId as StorageBuilding;
			if (storageBuilding != null && !storageBuilding.IsBuildingRepaired())
			{
				return;
			}
			Handheld.Vibrate();
			model.SelectedBuilding = id;
			model.CurrentMode = PickControllerModel.Mode.DragAndDrop;
			DragOffsetType type = DragOffsetType.NONE;
			if (definition.FootprintID == 300000)
			{
				type = DragOffsetType.ONE_X_ONE;
			}
			deselectMinionsSignal.Dispatch();
			dragAndDropSignal.Dispatch(1, inputPosition, type);
			BuildingDefinition definition2 = byInstanceId.Definition;
			if (byInstanceId != null && definition2.Movable)
			{
				selectBuildingSignal.Dispatch(id, definitionService.GetBuildingFootprint(definition2.FootprintID));
				deselectBuildingSignal.AddListener(DeselectBuilding);
				BuildingState state = byInstanceId.State;
				if (state == BuildingState.Working || state == BuildingState.Harvestable || state == BuildingState.HarvestableAndWorking || playerService.GetHighestFtueCompleted() < 6)
				{
					uiContext.injectionBinder.GetInstance<DisableMoveToInventorySignal>().Dispatch();
				}
			}
		}

		private void DeselectBuilding(int id)
		{
			if (model.SelectedBuilding == id)
			{
				model.SelectedBuilding = null;
				deselectBuildingSignal.RemoveListener(DeselectBuilding);
			}
		}

		private void TrySendMinions(BuildingObject buildingObj, Building building)
		{
			TaskableBuilding taskableBuilding = building as TaskableBuilding;
			if (model.SelectedMinions.Count == 0 || model.SelectedBuilding.HasValue || model.HeldTimer >= 0.75f || taskableBuilding == null || taskableBuilding.State == BuildingState.Harvestable || taskableBuilding.State == BuildingState.Cooldown || taskableBuilding is TikiBarBuilding)
			{
				return;
			}
			DebrisBuilding debrisBuilding = taskableBuilding as DebrisBuilding;
			if (debrisBuilding != null && !debrisBuilding.PaidInputCostToClear)
			{
				return;
			}
			MignetteBuilding mignetteBuilding = taskableBuilding as MignetteBuilding;
			if (mignetteBuilding != null && !TrySelectToFillTaskableBuilding(buildingObj, taskableBuilding))
			{
				showNeedXMinionsSignal.Dispatch(taskableBuilding.GetMinionSlotsOwned());
				return;
			}
			if (taskableBuilding.Definition.WorkStations > taskableBuilding.GetMinionsInBuilding())
			{
				globalSFXSignal.Dispatch("Play_minion_counter_down_01");
			}
			bool flag = true;
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			foreach (int key in model.SelectedMinions.Keys)
			{
				if (flag)
				{
					globalSFXSignal.Dispatch("Play_minion_confirm_pathToBldg_01");
					flag = false;
				}
				MinionObject second = component.Get(key);
				startMinionTaskSignal.Dispatch(new Tuple<int, MinionObject, int>(taskableBuilding.ID, second, timeService.CurrentTime()));
			}
			deselectTaskedMinionsSignal.Dispatch();
		}

		private bool TrySelectToFillTaskableBuilding(BuildingObject buildingObj, TaskableBuilding building)
		{
			int num = building.GetMinionSlotsOwned() - building.GetMinionsInBuilding();
			int count = model.SelectedMinions.Count;
			int num2 = num - count;
			if (num2 <= 0)
			{
				return true;
			}
			int num3 = playerService.GetMinionCount() - count;
			if (num2 > num3)
			{
				return false;
			}
			Vector3 center = buildingObj.Center;
			Queue<int> minionListSortedByDistanceAndState = minionManager.GetComponent<MinionManagerView>().GetMinionListSortedByDistanceAndState(inputPosition);
			Queue<int> queue = new Queue<int>();
			while (minionListSortedByDistanceAndState.Count > 0 && num2 > 0)
			{
				int num4 = minionListSortedByDistanceAndState.Dequeue();
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(num4);
				if (!model.SelectedMinions.ContainsKey(num4) && (byInstanceId.State == MinionState.Idle || byInstanceId.State == MinionState.Selectable || byInstanceId.State == MinionState.Leisure || byInstanceId.State == MinionState.Uninitialized))
				{
					queue.Enqueue(num4);
					num2--;
				}
			}
			Boxed<Vector3> param = new Boxed<Vector3>(center);
			bool flag = num2 == 0;
			if (flag)
			{
				while (queue.Count > 0)
				{
					selectMinionSignal.Dispatch(queue.Dequeue(), param, true);
				}
			}
			return flag;
		}
	}
}
