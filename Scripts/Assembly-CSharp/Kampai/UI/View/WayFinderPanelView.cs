using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class WayFinderPanelView : KampaiView
	{
		private delegate bool PriorityFunction(IWayFinderView wayFinderView);

		private Dictionary<int, IWayFinderView> trackedWayFinders;

		private Dictionary<int, IParentWayFinderView> trackedParentWayFinders;

		private IKampaiLogger logger;

		private ITikiBarService tikiBarService;

		private IPlayerService playerService;

		private IPrestigeService prestigeService;

		private WayFinderSettings tikiBarParentWayFinderSettings;

		private WayFinderSettings cabanaParentWayFinderSettings;

		private WayFinderSettings orderBoardWayFinderSettings;

		private WayFinderSettings storageBuildingWayFinderSettings;

		private WayFinderSettings stageBuildingWayFinderSettings;

		private int specialEventCharacterId;

		private int tsmWayFinderTrackedId;

		private bool isForceHideEnabled;

		private PickControllerModel pickControllerModel;

		private List<List<PriorityFunction>> allPriorityFunctions;

		internal void Init(IKampaiLogger logger, ITikiBarService tikiBarService, IPlayerService playerService, IPrestigeService prestigeService, IPositionService positionService, PickControllerModel pickControllerModel)
		{
			this.logger = logger;
			this.tikiBarService = tikiBarService;
			this.playerService = playerService;
			this.prestigeService = prestigeService;
			this.pickControllerModel = pickControllerModel;
			tikiBarParentWayFinderSettings = new WayFinderSettings(313);
			cabanaParentWayFinderSettings = new WayFinderSettings(1000008087);
			orderBoardWayFinderSettings = new WayFinderSettings(309);
			storageBuildingWayFinderSettings = new WayFinderSettings(314);
			stageBuildingWayFinderSettings = new WayFinderSettings(370);
			SpecialEventCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<SpecialEventCharacter>(70009);
			if (firstInstanceByDefinitionId != null)
			{
				specialEventCharacterId = firstInstanceByDefinitionId.ID;
			}
			tsmWayFinderTrackedId = 301;
			trackedWayFinders = new Dictionary<int, IWayFinderView>();
			trackedParentWayFinders = new Dictionary<int, IParentWayFinderView>();
			allPriorityFunctions = new List<List<PriorityFunction>>();
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeKevinWayfinder });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeVillainEntrance });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeSpecialEvent });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeQuestComplete, PrioritizeAtTikiBar });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeQuestComplete, PrioritizeAtCabana });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeNewQuest, PrioritizeAtTikiBar });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeNewQuest, PrioritizeAtCabana });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeTaskComplete, PrioritizeAtTikiBar });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeTaskComplete, PrioritizeAtCabana });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeBob });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeQuestAvailable, PrioritizeAtTikiBar });
			allPriorityFunctions.Add(new List<PriorityFunction> { PrioritizeQuestAvailable, PrioritizeAtCabana });
			UpdateHUDSnap(positionService);
			UpdateWayFinderPriority();
		}

		internal void UpdateHUDSnap(IPositionService positionService)
		{
			List<GameObject> list = GenerateListOfObjectsToSnapAround();
			foreach (GameObject item in list)
			{
				positionService.AddHUDElementToAvoid(item);
			}
		}

		private List<GameObject> GenerateListOfObjectsToSnapAround()
		{
			List<GameObject> list = new List<GameObject>();
			list.Add(GameObject.Find("btn_OpenStore"));
			list.Add(GameObject.Find("btn_Settings"));
			list.Add(GameObject.Find("group_Storage"));
			list.Add(GameObject.Find("group_Currency_Grind"));
			list.Add(GameObject.Find("group_Shopping"));
			GameObject gameObject = GameObject.Find("sale_snapTarget");
			if (gameObject != null)
			{
				list.Add(gameObject);
			}
			return list;
		}

		private bool PrioritizeBob(IWayFinderView wayFinderView)
		{
			return IsBobPointsAtStuffWayFinder(wayFinderView.Prestige);
		}

		private bool PrioritizeAtCabana(IWayFinderView wayFinderView)
		{
			return IsCabanaChildWayFinder(wayFinderView.Prestige);
		}

		private bool PrioritizeAtTikiBar(IWayFinderView wayFinderView)
		{
			return IsTikiBarChildWayFinder(wayFinderView.Prestige);
		}

		private bool PrioritizeVillainEntrance(IWayFinderView wayFinderView)
		{
			return wayFinderView.TrackedId == 374;
		}

		private bool PrioritizeKevinWayfinder(IWayFinderView wayFinderView)
		{
			int num = 0;
			KevinCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<KevinCharacter>(70003);
			if (firstInstanceByDefinitionId != null)
			{
				num = firstInstanceByDefinitionId.ID;
			}
			if (wayFinderView.TrackedId != num)
			{
				return false;
			}
			return true;
		}

		private bool PrioritizeSpecialEvent(IWayFinderView wayFinderView)
		{
			return wayFinderView as SpecialEventWayFinderView != null;
		}

		private bool PrioritizeQuestComplete(IWayFinderView wayFinderView)
		{
			AbstractQuestWayFinderView abstractQuestWayFinderView = wayFinderView as AbstractQuestWayFinderView;
			if (abstractQuestWayFinderView != null)
			{
				return abstractQuestWayFinderView.IsQuestComplete();
			}
			return false;
		}

		private bool PrioritizeQuestAvailable(IWayFinderView wayFinderView)
		{
			AbstractQuestWayFinderView abstractQuestWayFinderView = wayFinderView as AbstractQuestWayFinderView;
			if (abstractQuestWayFinderView != null)
			{
				return abstractQuestWayFinderView.IsQuestAvailable();
			}
			return false;
		}

		private bool PrioritizeNewQuest(IWayFinderView wayFinderView)
		{
			AbstractQuestWayFinderView abstractQuestWayFinderView = wayFinderView as AbstractQuestWayFinderView;
			if (abstractQuestWayFinderView != null)
			{
				return abstractQuestWayFinderView.IsNewQuestAvailable();
			}
			return false;
		}

		private bool PrioritizeTaskComplete(IWayFinderView wayFinderView)
		{
			AbstractQuestWayFinderView abstractQuestWayFinderView = wayFinderView as AbstractQuestWayFinderView;
			if (abstractQuestWayFinderView != null)
			{
				return abstractQuestWayFinderView.IsTaskReady();
			}
			return false;
		}

		internal void Cleanup()
		{
			if (trackedWayFinders != null)
			{
				trackedWayFinders.Clear();
			}
			if (trackedParentWayFinders != null)
			{
				trackedParentWayFinders.Clear();
			}
			if (allPriorityFunctions != null)
			{
				allPriorityFunctions.Clear();
			}
		}

		private bool IsTikiBarParentWayFinder(int trackedId)
		{
			return trackedId == tikiBarParentWayFinderSettings.TrackedId;
		}

		private bool IsTikiBarChildWayFinder(Prestige prestige)
		{
			return prestige != null && tikiBarService.IsCharacterSitting(prestige);
		}

		private bool IsOrderBoardWayFinder(int trackedId)
		{
			return trackedId == orderBoardWayFinderSettings.TrackedId;
		}

		private bool IsCabanaParentWayFinder(int trackedId)
		{
			return trackedId == cabanaParentWayFinderSettings.TrackedId;
		}

		private bool IsStorageBuildingWayFinder(int trackedId)
		{
			return trackedId == storageBuildingWayFinderSettings.TrackedId;
		}

		private bool IsStageBuildingWayFinder(int trackedId)
		{
			return trackedId == stageBuildingWayFinderSettings.TrackedId;
		}

		private bool IsTSMWayFinder(int trackedId)
		{
			return trackedId == tsmWayFinderTrackedId;
		}

		private bool IsMoveBuildingWayFinder(int trackedId)
		{
			if (trackedId == -1)
			{
				return true;
			}
			Building byInstanceId = playerService.GetByInstanceId<Building>(trackedId);
			if (byInstanceId != null && (byInstanceId is BridgeBuilding || byInstanceId is FountainBuilding || byInstanceId is StageBuilding || byInstanceId is WelcomeHutBuilding || byInstanceId.State == BuildingState.Complete))
			{
				return false;
			}
			return byInstanceId != null && pickControllerModel.CurrentMode == PickControllerModel.Mode.DragAndDrop;
		}

		private bool IsLairEntranceWayfinder(int trackedId)
		{
			if (trackedId != 374)
			{
				return false;
			}
			VillainLairEntranceBuilding byInstanceId = playerService.GetByInstanceId<VillainLairEntranceBuilding>(374);
			return byInstanceId.State != BuildingState.Inaccessible;
		}

		private bool IsCabanaChildWayFinder(Prestige prestige)
		{
			if (prestige != null)
			{
				PrestigeDefinition definition = prestige.Definition;
				return definition.Type == PrestigeType.Villain && definition.ID != 40001;
			}
			return false;
		}

		private bool IsBobPointsAtStuffWayFinder(Prestige prestige)
		{
			if (prestige != null)
			{
				return prestige.Definition.ID == 40002;
			}
			return false;
		}

		private bool IsFluxLairWayFinder(Prestige prestige)
		{
			if (prestige != null)
			{
				return prestige.Definition.ID == 40001;
			}
			return false;
		}

		private bool IsSpecialEventMinionWayFinder(int trackedId)
		{
			return specialEventCharacterId > 0 && trackedId == specialEventCharacterId;
		}

		private bool IsMignetteWayFinder(int trackedId)
		{
			return playerService.GetByInstanceId<MignetteBuilding>(trackedId) != null;
		}

		internal IWayFinderView CreateWayFinder(WayFinderSettings settings, bool updatePriority = true)
		{
			int trackedId = settings.TrackedId;
			IWayFinderView wayFinderView = null;
			if ((wayFinderView = GetWayFinder(trackedId)) != null)
			{
				return wayFinderView;
			}
			wayFinderView = SetupWayFinder(settings);
			trackedWayFinders[trackedId] = wayFinderView;
			PostCreateWayFinder(wayFinderView, updatePriority);
			return wayFinderView;
		}

		private IWayFinderView SetupWayFinder(WayFinderSettings settings)
		{
			int trackedId = settings.TrackedId;
			bool isQuest = settings.QuestDefId > 0;
			Prestige prestigeForWayFinder = GetPrestigeForWayFinder(trackedId);
			bool flag = IsCabanaChildWayFinder(prestigeForWayFinder);
			bool flag2 = IsTikiBarChildWayFinder(prestigeForWayFinder);
			IParentWayFinderView parentWayFinderView = null;
			if (flag)
			{
				parentWayFinderView = CreateWayFinder(cabanaParentWayFinderSettings) as IParentWayFinderView;
			}
			else if (flag2)
			{
				parentWayFinderView = CreateWayFinder(tikiBarParentWayFinderSettings) as IParentWayFinderView;
			}
			GameObject gameObject = Object.Instantiate(KampaiResources.Load("cmp_WayFinder")) as GameObject;
			gameObject.transform.SetParent(base.transform, false);
			gameObject.SetActive(true);
			WayFinderModal component = gameObject.GetComponent<WayFinderModal>();
			component.Settings = settings;
			component.Prestige = prestigeForWayFinder;
			return AddWayFinderViewComponent(gameObject, trackedId, prestigeForWayFinder, parentWayFinderView, isQuest, flag, flag2);
		}

		private IWayFinderView AddWayFinderViewComponent(GameObject wayFinderGO, int trackedId, Prestige prestige, IParentWayFinderView parentWayFinderView, bool isQuest, bool isCabanaChildWayFinder, bool isTikiBarChildWayFinder)
		{
			IWayFinderView wayFinderView = null;
			if (IsCabanaParentWayFinder(trackedId))
			{
				wayFinderView = wayFinderGO.AddComponent<CabanaParentWayFinderView>();
				trackedParentWayFinders.Add(trackedId, wayFinderView as CabanaParentWayFinderView);
			}
			else if (isCabanaChildWayFinder)
			{
				wayFinderView = wayFinderGO.AddComponent<CabanaChildWayFinderView>();
				CabanaChildWayFinderView childWayFinderView = wayFinderView as CabanaChildWayFinderView;
				parentWayFinderView.AddChildWayFinder(childWayFinderView);
			}
			else if (IsTikiBarParentWayFinder(trackedId))
			{
				wayFinderView = wayFinderGO.AddComponent<TikiBarParentWayFinderView>();
				trackedParentWayFinders.Add(trackedId, wayFinderView as TikiBarParentWayFinderView);
			}
			else if (isTikiBarChildWayFinder)
			{
				wayFinderView = wayFinderGO.AddComponent<TikiBarChildWayFinderView>();
				TikiBarChildWayFinderView childWayFinderView2 = wayFinderView as TikiBarChildWayFinderView;
				parentWayFinderView.AddChildWayFinder(childWayFinderView2);
			}
			else if (IsStageBuildingWayFinder(trackedId))
			{
				wayFinderView = wayFinderGO.AddComponent<StageBuildingWayFinderView>();
			}
			else if (IsOrderBoardWayFinder(trackedId))
			{
				wayFinderView = wayFinderGO.AddComponent<OrderBoardWayFinderView>();
			}
			else if (IsLairEntranceWayfinder(trackedId))
			{
				wayFinderView = wayFinderGO.AddComponent<LairEntranceWayfinderView>();
			}
			else if (IsMignetteWayFinder(trackedId))
			{
				wayFinderView = wayFinderGO.AddComponent<MignetteWayFinderView>();
			}
			else if (isQuest)
			{
				wayFinderView = (IsTSMWayFinder(trackedId) ? wayFinderGO.AddComponent<TSMWayFinderView>() : ((!IsSpecialEventMinionWayFinder(trackedId)) ? ((AbstractQuestWayFinderView)wayFinderGO.AddComponent<QuestWayFinderView>()) : ((AbstractQuestWayFinderView)wayFinderGO.AddComponent<SpecialEventWayFinderView>())));
			}
			else if (IsBobPointsAtStuffWayFinder(prestige))
			{
				wayFinderView = wayFinderGO.AddComponent<BobPointsAtStuffWayFinderView>();
			}
			else if (IsFluxLairWayFinder(prestige))
			{
				wayFinderView = wayFinderGO.AddComponent<VolcanoLairWayfinderView>();
			}
			else if (IsStorageBuildingWayFinder(trackedId))
			{
				wayFinderView = wayFinderGO.AddComponent<StorageBuildingWayFinderView>();
			}
			else if (!IsTSMWayFinder(trackedId))
			{
				wayFinderView = ((!IsMoveBuildingWayFinder(trackedId)) ? ((AbstractWayFinderView)wayFinderGO.AddComponent<WayFinderView>()) : ((AbstractWayFinderView)wayFinderGO.AddComponent<MoveBuildingWayFinderView>()));
			}
			else
			{
				logger.Info("Way finder with tracking id: {0} is on tsm for a trigger", trackedId);
				wayFinderView = wayFinderGO.AddComponent<TSMTriggerWayFinderView>();
			}
			return wayFinderView;
		}

		private void PostCreateWayFinder(IWayFinderView wayFinderView, bool updatePriority)
		{
			if (playerService.GetHighestFtueCompleted() < 3 && trackedWayFinders.Count > 2)
			{
				ForceHideTikiBarWayFinders(true);
			}
			wayFinderView.SetForceHide(isForceHideEnabled);
			if (updatePriority)
			{
				UpdateWayFinderPriority();
			}
		}

		private Prestige GetPrestigeForWayFinder(int trackedId)
		{
			Character byInstanceId = playerService.GetByInstanceId<Character>(trackedId);
			if (byInstanceId != null)
			{
				return tikiBarService.GetPrestigeForSeatableCharacter(byInstanceId);
			}
			CabanaBuilding byInstanceId2 = playerService.GetByInstanceId<CabanaBuilding>(trackedId);
			if (byInstanceId2 != null)
			{
				List<Villain> instancesByType = playerService.GetInstancesByType<Villain>();
				foreach (Villain item in instancesByType)
				{
					if (item.CabanaBuildingId == byInstanceId2.ID)
					{
						return prestigeService.GetPrestigeFromMinionInstance(item);
					}
				}
			}
			return null;
		}

		internal void RemoveWayFinder(int trackedId, bool updatePriority = true)
		{
			IWayFinderView wayFinderView = null;
			if ((wayFinderView = GetWayFinder(trackedId)) == null)
			{
				return;
			}
			trackedWayFinders.Remove(trackedId);
			IChildWayFinderView childWayFinderView = wayFinderView as IChildWayFinderView;
			if (childWayFinderView != null && childWayFinderView.ParentWayFinderTrackedId > 0)
			{
				IParentWayFinderView parentWayFinderView = GetWayFinder(childWayFinderView.ParentWayFinderTrackedId) as IParentWayFinderView;
				if (parentWayFinderView != null)
				{
					parentWayFinderView.RemoveChildWayFinder(trackedId);
					Dictionary<int, IChildWayFinderView> childrenWayFinders = parentWayFinderView.ChildrenWayFinders;
					if (childrenWayFinders != null && childrenWayFinders.Count == 0)
					{
						RemoveWayFinder(parentWayFinderView.TrackedId, false);
					}
				}
			}
			else
			{
				IParentWayFinderView parentWayFinderView2 = wayFinderView as IParentWayFinderView;
				if (parentWayFinderView2 != null)
				{
					Dictionary<int, IChildWayFinderView> childrenWayFinders2 = parentWayFinderView2.ChildrenWayFinders;
					if (childrenWayFinders2 != null && childrenWayFinders2.Count > 0)
					{
						foreach (IChildWayFinderView value in childrenWayFinders2.Values)
						{
							RemoveWayFinder(value.TrackedId, false);
						}
						return;
					}
					trackedParentWayFinders.Remove(trackedId);
				}
			}
			Object.Destroy(wayFinderView.GameObject);
			if (updatePriority)
			{
				UpdateWayFinderPriority();
			}
		}

		internal IWayFinderView GetWayFinder(int trackedId)
		{
			if (trackedWayFinders != null && trackedWayFinders.ContainsKey(trackedId))
			{
				return trackedWayFinders[trackedId];
			}
			return null;
		}

		internal void AddQuestToExistingWayFinder(int questDefId, int trackedId)
		{
			IWayFinderView wayFinder = GetWayFinder(trackedId);
			if (wayFinder != null)
			{
				IQuestWayFinderView questWayFinderView = wayFinder as IQuestWayFinderView;
				if (questWayFinderView != null)
				{
					questWayFinderView.AddQuest(questDefId);
				}
			}
		}

		internal void RemoveQuestFromExistingWayFinder(int questDefId, int trackedId)
		{
			IWayFinderView wayFinder = GetWayFinder(trackedId);
			if (wayFinder != null)
			{
				IQuestWayFinderView questWayFinderView = wayFinder as IQuestWayFinderView;
				if (questWayFinderView != null)
				{
					questWayFinderView.RemoveQuest(questDefId);
				}
			}
		}

		private void SetForceHideForAllWayFinders()
		{
			foreach (IWayFinderView value in trackedWayFinders.Values)
			{
				value.SetForceHide(isForceHideEnabled);
			}
		}

		internal void HideAllWayFinders()
		{
			isForceHideEnabled = true;
			SetForceHideForAllWayFinders();
		}

		internal void ShowAllWayFinders()
		{
			isForceHideEnabled = false;
			SetForceHideForAllWayFinders();
		}

		internal void SetLimitTikiBarWayFinders(bool limitTikiBarWayFinders)
		{
			ForceHideTikiBarWayFinders(limitTikiBarWayFinders);
		}

		internal void UpdateWayFinderPriority()
		{
			foreach (IParentWayFinderView value in trackedParentWayFinders.Values)
			{
				value.UpdateWayFinderIcon();
			}
			if (playerService.GetHighestFtueCompleted() < 9)
			{
				return;
			}
			IWayFinderView prioritizedWayFinder = GetPrioritizedWayFinder();
			if (prioritizedWayFinder != null)
			{
				SetWayFinderSnappable(prioritizedWayFinder);
				if (prioritizedWayFinder.TrackedId != orderBoardWayFinderSettings.TrackedId && GetWayFinder(orderBoardWayFinderSettings.TrackedId) != null)
				{
					RemoveWayFinder(orderBoardWayFinderSettings.TrackedId, false);
				}
			}
			else
			{
				IWayFinderView wayFinder = GetWayFinder(orderBoardWayFinderSettings.TrackedId);
				if (wayFinder == null)
				{
					wayFinder = CreateWayFinder(orderBoardWayFinderSettings, false);
				}
			}
		}

		private IWayFinderView GetPrioritizedWayFinder()
		{
			int count = trackedWayFinders.Count;
			if (count <= 0)
			{
				return null;
			}
			foreach (List<PriorityFunction> allPriorityFunction in allPriorityFunctions)
			{
				foreach (IWayFinderView value in trackedWayFinders.Values)
				{
					if (value.TrackedId == 301 || !PassesPriorityFunctions(value, allPriorityFunction))
					{
						continue;
					}
					return value;
				}
			}
			return null;
		}

		private bool PassesPriorityFunctions(IWayFinderView wayFinderView, List<PriorityFunction> priorityFunctions)
		{
			foreach (PriorityFunction priorityFunction in priorityFunctions)
			{
				if (!priorityFunction(wayFinderView))
				{
					return false;
				}
			}
			return true;
		}

		private void SetWayFinderSnappable(IWayFinderView snappableWayFinderView)
		{
			foreach (IWayFinderView value in trackedWayFinders.Values)
			{
				value.Snappable = false;
			}
			IChildWayFinderView childWayFinderView = snappableWayFinderView as IChildWayFinderView;
			if (childWayFinderView != null)
			{
				int parentWayFinderTrackedId = childWayFinderView.ParentWayFinderTrackedId;
				IParentWayFinderView parentWayFinderView = GetWayFinder(parentWayFinderTrackedId) as IParentWayFinderView;
				if (parentWayFinderTrackedId == 313)
				{
					snappableWayFinderView.Snappable = true;
					return;
				}
				if (parentWayFinderView != null)
				{
					parentWayFinderView.Snappable = true;
					return;
				}
				logger.Warning("Parent way finder with tracked id: {0} does not exist, child with id:{1} has no parent!", parentWayFinderTrackedId, childWayFinderView.TrackedId);
			}
			else
			{
				snappableWayFinderView.Snappable = true;
			}
		}

		private void ForceHideTikiBarWayFinders(bool hide)
		{
			IWayFinderView wayFinder = GetWayFinder(78);
			IWayFinderView wayFinder2 = GetWayFinder(313);
			if (wayFinder != null && wayFinder2 != null)
			{
				wayFinder.SetForceHide(hide);
				wayFinder2.SetForceHide(hide);
			}
		}
	}
}
