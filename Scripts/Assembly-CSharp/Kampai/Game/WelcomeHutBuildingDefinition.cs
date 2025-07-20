namespace Kampai.Game
{
	public class WelcomeHutBuildingDefinition : RepairableBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1069;
			}
		}

		public override Building BuildBuilding()
		{
			return new WelcomeHutBuilding(this);
		}
	}
}
