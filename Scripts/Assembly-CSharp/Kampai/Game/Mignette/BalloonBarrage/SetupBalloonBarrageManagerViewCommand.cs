using Kampai.Game.Mignette.BalloonBarrage.View;

namespace Kampai.Game.Mignette.BalloonBarrage
{
	public class SetupBalloonBarrageManagerViewCommand : SetupMignetteManagerViewCommand
	{
		public override void Execute()
		{
			BalloonBarrageMignetteManagerView balloonBarrageMignetteManagerView = CreateManagerView<BalloonBarrageMignetteManagerView>("BalloonBarrageMignetteManagerView");
			base.contextView.transform.position = balloonBarrageMignetteManagerView.MignetteBuildingObject.transform.position;
			InitializeChildObjects(balloonBarrageMignetteManagerView);
		}
	}
}
