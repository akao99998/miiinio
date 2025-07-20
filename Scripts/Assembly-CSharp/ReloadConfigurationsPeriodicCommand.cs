using System.Collections;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

public class ReloadConfigurationsPeriodicCommand : Command
{
	private static int CONFIG_RELOAD_INTERVAL = 120;

	public IKampaiLogger logger = LogManager.GetClassLogger("ReloadConfigurationsPeriodicCommand") as IKampaiLogger;

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public LoadConfigurationSignal loadConfigurationSignal { get; set; }

	public override void Execute()
	{
		logger.Debug("Scheduling reload of configs");
		routineRunner.StartCoroutine(PeriodicReloadConfigs());
	}

	private IEnumerator PeriodicReloadConfigs()
	{
		while (true)
		{
			yield return new WaitForSeconds(CONFIG_RELOAD_INTERVAL);
			logger.Info("ReloadConfigurationsPeriodicCommand: Reloading configurations!");
			loadConfigurationSignal.Dispatch(false);
		}
	}
}
