using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class AddFootprintCommand : Command
	{
		[Inject]
		public Locatable locatable { get; set; }

		[Inject]
		public Location location { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public override void Execute()
		{
			int x = location.x;
			int num = x;
			int num2 = location.y;
			string text = string.Empty;
			Instance instance = null;
			if (locatable is Building)
			{
				Building building = locatable as Building;
				text = definitionService.GetBuildingFootprint(building.Definition.FootprintID);
				instance = building;
			}
			else if (locatable is Plot)
			{
				Plot plot = locatable as Plot;
				text = definitionService.GetBuildingFootprint(plot.Definition.FootprintID);
				instance = plot;
			}
			string text2 = text;
			for (int i = 0; i < text2.Length; i++)
			{
				switch (text2[i])
				{
				case '.':
					environment.PlayerGrid[num, num2].Walkable = true;
					environment.PlayerGrid[num, num2].Occupied = true;
					environment.PlayerGrid[num, num2].Instance = instance;
					num++;
					break;
				case '?':
					num++;
					break;
				case '|':
					num = x;
					num2--;
					break;
				default:
					environment.PlayerGrid[num, num2].Walkable = false;
					environment.PlayerGrid[num, num2].Occupied = true;
					environment.PlayerGrid[num, num2].Instance = instance;
					num++;
					break;
				}
			}
			pathFinder.UpdateWalkableRegion();
		}
	}
}
