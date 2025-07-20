using Kampai.Game;

namespace Kampai.UI.View
{
	public class TSMWayFinderMediator : AbstractQuestWayFinderMediator
	{
		[Inject]
		public TSMWayFinderView TSMWayFinderView { get; set; }

		[Inject]
		public TSMReachedDestinationSignal tsmReachedDestinationSignal { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return TSMWayFinderView;
			}
		}

		public override void OnRegister()
		{
			base.OnRegister();
			tsmReachedDestinationSignal.AddListener(tsmReachedDestination);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			tsmReachedDestinationSignal.RemoveListener(tsmReachedDestination);
		}

		private void tsmReachedDestination()
		{
			TSMWayFinderView.SetDestinationReached();
		}
	}
}
