namespace Kampai.UI.View
{
	public class SpecialEventWayFinderMediator : AbstractQuestWayFinderMediator
	{
		[Inject]
		public SpecialEventWayFinderView SpecialEventWayFinderView { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return SpecialEventWayFinderView;
			}
		}
	}
}
