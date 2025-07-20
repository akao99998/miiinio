using Kampai.Common;
using Kampai.Game.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class LandExpansionPickController : Command
	{
		[Inject]
		public int pickEvent { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public SelectLandExpansionSignal selectLandExpansionSignal { get; set; }

		public override void Execute()
		{
			switch (pickEvent)
			{
			case 1:
				break;
			case 3:
				PickEnd();
				break;
			case 2:
				break;
			}
		}

		private void PickEnd()
		{
			if (!(model.EndHitObject == null) && !(model.StartHitObject != model.EndHitObject) && !model.DetectedMovement)
			{
				LandExpansionBuildingObject component = model.EndHitObject.GetComponent<LandExpansionBuildingObject>();
				selectLandExpansionSignal.Dispatch(component.ID);
			}
		}
	}
}
