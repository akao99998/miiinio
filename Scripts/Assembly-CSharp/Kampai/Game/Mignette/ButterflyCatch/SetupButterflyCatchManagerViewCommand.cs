using Kampai.Game.Mignette.ButterflyCatch.View;

namespace Kampai.Game.Mignette.ButterflyCatch
{
	public class SetupButterflyCatchManagerViewCommand : SetupMignetteManagerViewCommand
	{
		public override void Execute()
		{
			ButterflyCatchMignetteManagerView butterflyCatchMignetteManagerView = CreateManagerView<ButterflyCatchMignetteManagerView>("ButterflyCatchMignetteManagerView");
			base.contextView.transform.position = butterflyCatchMignetteManagerView.MignetteBuildingObject.transform.position;
			InitializeChildObjects(butterflyCatchMignetteManagerView);
		}
	}
}
