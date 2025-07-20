using Kampai.Common;
using Kampai.ForcedUpgrade.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.ForcedUpgrade
{
	public class ForcedUpgradeContext : BaseContext
	{
		public ForcedUpgradeContext()
		{
		}

		public ForcedUpgradeContext(MonoBehaviour view, bool autoStartup)
			: base(view, autoStartup)
		{
		}

		protected override void MapBindings()
		{
			injectionBinder.Bind<ILocalizationService>().To<HALService>().ToSingleton();
			injectionBinder.Bind<PlayGlobalSoundFXSignal>().ToSingleton();
			base.commandBinder.Bind<StartSignal>().To<ForcedUpgradeStartCommand>();
			base.commandBinder.Bind<InitLocalizationServiceSignal>().To<InitLocalizationServiceCommand>();
			base.mediationBinder.Bind<ForcedUpgradeView>().To<ForcedUpgradeMediator>();
		}
	}
}
