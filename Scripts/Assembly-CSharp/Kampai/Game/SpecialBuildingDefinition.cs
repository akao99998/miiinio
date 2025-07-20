namespace Kampai.Game
{
	public class SpecialBuildingDefinition : BuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1061;
			}
		}

		public override Building BuildBuilding()
		{
			return new SpecialBuilding(this);
		}
	}
}
