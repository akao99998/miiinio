using Kampai.Game.Mignette.WaterSlide.View;

namespace Kampai.Game.Mignette.WaterSlide
{
	public class WaterSlideMignetteSetupCommand : SetupMignetteManagerViewCommand
	{
		public override void Execute()
		{
			WaterSlideMignetteManagerView waterSlideMignetteManagerView = CreateManagerView<WaterSlideMignetteManagerView>("WaterSlideMignetteManagerView");
			base.contextView.transform.position = waterSlideMignetteManagerView.MignetteBuildingObject.transform.position;
			InitializeChildObjects(waterSlideMignetteManagerView);
		}
	}
}
