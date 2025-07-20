using Kampai.Game;

namespace Kampai.UI.View
{
	public class TSMTriggerWayFinderMediator : AbstractWayFinderMediator
	{
		[Inject]
		public TSMTriggerWayFinderView view { get; set; }

		[Inject]
		public TSMReachedDestinationSignal tsmReachedDestinationSignal { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return view;
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
			view.SetDestinationReached();
		}

		protected override void GoToClicked()
		{
			if (!base.pickModel.PanningCameraBlocked && !base.lairModel.goingToLair)
			{
				TSMCharacterSelectedSignal instance = base.GameContext.injectionBinder.GetInstance<TSMCharacterSelectedSignal>();
				instance.Dispatch();
			}
		}
	}
}
