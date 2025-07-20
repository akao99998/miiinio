using Kampai.Common;
using Kampai.Game.Mignette.AlligatorSkiing.View;
using UnityEngine;

namespace Kampai.Game.Mignette.AlligatorSkiing
{
	public class AlligatorSkiingMignetteContext : MignetteContext
	{
		public AlligatorSkiingMignetteContext()
		{
		}

		public AlligatorSkiingMignetteContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override void mapBindings()
		{
			base.mapBindings();
			base.commandBinder.Bind<StartSignal>().To<AlligatorSkiingMignetteSetupCommand>();
			base.mediationBinder.Bind<AlligatorSkiingMignetteManagerView>().To<AlligatorSkiingMignetteManagerMediator>();
			injectionBinder.Bind<AlligatorMignettePathCompletedSignal>().ToSingleton();
			injectionBinder.Bind<AlligatorMignetteMinionHitObstacleSignal>().ToSingleton();
			injectionBinder.Bind<AlligatorMignetteMinionHitCollectableSignal>().ToSingleton();
			injectionBinder.Bind<AlligatorMignetteJumpLandedSignal>().ToSingleton();
		}
	}
}
