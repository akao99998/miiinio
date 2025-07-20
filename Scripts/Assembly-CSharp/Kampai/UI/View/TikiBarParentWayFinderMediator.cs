namespace Kampai.UI.View
{
	public class TikiBarParentWayFinderMediator : AbstractParentWayFinderMediator
	{
		[Inject]
		public TikiBarParentWayFinderView TikiBarParentWayFinderView { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return TikiBarParentWayFinderView;
			}
		}
	}
}
