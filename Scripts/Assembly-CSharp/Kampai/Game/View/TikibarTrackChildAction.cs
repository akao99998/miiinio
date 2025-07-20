using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class TikibarTrackChildAction : KampaiAction
	{
		private TikiBarBuildingObjectView target;

		private RuntimeAnimatorController controller;

		private CharacterObject child;

		private int routeIndex;

		private GetNewQuestSignal getNewQuestSignal;

		public TikibarTrackChildAction(TikiBarBuildingObjectView target, CharacterObject child, int routeIndex, RuntimeAnimatorController controller, GetNewQuestSignal getNewQuestSignal, IKampaiLogger logger)
			: base(logger)
		{
			this.controller = controller;
			this.target = target;
			this.child = child;
			this.routeIndex = routeIndex;
			this.getNewQuestSignal = getNewQuestSignal;
		}

		public override void Execute()
		{
			target.TrackChild(child, controller, routeIndex);
			getNewQuestSignal.Dispatch();
			base.Done = true;
		}
	}
}
