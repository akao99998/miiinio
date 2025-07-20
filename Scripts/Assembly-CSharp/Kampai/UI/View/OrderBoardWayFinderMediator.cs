namespace Kampai.UI.View
{
	public class OrderBoardWayFinderMediator : AbstractWayFinderMediator
	{
		[Inject]
		public OrderBoardWayFinderView OrderBoardWayFinderView { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return OrderBoardWayFinderView;
			}
		}
	}
}
