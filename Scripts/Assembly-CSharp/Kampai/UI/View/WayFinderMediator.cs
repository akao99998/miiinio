namespace Kampai.UI.View
{
	public class WayFinderMediator : AbstractWayFinderMediator
	{
		[Inject]
		public WayFinderView WayFinderView { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return WayFinderView;
			}
		}
	}
}
