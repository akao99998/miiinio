namespace Kampai.UI.View
{
	public class TikiBarChildWayFinderMediator : AbstractChildWayFinderMediator
	{
		[Inject]
		public TikiBarChildWayFinderView TikiBarChildWayFinderView { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return TikiBarChildWayFinderView;
			}
		}
	}
}
