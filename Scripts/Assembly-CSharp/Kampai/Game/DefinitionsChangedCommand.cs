using System;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.injector.api;

namespace Kampai.Game
{
	internal sealed class DefinitionsChangedCommand : Command
	{
		private const string lowestQualityName = "VeryLow";

		private IKampaiLogger logger = LogManager.GetClassLogger("DefinitionsChangedCommand") as IKampaiLogger;

		[Inject]
		public bool hotSwap { get; set; }

		[Inject]
		public IMinionBuilder minionBuilder { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public LoadPlayerSignal loadPlayerSignal { get; set; }

		[Inject]
		public LoadMarketplaceOverridesSignal loadMarketplaceOverridesSignal { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject]
		public DefinitionsHotSwapCompleteSignal definitionsHotSwapCompleteSignal { get; set; }

		[Inject]
		public DLCLevelChangedSignal dlcLevelChangedSignal { get; set; }

		public override void Execute()
		{
			configurationsService.GetConfigurations().definitions = localPersistanceService.GetData("DefinitionsUrl");
			logger.Error("DefinitionsChangedCommand:: Definitions URL: {0}", configurationsService.GetConfigurations().definitions);
			TargetPerformance lOD = (TargetPerformance)(int)Enum.Parse(typeof(TargetPerformance), dlcService.GetDownloadQualityLevel().ToUpper());
			minionBuilder.SetLOD(lOD);
			int num = -1;
			int num2 = 0;
			string[] names = QualitySettings.names;
			for (int i = 0; i < names.Length; i++)
			{
				string text = names[i];
				if (text.Equals(dlcService.GetDownloadQualityLevel(), StringComparison.OrdinalIgnoreCase))
				{
					num = i;
				}
				if (text.Equals("VeryLow", StringComparison.OrdinalIgnoreCase))
				{
					num2 = i;
				}
			}
			if (num < 0)
			{
				num = num2;
			}
			QualitySettings.SetQualityLevel(num);
			dlcLevelChangedSignal.Dispatch();
			if (!hotSwap)
			{
				loadPlayerSignal.Dispatch();
				loadMarketplaceOverridesSignal.Dispatch();
			}
			else
			{
				definitionsHotSwapCompleteSignal.Dispatch(base.injectionBinder as ICrossContextInjectionBinder);
			}
		}
	}
}
