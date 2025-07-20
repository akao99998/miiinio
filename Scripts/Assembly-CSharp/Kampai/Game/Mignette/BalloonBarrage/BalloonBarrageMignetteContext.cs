using Kampai.Common;
using Kampai.Game.Mignette.BalloonBarrage.View;
using UnityEngine;

namespace Kampai.Game.Mignette.BalloonBarrage
{
	public class BalloonBarrageMignetteContext : MignetteContext
	{
		public BalloonBarrageMignetteContext()
		{
		}

		public BalloonBarrageMignetteContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override void mapBindings()
		{
			base.mapBindings();
			base.commandBinder.Bind<StartSignal>().To<SetupBalloonBarrageManagerViewCommand>();
			base.mediationBinder.Bind<BalloonBarrageMignetteManagerView>().To<BalloonBarrageMignetteManagerMediator>();
		}
	}
}
