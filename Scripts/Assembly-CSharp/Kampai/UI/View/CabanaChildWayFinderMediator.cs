namespace Kampai.UI.View
{
	public class CabanaChildWayFinderMediator : AbstractChildWayFinderMediator
	{
		[Inject]
		public CabanaChildWayFinderView CabanaChildWayFinderView { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return CabanaChildWayFinderView;
			}
		}
	}
}
