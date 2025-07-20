namespace Kampai.Game
{
	public class LandExpansionBuildingDefinition : BuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1051;
			}
		}

		public override Building BuildBuilding()
		{
			return new LandExpansionBuilding(this);
		}
	}
}
