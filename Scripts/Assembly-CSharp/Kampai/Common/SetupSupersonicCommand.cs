using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class SetupSupersonicCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("SetupSupersonicCommand") as IKampaiLogger;

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public ISupersonicService supersonicService { get; set; }

		[Inject(MainElement.MANAGER_PARENT)]
		public GameObject managers { get; set; }

		public override void Execute()
		{
			if (configurationsService.isKillSwitchOn(KillSwitch.SUPERSONIC))
			{
				logger.Info("Supersonic is disabled by kill switch.");
				return;
			}
			GameObject gameObject = new GameObject("Supersonic");
			gameObject.transform.parent = managers.transform;
			logger.Debug("SetupSupersonicCommand.Execute supersonic object added to scene");
			gameObject.AddComponent<SupersonicEvents>();
			supersonicService.Init();
		}
	}
}
