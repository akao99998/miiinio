using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RemoveBuildingCommand : Command
	{
		[Inject]
		public Location location { get; set; }

		[Inject]
		public string footprint { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public IBuildingUtilities buildingUtil { get; set; }

		public override void Execute()
		{
			int x = location.x;
			int num = x;
			int num2 = location.y;
			string text = footprint;
			foreach (char c in text)
			{
				if (!buildingUtil.CheckGridBounds(num, num2))
				{
					num = x;
					num2--;
					continue;
				}
				switch (c)
				{
				case '|':
					num = x;
					num2--;
					break;
				case '?':
					num++;
					break;
				default:
					environment.PlayerGrid[num, num2].Walkable = environment.Definition.DefinitionGrid[num, num2].Pathable;
					environment.PlayerGrid[num, num2].Occupied = false;
					environment.PlayerGrid[num, num2].Instance = null;
					num++;
					break;
				}
			}
			pathFinder.UpdateWalkableRegion();
		}
	}
}
