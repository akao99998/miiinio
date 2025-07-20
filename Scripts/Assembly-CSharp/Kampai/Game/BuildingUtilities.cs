namespace Kampai.Game
{
	public class BuildingUtilities : IBuildingUtilities
	{
		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public bool ValidateScaffoldingPlacement(BuildingDefinition buildingDef, Location location)
		{
			return ValidateLocation(null, buildingDef, location);
		}

		public bool ValidateLocation(Building building, Location location)
		{
			return ValidateLocation(building, building.Definition, location);
		}

		private bool ValidateLocation(Building building, BuildingDefinition buildingDef, Location location)
		{
			if (!CheckGridBounds(location))
			{
				return false;
			}
			if (!ValidateGridSquare(building, location))
			{
				return false;
			}
			int x = location.x;
			int num = location.y;
			int num2 = x;
			string buildingFootprint = definitionService.GetBuildingFootprint(buildingDef.FootprintID);
			string text = buildingFootprint;
			foreach (char c in text)
			{
				if (c == '|')
				{
					num2 = x;
					num--;
				}
				else
				{
					if (!ValidateGridSquare(building, num2, num))
					{
						return false;
					}
					num2++;
				}
				if (!CheckGridBounds(num2, num))
				{
					return false;
				}
			}
			return true;
		}

		public bool CheckGridBounds(Location location)
		{
			return CheckGridBounds(location.x, location.y);
		}

		public bool CheckGridBounds(int x, int y)
		{
			if (x < 0 || x >= environment.PlayerGrid.GetLength(0))
			{
				return false;
			}
			if (y < 0 || y >= environment.PlayerGrid.GetLength(1))
			{
				return false;
			}
			return true;
		}

		public bool ValidateGridSquare(Building building, Location location)
		{
			return ValidateGridSquare(building, location.x, location.y);
		}

		public bool ValidateGridSquare(Building building, int x, int y)
		{
			EnvironmentGridSquare[,] playerGrid = environment.PlayerGrid;
			EnvironmentGridSquareDefinition[,] definitionGrid = environment.Definition.DefinitionGrid;
			if (playerGrid[x, y] == null)
			{
				return false;
			}
			if (playerGrid[x, y].Occupied && playerGrid[x, y].Instance != null && (building == null || playerGrid[x, y].Instance != building))
			{
				return false;
			}
			if (!playerGrid[x, y].Unlocked || !definitionGrid[x, y].Usable)
			{
				return false;
			}
			return true;
		}

		public int AvailableLandSpaceCount()
		{
			EnvironmentGridSquare[,] playerGrid = environment.PlayerGrid;
			int num = 0;
			for (int i = 0; i < playerGrid.GetLength(0); i++)
			{
				for (int j = 0; j < playerGrid.GetLength(1); j++)
				{
					EnvironmentGridSquare environmentGridSquare = playerGrid[i, j];
					if (environmentGridSquare != null && environmentGridSquare.Unlocked && !environmentGridSquare.Occupied && environmentGridSquare.Instance == null)
					{
						num++;
					}
				}
			}
			return num;
		}
	}
}
