using Kampai.Game.Mignette.EdwardMinionHands.View;

namespace Kampai.Game.Mignette.EdwardMinionHands
{
	public class SetupEdwardMinionHandsManagerViewCommand : SetupMignetteManagerViewCommand
	{
		public override void Execute()
		{
			EdwardMinionHandsMignetteManagerView edwardMinionHandsMignetteManagerView = CreateManagerView<EdwardMinionHandsMignetteManagerView>("EdwardMinionHandsMignetteManagerView");
			base.contextView.transform.position = edwardMinionHandsMignetteManagerView.MignetteBuildingObject.transform.position;
			InitializeChildObjects(edwardMinionHandsMignetteManagerView);
		}
	}
}
