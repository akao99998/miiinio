using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Main
{
	public class HindsightContentRequestCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("HindsightContentRequestCommand") as IKampaiLogger;

		private Signal<IResponse> responseSignal = new Signal<IResponse>();

		[Inject]
		public HindsightCampaignDefinition definition { get; set; }

		[Inject]
		public IHindsightService hindsightService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public HindsightContentPreloadSignal hindsightContentPreloadSignal { get; set; }

		public override void Execute()
		{
			if (definition.Content == null)
			{
				logger.Error("Hindsight Campaign Content is null");
				return;
			}
			if (!HindsightUtil.ValidPlatform(definition))
			{
				logger.Info("Invalid Platform {0} for campaign {1}", definition.Platform, definition.ID);
				return;
			}
			string languageKey = localizationService.GetLanguageKey();
			string contentUri = HindsightUtil.GetContentUri(definition, languageKey);
			if (string.IsNullOrEmpty(contentUri))
			{
				logger.Error("Unable to find uri for language key: {0}", languageKey);
				return;
			}
			string contentCachePath = HindsightUtil.GetContentCachePath(definition, languageKey);
			if (string.IsNullOrEmpty(contentCachePath))
			{
				logger.Error("Invalid cache path for language: {0}", languageKey);
			}
			else
			{
				responseSignal.AddOnce(OnResponse);
				IRequest request = requestFactory.Resource(contentUri).WithOutputFile(contentCachePath).WithGZip(true)
					.WithUdp(true)
					.WithResume(true)
					.WithRetry()
					.WithResponseSignal(responseSignal);
				downloadService.Perform(request);
			}
		}

		private void OnResponse(IResponse response)
		{
			if (response.Success)
			{
				HindsightCampaign i = definition.Build() as HindsightCampaign;
				playerService.Add(i);
				hindsightService.UpdateCache();
				HindsightCampaign.Scope scope = HindsightUtil.GetScope(definition);
				hindsightContentPreloadSignal.Dispatch(scope);
			}
		}
	}
}
