using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class ControllerBuildingAction : KampaiAction
	{
		private AnimatingBuildingObject target;

		private RuntimeAnimatorController controller;

		public ControllerBuildingAction(AnimatingBuildingObject target, RuntimeAnimatorController controller, IKampaiLogger logger)
			: base(logger)
		{
			this.controller = controller;
			this.target = target;
		}

		public override void Execute()
		{
			target.SetAnimController(controller);
			TaskableBuildingObject taskableBuildingObject = target as TaskableBuildingObject;
			if (taskableBuildingObject != null)
			{
				taskableBuildingObject.SetupLayers();
			}
			TikiBarBuildingObjectView tikiBarBuildingObjectView = target as TikiBarBuildingObjectView;
			if (tikiBarBuildingObjectView != null)
			{
				tikiBarBuildingObjectView.SetupLayers();
			}
			base.Done = true;
		}
	}
}
