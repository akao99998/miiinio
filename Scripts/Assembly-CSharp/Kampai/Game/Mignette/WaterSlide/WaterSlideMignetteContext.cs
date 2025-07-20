using Kampai.Common;
using Kampai.Game.Mignette.WaterSlide.View;
using UnityEngine;

namespace Kampai.Game.Mignette.WaterSlide
{
	public class WaterSlideMignetteContext : MignetteContext
	{
		public WaterSlideMignetteContext()
		{
		}

		public WaterSlideMignetteContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override void mapBindings()
		{
			base.mapBindings();
			base.commandBinder.Bind<StartSignal>().To<WaterSlideMignetteSetupCommand>();
			base.mediationBinder.Bind<WaterSlideMignetteManagerView>().To<WaterSlideMignetteManagerMediator>();
			injectionBinder.Bind<WaterSlideMignetteJumpLandedSignal>().ToSingleton();
			injectionBinder.Bind<WaterSlideMignetteMinionHitObstacleSignal>().ToSingleton();
			injectionBinder.Bind<WaterSlideMignetteMinionHitCollectableSignal>().ToSingleton();
			injectionBinder.Bind<WaterSlideMignettePathCompletedSignal>().ToSingleton();
			injectionBinder.Bind<WaterslideMignetteOnDiveTriggerSignal>().ToSingleton();
			injectionBinder.Bind<WaterslideMignetteOnPlayDiveTriggerSignal>().ToSingleton();
		}
	}
}
