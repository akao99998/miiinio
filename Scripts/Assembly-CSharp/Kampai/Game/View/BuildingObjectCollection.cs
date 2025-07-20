namespace Kampai.Game.View
{
	public class BuildingObjectCollection
	{
		public BuildingObject BuildingObject { get; set; }

		public ScaffoldingBuildingObject ScaffoldingBuildingObject { get; set; }

		public RibbonBuildingObject RibbonBuildingObject { get; set; }

		public PlatformBuildingObject PlatformBuildingObject { get; set; }

		public bool Rushed { get; set; }

		public BuildingObjectCollection(BuildingObject buildingObject)
		{
			BuildingObject = buildingObject;
		}
	}
}
