using System;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class CheckAvailableStorageCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("CheckAvailableStorageCommand") as IKampaiLogger;

		[Inject]
		public string path { get; set; }

		[Inject]
		public ulong requiredStorage { get; set; }

		[Inject]
		public Action availableCallback { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		public override void Execute()
		{
			ulong availableStorage = Native.GetAvailableStorage(path);
			if (availableStorage >= requiredStorage)
			{
				availableCallback();
				return;
			}
			logger.FatalNoThrow(FatalCode.EX_INSUFFICIENT_STORAGE, localizationService.GetString("InsufficientStorageMessage"), (ulong)((float)(requiredStorage - availableStorage) / 1024f / 1024f + 0.5f));
		}
	}
}
