namespace Kampai.UI.View
{
	public class StageBuildingWayFinderMediator : AbstractWayFinderMediator
	{
		[Inject]
		public StageBuildingWayFinderView StageBuildingWayFinderView { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return StageBuildingWayFinderView;
			}
		}
	}
}
