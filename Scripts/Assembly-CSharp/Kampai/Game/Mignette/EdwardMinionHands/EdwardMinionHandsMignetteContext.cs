using Kampai.Common;
using Kampai.Game.Mignette.EdwardMinionHands.View;
using UnityEngine;

namespace Kampai.Game.Mignette.EdwardMinionHands
{
	public class EdwardMinionHandsMignetteContext : MignetteContext
	{
		public EdwardMinionHandsMignetteContext()
		{
		}

		public EdwardMinionHandsMignetteContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override void mapBindings()
		{
			base.mapBindings();
			base.commandBinder.Bind<StartSignal>().To<SetupEdwardMinionHandsManagerViewCommand>();
			base.mediationBinder.Bind<EdwardMinionHandsMignetteManagerView>().To<EdwardMinionHandsMignetteManagerMediator>();
		}
	}
}
