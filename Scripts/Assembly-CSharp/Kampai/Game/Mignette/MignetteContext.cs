using Kampai.Common;
using UnityEngine;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.context.impl;

namespace Kampai.Game.Mignette
{
	public class MignetteContext : MVCSContext
	{
		public MignetteContext()
		{
		}

		public MignetteContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override void addCoreComponents()
		{
			base.addCoreComponents();
			injectionBinder.Unbind<ICommandBinder>();
			injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
		}

		public override void Launch()
		{
			base.Launch();
			injectionBinder.GetInstance<StartSignal>().Dispatch();
		}

		protected override void mapBindings()
		{
			base.mapBindings();
			base.commandBinder.Bind<ChangeMignetteScoreSignal>().To<ChangeCurrentMignetteScore>();
			base.commandBinder.Bind<DestroyMignetteContextSignal>().To<DestroyMignetteContextCommand>();
		}
	}
}
