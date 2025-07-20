using System;
using System.Collections.Generic;
using System.Linq;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class MasterPlanService : ActionableObjectManagerView, IMasterPlanService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MasterPlanService") as IKampaiLogger;

		private int _forceDefinitionID;

		private MasterPlan _currentMasterPlan;

		private MasterPlanComponent activeComponent;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public MasterPlanTaskCompleteSignal taskCompleteSignal { get; set; }

		[Inject]
		public MasterPlanComponentTaskUpdatedSignal taskUpdatedSignal { get; set; }

		[Inject]
		public ResetLairWayfinderIconSignal resetIconSignal { get; set; }

		[Inject]
		public SetMasterPlanWayfinderIconToCompleteSignal setCompleteIconSignal { get; set; }

		[Inject]
		public UpdateAllVillainLairCollidersSignal updateLairCollidersSignal { get; set; }

		public MasterPlan CurrentMasterPlan
		{
			get
			{
				if (_currentMasterPlan == null)
				{
					_currentMasterPlan = playerService.GetFirstInstanceByDefintion<MasterPlan, MasterPlanDefinition>();
				}
				return _currentMasterPlan;
			}
			private set
			{
				_currentMasterPlan = value;
			}
		}

		public void AddMasterPlanObject(MasterPlanObject obj)
		{
			if (!ActionableObjectManagerView.allObjects.ContainsKey(obj.ID))
			{
				ActionableObjectManagerView.allObjects.Add(obj.ID, obj);
			}
		}

		public void Initialize()
		{
			activeComponent = GetActiveComponent();
		}

		private MasterPlan CreateMasterPlan(MasterPlanDefinition masterPlanDefinition)
		{
			MasterPlan masterPlan = masterPlanDefinition.Build() as MasterPlan;
			masterPlan.StartGameTime = playerDurationService.TotalGamePlaySeconds;
			playerService.Add(masterPlan);
			CurrentMasterPlan = masterPlan;
			return masterPlan;
		}

		public void CreateMasterPlanComponents(MasterPlan masterPlan)
		{
			IList<MasterPlanComponentDefinition> componentDefinitions = GetComponentDefinitions(masterPlan.Definition);
			MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(componentDefinitions[0].ID);
			if (firstInstanceByDefinitionId != null)
			{
				logger.Warning("Trying to create components that already exist");
				return;
			}
			IList<int> compBuildingDefinitionIDs = masterPlan.Definition.CompBuildingDefinitionIDs;
			if (componentDefinitions.Count != compBuildingDefinitionIDs.Count)
			{
				logger.Fatal(FatalCode.DS_COMPONENT_COUNT_MISMATCH, masterPlan.Definition.ID);
				return;
			}
			VillainLairDefinition villainLairDefinition = definitionService.Get<VillainLairDefinition>(StaticItem.VILLAIN_LAIR_DEFINITION_ID);
			List<PlatformDefinition> platforms = villainLairDefinition.Platforms;
			for (int i = 0; i < componentDefinitions.Count; i++)
			{
				MasterPlanComponentDefinition masterPlanComponentDefinition = componentDefinitions[i];
				MasterPlanComponent masterPlanComponent = masterPlanComponentDefinition.Build() as MasterPlanComponent;
				masterPlanComponent.planTrackingInstance = masterPlan.ID;
				masterPlanComponent.reward = masterPlanComponentDefinition.Reward.Build();
				masterPlanComponent.buildingDefID = compBuildingDefinitionIDs[i];
				masterPlanComponent.buildingLocation = platforms[i].placementLocation;
				for (int j = 0; j < masterPlanComponentDefinition.Tasks.Count; j++)
				{
					masterPlanComponent.tasks.Add(masterPlanComponentDefinition.Tasks[j].Build());
				}
				playerService.Add(masterPlanComponent);
			}
			updateLairCollidersSignal.Dispatch();
		}

		public MasterPlan CreateNewMasterPlan()
		{
			if (_forceDefinitionID != 0)
			{
				return ForceCreateMasterPlan();
			}
			MasterPlanDefinition masterPlanDefinition = null;
			WeightedInstance weightedInstance = playerService.GetWeightedInstance(4037);
			IList<WeightedQuantityItem> entities = weightedInstance.Definition.Entities;
			if (CurrentMasterPlan == null)
			{
				masterPlanDefinition = definitionService.Get<MasterPlanDefinition>(entities[0].ID);
				MasterPlan masterPlan = CreateMasterPlan(masterPlanDefinition);
				CreateMasterPlanComponents(masterPlan);
				return masterPlan;
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.MASTER_PLAN_PLAYLIST_INDEX);
			if (quantity < entities.Count - 1)
			{
				playerService.AlterQuantity(StaticItem.MASTER_PLAN_PLAYLIST_INDEX, 1);
				quantity++;
				masterPlanDefinition = definitionService.Get<MasterPlanDefinition>(entities[quantity].ID);
			}
			else
			{
				masterPlanDefinition = GenerateNextRandomPlanDefinition(CurrentMasterPlan.Definition, weightedInstance);
			}
			playerService.Remove(CurrentMasterPlan);
			MasterPlan result = CreateMasterPlan(masterPlanDefinition);
			GenerateDynamicMasterPlan(masterPlanDefinition);
			return result;
		}

		public bool ForceNextMPDefinition(int defID)
		{
			_forceDefinitionID = defID;
			MasterPlanDefinition definition = null;
			if (definitionService.TryGet<MasterPlanDefinition>(defID, out definition))
			{
				return true;
			}
			_forceDefinitionID = 0;
			return false;
		}

		private MasterPlan ForceCreateMasterPlan()
		{
			if (CurrentMasterPlan != null)
			{
				playerService.Remove(CurrentMasterPlan);
			}
			MasterPlanDefinition masterPlanDefinition = definitionService.Get<MasterPlanDefinition>(_forceDefinitionID);
			_forceDefinitionID = 0;
			MasterPlan result = CreateMasterPlan(masterPlanDefinition);
			GenerateDynamicMasterPlan(masterPlanDefinition);
			return result;
		}

		private MasterPlanDefinition GenerateNextRandomPlanDefinition(MasterPlanDefinition currentPlanDef, WeightedInstance wi)
		{
			if (wi != null && wi.Definition.Entities.Count > 1)
			{
				List<int> list = new List<int>();
				for (int i = 0; i < wi.Definition.Entities.Count; i++)
				{
					int iD = wi.Definition.Entities[i].ID;
					if (iD != currentPlanDef.ID)
					{
						list.AddRange(Enumerable.Repeat(iD, (int)wi.Definition.Entities[i].Weight));
					}
				}
				if (list.Count > 0)
				{
					for (int j = 0; j < list.Count; j++)
					{
						int index = randomService.NextInt(j, list.Count);
						int value = list[j];
						list[j] = list[index];
						list[index] = value;
					}
					MasterPlanDefinition definition;
					if (definitionService.TryGet<MasterPlanDefinition>(list[list.Count - 1], out definition))
					{
						return definition;
					}
				}
			}
			logger.Error("Was unable to randomly create a new master plan.  Returning the old random plan: {0}", currentPlanDef.ID);
			return currentPlanDef;
		}

		private void GenerateDynamicMasterPlan(MasterPlanDefinition masterPlanDefinition)
		{
			IList<MasterPlanComponentDefinition> componentDefinitions = GetComponentDefinitions(masterPlanDefinition);
			MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(componentDefinitions[0].ID);
			if (firstInstanceByDefinitionId != null)
			{
				logger.Warning("Trying to create components that already exist");
				return;
			}
			IList<int> compBuildingDefinitionIDs = masterPlanDefinition.CompBuildingDefinitionIDs;
			if (componentDefinitions.Count != compBuildingDefinitionIDs.Count)
			{
				logger.Error("The number of components and buildings do not match for plan {0}", masterPlanDefinition.ID);
				logger.Fatal(FatalCode.DS_COMPONENT_COUNT_MISMATCH, masterPlanDefinition.ID);
				return;
			}
			List<PlatformDefinition> platforms = definitionService.Get<VillainLairDefinition>(StaticItem.VILLAIN_LAIR_DEFINITION_ID).Platforms;
			DynamicMasterPlanDefinition dynamicMasterPlanDefinition = definitionService.Get<DynamicMasterPlanDefinition>();
			IList<IngredientsItemDefinition> list = new List<IngredientsItemDefinition>();
			IList<IngredientsItemDefinition> list2 = new List<IngredientsItemDefinition>();
			GenerateItems(dynamicMasterPlanDefinition.ItemCategoryCount, list, list2);
			for (int i = 0; i < dynamicMasterPlanDefinition.DynamicComponents.Count; i++)
			{
				MasterPlanComponent masterPlanComponent = componentDefinitions[i].Build() as MasterPlanComponent;
				masterPlanComponent.planTrackingInstance = CurrentMasterPlan.ID;
				masterPlanComponent.buildingDefID = compBuildingDefinitionIDs[i];
				masterPlanComponent.buildingLocation = platforms[i].placementLocation;
				playerService.Add(masterPlanComponent);
				MasterPlanComponentDefinition masterPlanComponentDefinition = dynamicMasterPlanDefinition.DynamicComponents[i];
				for (int j = 0; j < masterPlanComponentDefinition.Tasks.Count; j++)
				{
					int requiredItemId = masterPlanComponentDefinition.Tasks[j].requiredItemId;
					IList<IngredientsItemDefinition> list4;
					if (requiredItemId % 2 == 0)
					{
						IList<IngredientsItemDefinition> list3 = list2;
						list4 = list3;
					}
					else
					{
						list4 = list;
					}
					IList<IngredientsItemDefinition> list5 = list4;
					IngredientsItemDefinition item = list5[requiredItemId / 2];
					MasterPlanComponentTask item2 = GenerateDynamicTask(masterPlanComponentDefinition.Tasks[j], item);
					masterPlanComponent.tasks.Add(item2);
				}
				int rewardItemId = masterPlanComponentDefinition.Reward.rewardItemId;
				IList<IngredientsItemDefinition> list6;
				if (rewardItemId % 2 == 0)
				{
					IList<IngredientsItemDefinition> list3 = list2;
					list6 = list3;
				}
				else
				{
					list6 = list;
				}
				IList<IngredientsItemDefinition> list7 = list6;
				IngredientsItemDefinition item3 = list7[rewardItemId / 2];
				masterPlanComponent.reward = GenerateDynamicReward(masterPlanComponent, item3);
			}
			updateLairCollidersSignal.Dispatch();
		}

		public bool HasReceivedInitialRewardFromCurrentPlan()
		{
			if (CurrentMasterPlan == null)
			{
				return false;
			}
			return HasReceivedInitialRewardFromPlanDefinition(CurrentMasterPlan.Definition);
		}

		public bool HasReceivedInitialRewardFromPlanDefinition(MasterPlanDefinition planDefinition)
		{
			if (planDefinition == null)
			{
				return false;
			}
			return playerService.GetInstancesByDefinitionID(planDefinition.LeavebehindBuildingDefID).Count > 0;
		}

		public void SelectMasterPlanComponent(MasterPlanComponent component)
		{
			activeComponent = component;
			if (activeComponent != null)
			{
				if (activeComponent.State == MasterPlanComponentState.NotStarted)
				{
					activeComponent.State = MasterPlanComponentState.InProgress;
				}
				ProcessActiveComponent(MasterPlanComponentTaskType.Deliver, 0u);
				ProcessActiveComponent(MasterPlanComponentTaskType.Collect, 0u);
			}
		}

		public void ProcessTransactionData(TransactionUpdateData data)
		{
			if (activeComponent == null)
			{
				return;
			}
			ProcessActiveComponent(MasterPlanComponentTaskType.Deliver, 0u);
			if (data.Target == TransactionTarget.BLACKMARKETBOARD && data.Type == UpdateType.TRANSACTION_FINISH)
			{
				ProcessActiveComponent(MasterPlanComponentTaskType.CompleteOrders, 1u);
			}
			if (data.Outputs == null)
			{
				return;
			}
			IList<QuantityItem> outputs = data.Outputs;
			foreach (QuantityItem item in outputs)
			{
				if (item.Quantity != 0)
				{
					if (item.ID == 2)
					{
						ProcessActiveComponent(MasterPlanComponentTaskType.EarnPartyPoints, item.Quantity, item.ID);
					}
					if (data.Target == TransactionTarget.HARVEST || data.Target == TransactionTarget.MARKETPLACE)
					{
						ProcessActiveComponent(MasterPlanComponentTaskType.Collect, item.Quantity, item.ID);
					}
					if (data.Target == TransactionTarget.BLACKMARKETBOARD && item.ID == 0)
					{
						ProcessActiveComponent(MasterPlanComponentTaskType.EarnSandDollars, item.Quantity, 3022);
					}
					else if (data.Target == TransactionTarget.MARKETPLACE && item.ID == 0)
					{
						ProcessActiveComponent(MasterPlanComponentTaskType.EarnSandDollars, item.Quantity, 3117);
					}
					else if (item.ID == 0)
					{
						ProcessActiveComponent(MasterPlanComponentTaskType.EarnSandDollars, item.Quantity);
					}
				}
			}
		}

		public void ProcessActiveComponent(MasterPlanComponentTaskType type, uint progress, int source = 0)
		{
			if (activeComponent == null)
			{
				return;
			}
			for (int i = 0; i < activeComponent.tasks.Count; i++)
			{
				MasterPlanComponentTask masterPlanComponentTask = activeComponent.tasks[i];
				if (activeComponent.tasks[i].Definition.Type == type)
				{
					ProcessTask(masterPlanComponentTask, progress, source);
				}
				if (masterPlanComponentTask.isHarvestable && masterPlanComponentTask.Definition.Type != 0)
				{
					taskCompleteSignal.Dispatch(activeComponent, i);
				}
			}
			taskUpdatedSignal.Dispatch(activeComponent);
		}

		private MasterPlanComponentTask GenerateDynamicTask(MasterPlanComponentTaskDefinition taskDefinition, IngredientsItemDefinition item)
		{
			MasterPlanComponentTask masterPlanComponentTask = new MasterPlanComponentTask();
			masterPlanComponentTask.Definition = new MasterPlanComponentTaskDefinition();
			masterPlanComponentTask.Definition.Type = taskDefinition.Type;
			DynamicMasterPlanDefinition dynamicMasterPlanDefinition = definitionService.Get<DynamicMasterPlanDefinition>();
			switch (taskDefinition.Type)
			{
			case MasterPlanComponentTaskType.Deliver:
			case MasterPlanComponentTaskType.Collect:
				masterPlanComponentTask.Definition.requiredItemId = item.ID;
				masterPlanComponentTask.Definition.requiredQuantity = (uint)GenerateItemQuantity(item);
				break;
			case MasterPlanComponentTaskType.CompleteOrders:
				masterPlanComponentTask.Definition.requiredQuantity = (uint)randomService.NextInt((int)dynamicMasterPlanDefinition.FillOrderRangeMin, (int)dynamicMasterPlanDefinition.FillOrderRangeMax);
				break;
			case MasterPlanComponentTaskType.PlayMiniGame:
				if (randomService.NextBoolean())
				{
					IList<MignetteBuilding> instancesByType2 = playerService.GetInstancesByType<MignetteBuilding>();
					int index2 = randomService.NextInt(instancesByType2.Count);
					masterPlanComponentTask.Definition.requiredItemId = instancesByType2[index2].Definition.ID;
				}
				masterPlanComponentTask.Definition.ShowWayfinder = taskDefinition.ShowWayfinder;
				masterPlanComponentTask.Definition.requiredQuantity = (uint)randomService.NextInt((int)dynamicMasterPlanDefinition.PlayMiniGameRangeMin, (int)dynamicMasterPlanDefinition.PlayMiniGameRangeMax);
				break;
			case MasterPlanComponentTaskType.MiniGameScore:
			{
				IList<MignetteBuilding> instancesByType = playerService.GetInstancesByType<MignetteBuilding>();
				int index = randomService.NextInt(instancesByType.Count);
				MignetteBuildingDefinition definition = instancesByType[index].Definition;
				masterPlanComponentTask.Definition.requiredItemId = definition.ID;
				MiniGameScoreRange miniGameScoreRange = GetMiniGameScoreRange(definition.ID);
				masterPlanComponentTask.Definition.requiredQuantity = GetRoundedQuantityWithinRange(miniGameScoreRange.ScoreRangeMin, miniGameScoreRange.ScoreRangeMax);
				masterPlanComponentTask.Definition.ShowWayfinder = taskDefinition.ShowWayfinder;
				break;
			}
			case MasterPlanComponentTaskType.EarnPartyPoints:
			case MasterPlanComponentTaskType.EarnLeisurePartyPoints:
			case MasterPlanComponentTaskType.EarnMignettePartyPoints:
				masterPlanComponentTask.Definition.requiredQuantity = GetRoundedQuantityWithinRange((int)dynamicMasterPlanDefinition.PartyPointsRangeMin, (int)dynamicMasterPlanDefinition.PartyPointsRangeMax);
				break;
			case MasterPlanComponentTaskType.EarnSandDollars:
				masterPlanComponentTask.Definition.requiredQuantity = GetRoundedQuantityWithinRange((int)dynamicMasterPlanDefinition.EarnSandDollarMin, (int)dynamicMasterPlanDefinition.EarnSandDollarMax);
				break;
			default:
				logger.Error("Undefined component task type: {0}", taskDefinition.Type);
				break;
			}
			return masterPlanComponentTask;
		}

		private MasterPlanComponentReward GenerateDynamicReward(MasterPlanComponent component, IngredientsItemDefinition item)
		{
			MasterPlanComponentReward masterPlanComponentReward = new MasterPlanComponentReward();
			masterPlanComponentReward.Definition = new MasterPlanComponentRewardDefinition();
			masterPlanComponentReward.Definition.rewardItemId = item.ID;
			masterPlanComponentReward.Definition.rewardQuantity = (uint)Mathf.CeilToInt((float)GenerateItemQuantity(item) / 2f);
			uint num = 0u;
			uint num2 = 0u;
			DynamicMasterPlanDefinition dynamicMasterPlanDefinition = definitionService.Get<DynamicMasterPlanDefinition>();
			for (int i = 0; i < component.tasks.Count; i++)
			{
				MasterPlanComponentTaskDefinition definition = component.tasks[i].Definition;
				switch (definition.Type)
				{
				case MasterPlanComponentTaskType.Deliver:
				case MasterPlanComponentTaskType.Collect:
				{
					ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(definition.requiredItemId);
					num += (uint)(itemDefinition.BaseGrindCost * (int)definition.requiredQuantity);
					break;
				}
				case MasterPlanComponentTaskType.CompleteOrders:
					num2 += GetPremiumReward(dynamicMasterPlanDefinition.RewardTableCompleteOrders, definition.requiredQuantity);
					break;
				case MasterPlanComponentTaskType.PlayMiniGame:
					num2 += GetPremiumReward(dynamicMasterPlanDefinition.RewardTablePlayMiniGame, definition.requiredQuantity);
					break;
				case MasterPlanComponentTaskType.MiniGameScore:
				{
					MiniGameScoreReward miniGameReward = GetMiniGameReward(definition.requiredItemId);
					num2 += GetPremiumReward(miniGameReward.rewardTable, definition.requiredQuantity);
					break;
				}
				case MasterPlanComponentTaskType.EarnPartyPoints:
				case MasterPlanComponentTaskType.EarnLeisurePartyPoints:
				case MasterPlanComponentTaskType.EarnMignettePartyPoints:
					num2 += GetPremiumReward(dynamicMasterPlanDefinition.RewardTableEarnPartyPoints, definition.requiredQuantity);
					break;
				case MasterPlanComponentTaskType.EarnSandDollars:
					num2 += GetPremiumReward(dynamicMasterPlanDefinition.RewardTableEarnSandDollars, definition.requiredQuantity);
					break;
				}
			}
			masterPlanComponentReward.Definition.grindReward = Math.Max(dynamicMasterPlanDefinition.MinGrindReward, num);
			masterPlanComponentReward.Definition.premiumReward = Math.Max(dynamicMasterPlanDefinition.MinPremiumReward, num2);
			return masterPlanComponentReward;
		}

		private uint GetPremiumReward(IList<Reward> rewardTable, uint requiredQuantity)
		{
			for (int num = rewardTable.Count - 1; num >= 0; num--)
			{
				if (rewardTable[num].requiredQuantity <= requiredQuantity)
				{
					return rewardTable[num].premiumReward;
				}
			}
			return 0u;
		}

		private uint GetRoundedQuantityWithinRange(int rangeMin, int rangeMax)
		{
			int num = randomService.NextInt(rangeMin, rangeMax);
			int b = ((num != 0) ? ((int)Math.Round((double)num / 100.0, 0) * 100) : 0);
			return (uint)Mathf.Max(rangeMin, b);
		}

		private MiniGameScoreRange GetMiniGameScoreRange(int miniGameDefinitionId)
		{
			DynamicMasterPlanDefinition dynamicMasterPlanDefinition = definitionService.Get<DynamicMasterPlanDefinition>();
			foreach (MiniGameScoreRange item in dynamicMasterPlanDefinition.RequirementTableMiniGameScore)
			{
				if (item.MiniGameId == miniGameDefinitionId)
				{
					return item;
				}
			}
			return null;
		}

		private MiniGameScoreReward GetMiniGameReward(int miniGameDefinitionId)
		{
			DynamicMasterPlanDefinition dynamicMasterPlanDefinition = definitionService.Get<DynamicMasterPlanDefinition>();
			foreach (MiniGameScoreReward item in dynamicMasterPlanDefinition.RewardTableMiniGameScore)
			{
				if (item.MiniGameId == miniGameDefinitionId)
				{
					return item;
				}
			}
			return null;
		}

		private void GenerateItems(int itemCount, IList<IngredientsItemDefinition> craftItems, IList<IngredientsItemDefinition> resourceItems)
		{
			IList<IngredientsItemDefinition> list = GenerateCategory("Craftable");
			IList<IngredientsItemDefinition> list2 = GenerateCategory("Base Resource");
			while (craftItems.Count < itemCount)
			{
				IngredientsItemDefinition ingredientsItemDefinition = list[randomService.NextInt(list.Count)];
				if (craftItems.Contains(ingredientsItemDefinition))
				{
					continue;
				}
				IList<IngredientsItemDefinition> list3 = new List<IngredientsItemDefinition>();
				TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(ingredientsItemDefinition.TransactionId);
				for (int i = 0; i < transactionDefinition.Inputs.Count; i++)
				{
					IngredientsItemDefinition ingredientsItemDefinition2 = definitionService.Get<IngredientsItemDefinition>(transactionDefinition.Inputs[i].ID);
					if (ingredientsItemDefinition2 != null && !(ingredientsItemDefinition2 is DynamicIngredientsDefinition) && list2.Contains(ingredientsItemDefinition2) && !resourceItems.Contains(ingredientsItemDefinition2))
					{
						list3.Add(ingredientsItemDefinition2);
					}
				}
				if (list3.Count != 0)
				{
					craftItems.Add(ingredientsItemDefinition);
					int index = randomService.NextInt(list3.Count);
					resourceItems.Add(list3[index]);
				}
			}
		}

		private int GenerateItemQuantity(IngredientsItemDefinition item)
		{
			DynamicMasterPlanDefinition dynamicMasterPlanDefinition = definitionService.Get<DynamicMasterPlanDefinition>();
			int a = (int)(dynamicMasterPlanDefinition.MaxProductionTime / IngredientsItemUtil.GetHarvestTimeFromIngredientDefinition(item, definitionService));
			int b = (int)(dynamicMasterPlanDefinition.MaxStorageCapactiy * (float)playerService.GetCurrentStorageCapacity());
			int a2 = Mathf.Min(a, b);
			int num = Mathf.Min(a2, (int)dynamicMasterPlanDefinition.MaxProductionCount);
			return (num != 0) ? num : ((int)dynamicMasterPlanDefinition.MinProductionCount);
		}

		private IList<IngredientsItemDefinition> GenerateCategory(string category)
		{
			IList<IngredientsItemDefinition> list = new List<IngredientsItemDefinition>();
			IList<IngredientsItemDefinition> unlockedDefsByType = playerService.GetUnlockedDefsByType<IngredientsItemDefinition>();
			for (int i = 0; i < unlockedDefsByType.Count; i++)
			{
				if (category.Equals(unlockedDefsByType[i].TaxonomySpecific))
				{
					list.Add(unlockedDefsByType[i]);
				}
			}
			return list;
		}

		private void ProcessTask(MasterPlanComponentTask task, uint progress, int source = 0)
		{
			if (task.isComplete)
			{
				return;
			}
			int requiredItemId = task.Definition.requiredItemId;
			switch (task.Definition.Type)
			{
			case MasterPlanComponentTaskType.Deliver:
			case MasterPlanComponentTaskType.Collect:
				task.earnedQuantity = playerService.GetQuantity((StaticItem)requiredItemId);
				break;
			case MasterPlanComponentTaskType.CompleteOrders:
				task.earnedQuantity += progress;
				break;
			case MasterPlanComponentTaskType.PlayMiniGame:
			case MasterPlanComponentTaskType.MiniGameScore:
			case MasterPlanComponentTaskType.EarnPartyPoints:
			case MasterPlanComponentTaskType.EarnLeisurePartyPoints:
			case MasterPlanComponentTaskType.EarnMignettePartyPoints:
			case MasterPlanComponentTaskType.EarnSandDollars:
				if (requiredItemId == 0 || requiredItemId == source)
				{
					task.earnedQuantity += progress;
				}
				break;
			default:
				logger.Error("Undefined component task type: {0}", task.Definition.Type);
				break;
			}
		}

		private MasterPlanComponent GetActiveComponent()
		{
			IList<MasterPlanComponent> instancesByType = playerService.GetInstancesByType<MasterPlanComponent>();
			foreach (MasterPlanComponent item in instancesByType)
			{
				if (item.State == MasterPlanComponentState.NotStarted || item.State == MasterPlanComponentState.Complete)
				{
					continue;
				}
				return item;
			}
			return null;
		}

		public MasterPlanComponent GetActiveComponentFromPlanDefinition(int masterPlanDefinitionID)
		{
			MasterPlanDefinition masterPlanDefinition = definitionService.Get<MasterPlanDefinition>(masterPlanDefinitionID);
			for (int i = 0; i < masterPlanDefinition.ComponentDefinitionIDs.Count; i++)
			{
				MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(masterPlanDefinition.ComponentDefinitionIDs[i]);
				if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.State != 0 && firstInstanceByDefinitionId.State != MasterPlanComponentState.Complete)
				{
					return firstInstanceByDefinitionId;
				}
			}
			return null;
		}

		public bool AllComponentsAreComplete(int masterPlanDefinitionID)
		{
			MasterPlanDefinition masterPlanDefinition = definitionService.Get<MasterPlanDefinition>(masterPlanDefinitionID);
			for (int i = 0; i < masterPlanDefinition.ComponentDefinitionIDs.Count; i++)
			{
				MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(masterPlanDefinition.ComponentDefinitionIDs[i]);
				if (firstInstanceByDefinitionId == null || firstInstanceByDefinitionId.State != MasterPlanComponentState.Complete)
				{
					return false;
				}
			}
			return true;
		}

		public int GetComponentCompleteCount(MasterPlanDefinition definition)
		{
			int num = 0;
			foreach (int componentDefinitionID in definition.ComponentDefinitionIDs)
			{
				MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(componentDefinitionID);
				if (firstInstanceByDefinitionId.State == MasterPlanComponentState.Complete)
				{
					num++;
				}
			}
			return num;
		}

		public IList<MasterPlanComponent> GetInactiveComponents(MasterPlanDefinition masterPlanDefinition)
		{
			IList<MasterPlanComponent> list = new List<MasterPlanComponent>();
			foreach (int componentDefinitionID in masterPlanDefinition.ComponentDefinitionIDs)
			{
				MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(componentDefinitionID);
				if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.State == MasterPlanComponentState.NotStarted && masterPlanDefinition.ComponentDefinitionIDs.Contains(firstInstanceByDefinitionId.Definition.ID))
				{
					list.Add(firstInstanceByDefinitionId);
				}
			}
			return list;
		}

		public void SetWayfinderState()
		{
			MasterPlan currentMasterPlan = CurrentMasterPlan;
			if (currentMasterPlan == null)
			{
				resetIconSignal.Dispatch();
				return;
			}
			int iD = currentMasterPlan.Definition.ID;
			MasterPlanComponent activeComponentFromPlanDefinition = GetActiveComponentFromPlanDefinition(iD);
			if (activeComponentFromPlanDefinition != null)
			{
				taskUpdatedSignal.Dispatch(activeComponentFromPlanDefinition);
				return;
			}
			MasterPlanDefinition masterPlanDefinition = definitionService.Get<MasterPlanDefinition>(iD);
			MasterPlanComponentBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(masterPlanDefinition.BuildingDefID);
			if (AllComponentsAreComplete(iD) && firstInstanceByDefinitionId != null)
			{
				setCompleteIconSignal.Dispatch();
			}
			else
			{
				resetIconSignal.Dispatch();
			}
		}

		private IList<MasterPlanComponentDefinition> GetComponentDefinitions(MasterPlanDefinition masterPlanDefinition)
		{
			List<MasterPlanComponentDefinition> list = new List<MasterPlanComponentDefinition>();
			foreach (int componentDefinitionID in masterPlanDefinition.ComponentDefinitionIDs)
			{
				MasterPlanComponentDefinition item = definitionService.Get<MasterPlanComponentDefinition>(componentDefinitionID);
				list.Add(item);
			}
			return list;
		}

		public Vector3 GetComponentBuildingOffset(int buildingID)
		{
			Vector3 zero = Vector3.zero;
			VillainLairDefinition villainLairDefinition = definitionService.Get<VillainLairDefinition>(StaticItem.VILLAIN_LAIR_DEFINITION_ID);
			List<int> compBuildingDefinitionIDs = CurrentMasterPlan.Definition.CompBuildingDefinitionIDs;
			List<PlatformDefinition> platforms = villainLairDefinition.Platforms;
			int index = 0;
			if (platforms == null || platforms.Count < 1)
			{
				return zero;
			}
			if (CurrentMasterPlan.Definition.BuildingDefID == buildingID)
			{
				index = platforms.Count - 1;
			}
			else
			{
				for (int i = 0; i < compBuildingDefinitionIDs.Count; i++)
				{
					if (compBuildingDefinitionIDs[i] == buildingID)
					{
						index = i;
						break;
					}
				}
			}
			return platforms[index].offset;
		}

		public Vector3 GetComponentBuildingPosition(int buildingID)
		{
			Vector3 zero = Vector3.zero;
			Location location = null;
			VillainLairDefinition villainLairDefinition = definitionService.Get<VillainLairDefinition>(StaticItem.VILLAIN_LAIR_DEFINITION_ID);
			List<int> compBuildingDefinitionIDs = CurrentMasterPlan.Definition.CompBuildingDefinitionIDs;
			List<PlatformDefinition> platforms = villainLairDefinition.Platforms;
			int index = 0;
			if (platforms == null || platforms.Count < 1)
			{
				return zero;
			}
			if (CurrentMasterPlan.Definition.BuildingDefID == buildingID)
			{
				index = platforms.Count - 1;
			}
			else
			{
				for (int i = 0; i < compBuildingDefinitionIDs.Count; i++)
				{
					if (compBuildingDefinitionIDs[i] == buildingID)
					{
						index = i;
						break;
					}
				}
			}
			zero = platforms[index].offset;
			location = platforms[index].placementLocation;
			return zero + (Vector3)location;
		}
	}
}
