namespace Kampai.UI.View
{
	public class MignetteWayFinderView : AbstractWayFinderView
	{
		protected override string UIName
		{
			get
			{
				return "MignetteWayFinder";
			}
		}

		protected override string WayFinderDefaultIcon
		{
			get
			{
				return wayFinderDefinition.MignetteDefaultIcon;
			}
		}

		protected override bool OnCanUpdate()
		{
			return true;
		}
	}
}
