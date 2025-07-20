using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game
{
	public class PrestigeService : IPrestigeService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PrestigeService") as IKampaiLogger;

		private Dictionary<int, Prestige> prestigeMap = new Dictionary<int, Prestige>();

		private Dictionary<PrestigeType, UpdatePrestigeSignal> updateTable = new Dictionary<PrestigeType, UpdatePrestigeSignal>();

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IOrderBoardService orderBoardService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject namedCharacterManager { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public UpdateNamedCharacterPrestigeSignal updateNamedCharacterSignal { get; set; }

		[Inject]
		public UpdateVillainPrestigeSignal updateVillainSignal { get; set; }

		[Inject]
		public MinionPrestigeCompleteSignal minionPrestigeCompleteSignal { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			updateTable[PrestigeType.Minion] = updateNamedCharacterSignal;
			updateTable[PrestigeType.Villain] = updateVillainSignal;
		}

		public void Initialize()
		{
			GeneratePrestigeMap();
		}

		public Prestige GetPrestige(int prestigeDefinitionId, bool logIfNonexistant = true)
		{
			if (prestigeMap.ContainsKey(prestigeDefinitionId))
			{
				return prestigeMap[prestigeDefinitionId];
			}
			if (logIfNonexistant && prestigeDefinitionId != 0)
			{
				logger.Info("Prestige doesn't exist for the prestige definition Id: {0}", prestigeDefinitionId);
			}
			return null;
		}

		public Prestige CreatePrestige(int prestigeDefinitionId)
		{
			Prestige prestige = GetPrestige(prestigeDefinitionId, false);
			if (prestige == null)
			{
				PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(prestigeDefinitionId);
				if (prestigeDefinition == null)
				{
					return null;
				}
				prestige = new Prestige(prestigeDefinition);
				AddPrestige(prestige);
			}
			return prestige;
		}

		public IList<Prestige> GetBuddyPrestiges()
		{
			IList<Prestige> list = new List<Prestige>();
			foreach (KeyValuePair<int, Prestige> item in prestigeMap)
			{
				Prestige value = item.Value;
				if (value.CurrentPrestigePoints > 0 && (value.state == PrestigeState.PreUnlocked || value.state == PrestigeState.Prestige))
				{
					list.Add(value);
				}
			}
			return list;
		}

		public int GetIdlePrestigeDuration(int prestigeDefinitionId)
		{
			if (prestigeMap.ContainsKey(prestigeDefinitionId))
			{
				Prestige prestige = prestigeMap[prestigeDefinitionId];
				if (prestige.state == PrestigeState.Prestige)
				{
					return playerDurationService.GetGameTimeDuration(prestige);
				}
			}
			return 0;
		}

		public Dictionary<int, bool> GetPrestigedCharacterStates(bool includeBob = true)
		{
			Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
			IList<PrestigeDefinition> all = definitionService.GetAll<PrestigeDefinition>();
			foreach (PrestigeDefinition item in all)
			{
				int iD = item.ID;
				if (iD != 40014 && (includeBob || iD != 40002))
				{
					Prestige firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Prestige>(iD);
					if (firstInstanceByDefinitionId != null && (firstInstanceByDefinitionId.CurrentPrestigeLevel > 0 || firstInstanceByDefinitionId.state == PrestigeState.Questing || firstInstanceByDefinitionId.state == PrestigeState.Taskable || firstInstanceByDefinitionId.state == PrestigeState.Locked || firstInstanceByDefinitionId.state == PrestigeState.TaskableWhileQuesting))
					{
						dictionary.Add(iD, true);
					}
					else
					{
						dictionary.Add(iD, false);
					}
				}
			}
			return dictionary;
		}

		public void GetCharacterImageBasedOnMood(int prestigeDefinitionId, CharacterImageType type, out Sprite characterImage, out Sprite characterMask)
		{
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(prestigeDefinitionId);
			GetCharacterImageBasedOnMood(prestigeDefinition, type, out characterImage, out characterMask);
		}

		public void GetCharacterImageBasedOnMood(PrestigeDefinition prestigeDefinition, CharacterImageType type, out Sprite characterImage, out Sprite characterMask)
		{
			QuestResourceDefinition questResourceDefinition = null;
			characterImage = null;
			characterMask = null;
			questResourceDefinition = DetermineQuestResourceDefinition(prestigeDefinition, type);
			if (questResourceDefinition != null)
			{
				characterImage = UIUtils.LoadSpriteFromPath(questResourceDefinition.resourcePath);
				characterMask = UIUtils.LoadSpriteFromPath(questResourceDefinition.maskPath);
			}
		}

		public void GetCharacterImagePathBasedOnMood(int prestigeDefinitionId, CharacterImageType type, out string characterImagePath, out string characterMaskPath)
		{
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(prestigeDefinitionId);
			GetCharacterImagePathBasedOnMood(prestigeDefinition, type, out characterImagePath, out characterMaskPath);
		}

		public void GetCharacterImagePathBasedOnMood(PrestigeDefinition prestigeDefinition, CharacterImageType type, out string characterImagePath, out string characterMaskPath)
		{
			QuestResourceDefinition questResourceDefinition = null;
			characterImagePath = string.Empty;
			characterMaskPath = string.Empty;
			questResourceDefinition = DetermineQuestResourceDefinition(prestigeDefinition, type);
			if (questResourceDefinition != null)
			{
				characterImagePath = questResourceDefinition.resourcePath;
				characterMaskPath = questResourceDefinition.maskPath;
			}
		}

		public QuestResourceDefinition DetermineQuestResourceDefinition(int prestigeDefinitionId, CharacterImageType type)
		{
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(prestigeDefinitionId);
			return DetermineQuestResourceDefinition(prestigeDefinition, type);
		}

		private QuestResourceDefinition DetermineQuestResourceDefinition(PrestigeDefinition prestigeDefinition, CharacterImageType type)
		{
			QuestResourceDefinition result = null;
			switch (type)
			{
			case CharacterImageType.SmallAvatarIcon:
				result = definitionService.Get<QuestResourceDefinition>(prestigeDefinition.SmallAvatarResouceId);
				break;
			case CharacterImageType.WayfinderIcon:
				result = definitionService.Get<QuestResourceDefinition>(prestigeDefinition.WayFinderIconResourceId);
				break;
			case CharacterImageType.BigAvatarIcon:
				result = definitionService.Get<QuestResourceDefinition>(prestigeDefinition.BigAvatarResourceId);
				break;
			}
			return result;
		}

		public void AddMinionToTikiBarSlot(Character targetMinion, int slotIndex, TikiBarBuilding tikiBar, bool enablePathing = false)
		{
			CharacterObject characterObject = null;
			int iD = targetMinion.ID;
			Minion minion = targetMinion as Minion;
			if (minion != null)
			{
				MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
				characterObject = component.Get(iD);
			}
			else if (targetMinion is NamedCharacter)
			{
				NamedCharacterManagerView component2 = namedCharacterManager.GetComponent<NamedCharacterManagerView>();
				characterObject = component2.Get(iD);
			}
			if (characterObject == null)
			{
				logger.Error("AddMinionToTikiBarSlot: ao as MinionObject and NamedCharacterObject == null");
				return;
			}
			BuildingManagerView component3 = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component3.GetBuildingObject(tikiBar.ID);
			TikiBarBuildingObjectView tikiBarBuildingObjectView = buildingObject as TikiBarBuildingObjectView;
			Vector3 position = characterObject.transform.position;
			Vector3 routePosition = tikiBarBuildingObjectView.GetRoutePosition(slotIndex, tikiBar, position);
			Vector3 routeRotation = tikiBarBuildingObjectView.GetRouteRotation(slotIndex);
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			if (enablePathing)
			{
				IList<Vector3> list = pathFinder.FindPath(position, routePosition, 4, true);
				if (list == null)
				{
					List<Vector3> list2 = new List<Vector3>();
					list2.Add(routePosition);
					list = list2;
				}
				RouteInstructions type = default(RouteInstructions);
				type.minion = characterObject as MinionObject;
				type.Path = list;
				type.Rotation = routeRotation.y;
				type.TargetBuilding = tikiBar;
				injectionBinder.GetInstance<PathCharacterToTikiBarSignal>().Dispatch(characterObject, type, slotIndex);
			}
			else
			{
				injectionBinder.GetInstance<TeleportCharacterToTikiBarSignal>().Dispatch(characterObject, slotIndex);
			}
			Prestige prestigeFromMinionInstance = GetPrestigeFromMinionInstance(targetMinion);
			if (prestigeFromMinionInstance == null)
			{
				logger.Error("AddMinionToTikiBarSlot: Prestige == null for minion ID: {0}", iD);
				return;
			}
			ChangeToPrestigeState(prestigeFromMinionInstance, PrestigeState.Questing);
			if (minion != null)
			{
				minion.PrestigeId = prestigeFromMinionInstance.ID;
				injectionBinder.GetInstance<MinionStateChangeSignal>().Dispatch(iD, MinionState.Questing);
			}
		}

		public Prestige GetPrestigeFromMinionInstance(Character minionCharacter)
		{
			foreach (KeyValuePair<int, Prestige> item in prestigeMap)
			{
				Prestige value = item.Value;
				if (value.trackedInstanceId == minionCharacter.ID)
				{
					return value;
				}
			}
			logger.Log(KampaiLogLevel.Warning, "Prestige doesn't exist for the character: {0}", minionCharacter.ID);
			return null;
		}

		public Dictionary<int, Prestige> GetAllUnlockedPrestiges()
		{
			return prestigeMap;
		}

		public void AddPrestige(Prestige prestige)
		{
			if (!prestigeMap.ContainsKey(prestige.Definition.ID))
			{
				prestigeMap.Add(prestige.Definition.ID, prestige);
				playerService.Add(prestige);
			}
		}

		public void RemovePrestige(Prestige prestige)
		{
			if (prestigeMap.ContainsKey(prestige.Definition.ID))
			{
				prestigeMap.Remove(prestige.Definition.ID);
				playerService.Remove(prestige);
			}
		}

		public void ChangeToPrestigeState(Prestige prestige, PrestigeState targetState, int targetPrestigeLevel = 0, bool triggerNewQuest = true)
		{
			int iD = prestige.Definition.ID;
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			if (prestige.state == targetState)
			{
				if (targetState == PrestigeState.Questing && triggerNewQuest)
				{
					injectionBinder.GetInstance<GetNewQuestSignal>().Dispatch();
				}
				return;
			}
			PrestigeState state = prestige.state;
			prestige.state = targetState;
			switch (targetState)
			{
			case PrestigeState.PreUnlocked:
			case PrestigeState.Prestige:
				if (targetPrestigeLevel <= 0 || prestige.CurrentPrestigeLevel + 1 == targetPrestigeLevel)
				{
					prestige.CurrentPrestigeLevel = targetPrestigeLevel;
				}
				else if (targetPrestigeLevel >= 1 && prestige.CurrentPrestigeLevel < 0)
				{
					prestige.CurrentPrestigeLevel = 0;
				}
				else
				{
					prestige.CurrentPrestigeLevel++;
				}
				break;
			case PrestigeState.InQueue:
				prestige.CurrentPrestigePoints = 0;
				orderBoardService.ReplaceCharacterTickets(iD);
				injectionBinder.GetInstance<GetNewQuestSignal>().Dispatch();
				break;
			case PrestigeState.Questing:
				if (iD == 40000)
				{
					return;
				}
				if (prestige.UTCTimeUnlocked == 0)
				{
					prestige.UTCTimeUnlocked = timeService.CurrentTime();
				}
				prestige.CurrentPrestigePoints = 0;
				if (triggerNewQuest)
				{
					injectionBinder.GetInstance<GetNewQuestSignal>().Dispatch();
				}
				break;
			}
			if (targetState == PrestigeState.Prestige)
			{
				prestige.StartGameTime = playerDurationService.TotalGamePlaySeconds;
			}
			UpdatePrestigeSignal value;
			if (!updateTable.TryGetValue(prestige.Definition.Type, out value))
			{
				logger.Error("PrestigeService doesn't know how to update a presetige with type {0}!", prestige.Definition.Type);
			}
			else
			{
				Tuple<PrestigeState, PrestigeState> type = new Tuple<PrestigeState, PrestigeState>(state, targetState);
				value.Dispatch(prestige, type);
			}
		}

		public void UpdateEligiblePrestigeList()
		{
			IList<PrestigeDefinition> all = definitionService.GetAll<PrestigeDefinition>();
			foreach (PrestigeDefinition item in all)
			{
				int iD = item.ID;
				if (iD == 40000 || iD == 40014)
				{
					continue;
				}
				if (item.PrestigeLevelSettings == null)
				{
					uint quantity = playerService.GetQuantity(StaticItem.LEVEL_ID);
					MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
					if (item.PreUnlockLevel != 0 && quantity >= item.PreUnlockLevel && !prestigeMap.ContainsKey(iD) && minionPartyInstance != null && !minionPartyInstance.IsPartyHappening && !minionPartyInstance.IsPartyReady)
					{
						Prestige type = CreatePrestige(iD);
						minionPrestigeCompleteSignal.Dispatch(type);
					}
					continue;
				}
				if (prestigeMap.ContainsKey(iD))
				{
					Prestige prestige = prestigeMap[iD];
					if (prestige.state != 0 && prestige.state != PrestigeState.Taskable && prestige.state != PrestigeState.PreUnlocked)
					{
						continue;
					}
					int prestigeUnlockedPrestigeLevel = GetPrestigeUnlockedPrestigeLevel(item);
					if (prestige.CurrentPrestigeLevel < prestigeUnlockedPrestigeLevel)
					{
						if (prestige.CurrentPrestigeLevel > -1)
						{
							prestige.CurrentPrestigePoints = 0;
						}
						ChangeToPrestigeState(prestige, PrestigeState.Prestige, prestigeUnlockedPrestigeLevel);
						orderBoardService.AddPriorityPrestigeCharacter(iD);
					}
					continue;
				}
				int prestigeUnlockedPrestigeLevel2 = GetPrestigeUnlockedPrestigeLevel(item);
				if (prestigeUnlockedPrestigeLevel2 == -1)
				{
					Prestige prestige2 = new Prestige(item);
					ChangeToPrestigeState(prestige2, PrestigeState.PreUnlocked, prestigeUnlockedPrestigeLevel2);
					AddPrestige(prestige2);
					orderBoardService.AddPriorityPrestigeCharacter(iD);
				}
				else if (prestigeUnlockedPrestigeLevel2 >= 0)
				{
					Prestige prestige3 = new Prestige(item);
					ChangeToPrestigeState(prestige3, PrestigeState.Prestige, prestigeUnlockedPrestigeLevel2);
					AddPrestige(prestige3);
					orderBoardService.AddPriorityPrestigeCharacter(iD);
				}
			}
		}

		public void CheckCompletedPrestiges()
		{
			foreach (KeyValuePair<int, Prestige> item in prestigeMap)
			{
				Prestige value = item.Value;
				if (value.state == PrestigeState.Prestige && IsPrestigeFulfilled(value))
				{
					ChangeToPrestigeState(value, PrestigeState.InQueue);
				}
			}
		}

		public void PostOrderCompletion(Prestige prestige)
		{
			if (IsPrestigeFulfilled(prestige))
			{
				ChangeToPrestigeState(prestige, PrestigeState.InQueue);
			}
		}

		public bool IsPrestigeFulfilled(Prestige prestige)
		{
			if (prestige.CurrentPrestigeLevel >= 0 && prestige.Definition.PrestigeLevelSettings != null && prestige.Definition.PrestigeLevelSettings[prestige.CurrentPrestigeLevel] != null && prestige.CurrentPrestigePoints >= prestige.Definition.PrestigeLevelSettings[prestige.CurrentPrestigeLevel].PointsNeeded)
			{
				return true;
			}
			return false;
		}

		public int GetPrestigeUnlockedPrestigeLevel(PrestigeDefinition prestigeDefinition)
		{
			uint quantity = playerService.GetQuantity(StaticItem.LEVEL_ID);
			int result = -2;
			if (quantity < prestigeDefinition.PreUnlockLevel)
			{
				return result;
			}
			if (questService.IsQuestCompleted(prestigeDefinition.PrestigeLevelSettings[0].UnlockQuestID))
			{
				result = -1;
				for (int i = 0; i < prestigeDefinition.PrestigeLevelSettings.Count; i++)
				{
					if (prestigeDefinition.PrestigeLevelSettings[i].UnlockLevel <= quantity && questService.IsQuestCompleted(prestigeDefinition.PrestigeLevelSettings[i].UnlockQuestID))
					{
						result = i;
					}
				}
				return result;
			}
			return result;
		}

		public int ResolveTrackedId(int questTrackedInstanceId)
		{
			Character byInstanceId = playerService.GetByInstanceId<Character>(questTrackedInstanceId);
			int iD = byInstanceId.Definition.ID;
			if (iD == 70003 || iD == 70008)
			{
				return questTrackedInstanceId;
			}
			Villain villain = byInstanceId as Villain;
			if (villain != null && villain.CabanaBuildingId >= 0)
			{
				return villain.CabanaBuildingId;
			}
			if (byInstanceId is NamedCharacter || byInstanceId is Minion)
			{
				return 78;
			}
			return questTrackedInstanceId;
		}

		public bool IsTikiBarFull()
		{
			TikiBarBuilding byInstanceId = playerService.GetByInstanceId<TikiBarBuilding>(313);
			if (byInstanceId.GetOpenSlot() == -1)
			{
				return true;
			}
			return false;
		}

		public CabanaBuilding GetEmptyCabana()
		{
			foreach (Instance item in playerService.GetInstancesByDefinition<CabanaBuildingDefinition>())
			{
				CabanaBuilding cabanaBuilding = item as CabanaBuilding;
				if (cabanaBuilding != null && !cabanaBuilding.Occupied)
				{
					return cabanaBuilding;
				}
			}
			return null;
		}

		private void GeneratePrestigeMap()
		{
			IList<Prestige> instancesByType = playerService.GetInstancesByType<Prestige>();
			foreach (Prestige item in instancesByType)
			{
				prestigeMap.Add(item.Definition.ID, item);
			}
			if (!prestigeMap.ContainsKey(40000))
			{
				PrestigeDefinition def = definitionService.Get<PrestigeDefinition>(40000);
				Prestige prestige = new Prestige(def);
				prestige.state = PrestigeState.Taskable;
				prestige.trackedInstanceId = 78;
				AddPrestige(prestige);
			}
			if (!prestigeMap.ContainsKey(40014))
			{
				PrestigeDefinition def2 = definitionService.Get<PrestigeDefinition>(40014);
				Prestige prestige2 = new Prestige(def2);
				prestige2.state = PrestigeState.Taskable;
				prestige2.trackedInstanceId = 301;
				AddPrestige(prestige2);
			}
		}
	}
}
