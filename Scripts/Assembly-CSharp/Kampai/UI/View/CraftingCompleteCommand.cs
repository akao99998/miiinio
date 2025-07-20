using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class CraftingCompleteCommand : Command
	{
		[Inject]
		public int buildingID { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public CraftingCompleteSignal craftingComplete { get; set; }

		[Inject]
		public RemoveCraftingQueueSignal removeCraftingQueueSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public UIModel model { get; set; }

		public override void Execute()
		{
			CraftingBuilding byInstanceId = playerService.GetByInstanceId<CraftingBuilding>(buildingID);
			if (!model.CraftingUIOpen && byInstanceId.RecipeInQueue.Count > 0)
			{
				removeCraftingQueueSignal.Dispatch(new Tuple<int, int>(buildingID, 0));
				if (byInstanceId.RecipeInQueue.Count > 0)
				{
					byInstanceId.CraftingStartTime = timeService.CurrentTime();
					IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(byInstanceId.RecipeInQueue[0]);
					timeEventService.AddEvent(byInstanceId.ID, timeService.CurrentTime(), (int)ingredientsItemDefinition.TimeToHarvest, craftingComplete, TimeEventType.ProductionBuff);
				}
			}
		}
	}
}
