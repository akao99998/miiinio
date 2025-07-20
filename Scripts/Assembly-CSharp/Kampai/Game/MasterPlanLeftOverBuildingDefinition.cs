namespace Kampai.Game
{
	public class MasterPlanLeftOverBuildingDefinition : BuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1055;
			}
		}

		public override Building BuildBuilding()
		{
			return new MasterPlanLeftOverBuilding(this);
		}
	}
}
