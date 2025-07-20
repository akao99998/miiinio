using System;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ConfigurationsLoadedCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ConfigurationsLoadedCommand") as IKampaiLogger;

		[Inject]
		public bool init { get; set; }

		[Inject]
		public IInvokerService invoker { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public DownloadManifestSignal downloadManifestSignal { get; set; }

		[Inject]
		public ILocalPersistanceService persistService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CheckUpgradeSignal checkUpgradeSignal { get; set; }

		[Inject]
		public DeviceInformation deviceInformation { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IConfigurationsService configurationService { get; set; }

		public override void Execute()
		{
			ConfigurationDefinition configDef = configurationService.GetConfigurations();
			LogManager.Instance.SetConfig(configDef.loggingConfig);
			logger.EventStart("ConfigurationsLoadedCommand.Execute");
			invoker.Add(delegate
			{
				if (init)
				{
					telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("20 - Loaded Config", playerService.SWRVEGroup, dlcService.GetDownloadQualityLevel());
					TargetPerformance targetPerformance = configDef.targetPerformance;
					if (targetPerformance == TargetPerformance.UNKNOWN)
					{
						targetPerformance = new DeviceCapabilities().GetTargetPerformance(logger, Application.platform, deviceInformation);
					}
					if (targetPerformance < TargetPerformance.MED)
					{
						Shader.globalMaximumLOD = 100;
					}
					string text = dlcService.GetDisplayQualityLevel();
					if (text == null || text.Equals(string.Empty))
					{
						text = QualityHelper.getStartingQuality(targetPerformance);
						dlcService.SetDisplayQualityLevel(text);
					}
					TargetPerformance targetPerformance2 = QualityHelper.getCurrentTarget(targetPerformance, text);
					string value = persistService.GetData("FORCE_LOD");
					if (!string.IsNullOrEmpty(value))
					{
						targetPerformance2 = (TargetPerformance)(int)Enum.Parse(typeof(TargetPerformance), value);
					}
					logger.Debug("DLC quality to download = {0}", targetPerformance2.ToString());
					dlcService.SetDownloadQualityLevel(targetPerformance2);
					definitionService.SetPerformanceQualityLevel(targetPerformance2);
					downloadManifestSignal.Dispatch();
					logger.EventStop("ConfigurationsLoadedCommand.Execute");
				}
				checkUpgradeSignal.Dispatch();
			});
		}
	}
}
