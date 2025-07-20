using System;
using System.IO;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Main
{
	public class DownloadManifestCommand : Command
	{
		private const string REST_DLC_MANIFEST = "manifests";

		private IKampaiLogger logger = LogManager.GetClassLogger("DownloadManifestCommand") as IKampaiLogger;

		[Inject]
		public IConfigurationsService configService { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public IInvokerService invoker { get; set; }

		[Inject]
		public PostDownloadManifestSignal postSignal { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			TimeProfiler.StartSection("retrieve manifest");
			logger.EventStart("DownloadManifestCommand.Execute");
			string text = dlcService.GetDownloadQualityLevel();
			if (string.IsNullOrEmpty(text))
			{
				text = TargetPerformance.LOW.ToString().ToLower();
			}
			ConfigurationDefinition configurations = configService.GetConfigurations();
			logger.Debug("Downloading {0} Manifest ", text);
			logger.Debug(configurations.ToString());
			if (configurations.dlcManifests == null || configurations.dlcManifests.Count == 0)
			{
				logger.Fatal(FatalCode.GS_ERROR_MISSING_DLC);
				return;
			}
			string text2 = configService.GetConfigurations().dlcManifests[text];
			bool flag = false;
			if (File.Exists(GameConstants.RESOURCE_MANIFEST_PATH))
			{
				try
				{
					ManifestObject manifestObject = FastJSONDeserializer.DeserializeFromFile<ManifestObject>(GameConstants.RESOURCE_MANIFEST_PATH);
					if (!text2.Contains("manifests") || !text2.StartsWith("http"))
					{
						logger.Fatal(FatalCode.GS_ERROR_BAD_MANIFEST, text2);
						return;
					}
					string text3 = text2.Substring(text2.LastIndexOf('/') + 1, text2.LastIndexOf('.') - text2.LastIndexOf('/') - 1);
					logger.Debug("MANIFEST: {0}, existing: {1}", text3, manifestObject.id);
					if (text3 == manifestObject.id)
					{
						flag = true;
					}
				}
				catch (Exception ex)
				{
					logger.Debug("Exception in deserializing manifest = {0}", ex.ToString());
				}
			}
			if (flag)
			{
				logger.Debug("Manifest Exists No need to download it");
				postSignal.Dispatch();
			}
			else
			{
				logger.Debug("requested manifest does not exist, fetching it");
				Signal<IResponse> signal = new Signal<IResponse>();
				signal.AddListener(ReceivedManifestCallback);
				downloadService.Perform(requestFactory.Resource(text2).WithOutputFile(GameConstants.RESOURCE_MANIFEST_PATH).WithGZip(true)
					.WithResponseSignal(signal)
					.WithRetry());
			}
			logger.EventStop("DownloadManifestCommand.Execute");
		}

		private void ReceivedManifestCallback(IResponse response)
		{
			invoker.Add(delegate
			{
				if (!response.Success)
				{
					logger.Fatal(FatalCode.GS_ERROR_DOWNLOAD_MANIFEST, "Unable to download manifest");
				}
				else
				{
					postSignal.Dispatch();
				}
			});
		}
	}
}
