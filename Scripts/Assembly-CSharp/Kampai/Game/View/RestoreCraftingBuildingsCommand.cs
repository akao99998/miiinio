using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class RestoreCraftingBuildingsCommand : Command
	{
		[Inject]
		public CraftingBuilding building { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public HarvestReadySignal harvestSignal { get; set; }

		[Inject]
		public CraftingCompleteSignal craftingCompleteSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		public override void Execute()
		{
			RestoreCraftingBuilding(building);
		}

		private void RestoreCraftingBuilding(CraftingBuilding craftingBuilding)
		{
			if (craftingBuilding.State != BuildingState.Construction && craftingBuilding.State != 0)
			{
				timeEventService.RemoveEvent(craftingBuilding.ID);
			}
			IList<int> list = new List<int>();
			int craftingStartTime = craftingBuilding.CraftingStartTime;
			craftingStartTime -= craftingBuilding.PartyTimeReduction;
			craftingBuilding.PartyTimeReduction = 0;
			IList<int> recipesInQueue = craftingBuilding.RecipeInQueue;
			RemoveUnusedOneOffCraftable(craftingBuilding, list, ref recipesInQueue);
			foreach (int item in recipesInQueue)
			{
				IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(item);
				if (craftingStartTime + ingredientsItemDefinition.TimeToHarvest <= timeService.CurrentTime())
				{
					list.Add(ingredientsItemDefinition.ID);
					craftingBuilding.CompletedCrafts.Add(ingredientsItemDefinition.ID);
					craftingStartTime += Convert.ToInt32(ingredientsItemDefinition.TimeToHarvest);
					continue;
				}
				timeEventService.AddEvent(craftingBuilding.ID, craftingStartTime, (int)ingredientsItemDefinition.TimeToHarvest, craftingCompleteSignal, TimeEventType.ProductionBuff);
				break;
			}
			craftingBuilding.CraftingStartTime = craftingStartTime;
			foreach (int item2 in list)
			{
				craftingBuilding.RecipeInQueue.Remove(item2);
			}
			ValidateCompletedQueue(craftingBuilding);
			SetState(craftingBuilding);
		}

		private void ValidateCompletedQueue(CraftingBuilding craftingBuilding)
		{
			List<int> list = new List<int>();
			foreach (int completedCraft in craftingBuilding.CompletedCrafts)
			{
				DynamicIngredientsDefinition definition;
				if (definitionService.TryGet<DynamicIngredientsDefinition>(completedCraft, out definition) && definition.Depreciated)
				{
					list.Add(completedCraft);
				}
			}
			foreach (int item in list)
			{
				craftingBuilding.CompletedCrafts.Remove(item);
			}
		}

		private void RemoveUnusedOneOffCraftable(CraftingBuilding craftingBuilding, IList<int> toBeRemoved, ref IList<int> recipesInQueue)
		{
			foreach (int item in recipesInQueue)
			{
				DynamicIngredientsDefinition definition = null;
				definitionService.TryGet<DynamicIngredientsDefinition>(item, out definition);
				if (definition != null && definition.Depreciated)
				{
					toBeRemoved.Add(item);
				}
			}
			foreach (int item2 in toBeRemoved)
			{
				craftingBuilding.RecipeInQueue.Remove(item2);
			}
			recipesInQueue = craftingBuilding.RecipeInQueue;
			toBeRemoved.Clear();
		}

		private void SetState(CraftingBuilding craftingBuilding)
		{
			BuildingState newState = BuildingState.Inactive;
			if (craftingBuilding.CompletedCrafts.Count > 0)
			{
				harvestSignal.Dispatch(craftingBuilding.ID);
				if (craftingBuilding.RecipeInQueue.Count > 0)
				{
					newState = BuildingState.HarvestableAndWorking;
				}
				else
				{
					newState = BuildingState.Harvestable;
				}
			}
			else if (craftingBuilding.RecipeInQueue.Count > 0)
			{
				newState = BuildingState.Working;
			}
			if (newState != 0)
			{
				routineRunner.StartCoroutine(WaitAFrame(delegate
				{
					buildingChangeStateSignal.Dispatch(craftingBuilding.ID, newState);
				}));
			}
		}

		private IEnumerator WaitAFrame(Action a)
		{
			yield return null;
			a();
		}
	}
}
