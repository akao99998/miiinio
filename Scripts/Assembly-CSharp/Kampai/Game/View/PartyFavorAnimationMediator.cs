using Elevation.Logging;
using Kampai.Util;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class PartyFavorAnimationMediator : EventMediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("PartyFavorAnimationMediator") as IKampaiLogger;

		[Inject]
		public PartyFavorAnimationView view { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChangeSignal { get; set; }

		[Inject]
		public IBuildingUtilities buildingUtilies { get; set; }

		[Inject]
		public PartyFavorTrackChildSignal trackChildSignal { get; set; }

		[Inject]
		public PartyFavorFreeAllMinionsSignal freeMinionsSignal { get; set; }

		public override void OnRegister()
		{
			view.SetupInjections(logger, minionStateChangeSignal, buildingUtilies);
			trackChildSignal.AddListener(TrackChild);
			freeMinionsSignal.AddListener(FreeAllMinions);
		}

		public override void OnRemove()
		{
			trackChildSignal.RemoveListener(TrackChild);
			freeMinionsSignal.RemoveListener(FreeAllMinions);
		}

		private void TrackChild(int ID, MinionObject minion)
		{
			if (ID == view.PartyFavorDefinition.ID)
			{
				view.TrackChild(minion);
			}
		}

		private void FreeAllMinions(int ID)
		{
			if (ID == view.PartyFavorDefinition.ID)
			{
				view.FreeAllMinions();
			}
		}
	}
}
