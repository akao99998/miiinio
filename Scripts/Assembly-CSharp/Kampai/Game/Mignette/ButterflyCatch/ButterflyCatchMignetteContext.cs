using Kampai.Common;
using Kampai.Game.Mignette.ButterflyCatch.View;
using UnityEngine;

namespace Kampai.Game.Mignette.ButterflyCatch
{
	public class ButterflyCatchMignetteContext : MignetteContext
	{
		public ButterflyCatchMignetteContext()
		{
		}

		public ButterflyCatchMignetteContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override void mapBindings()
		{
			base.mapBindings();
			base.commandBinder.Bind<StartSignal>().To<SetupButterflyCatchManagerViewCommand>();
			base.mediationBinder.Bind<ButterflyCatchMignetteManagerView>().To<ButterflyCatchMignetteManagerMediator>();
		}
	}
}
