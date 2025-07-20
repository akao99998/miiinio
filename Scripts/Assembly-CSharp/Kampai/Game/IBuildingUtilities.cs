namespace Kampai.Game
{
	public interface IBuildingUtilities
	{
		bool ValidateScaffoldingPlacement(BuildingDefinition buildingDef, Location location);

		bool ValidateLocation(Building building, Location location);

		bool CheckGridBounds(Location location);

		bool CheckGridBounds(int x, int y);

		bool ValidateGridSquare(Building building, Location location);

		bool ValidateGridSquare(Building building, int x, int y);

		int AvailableLandSpaceCount();
	}
}
