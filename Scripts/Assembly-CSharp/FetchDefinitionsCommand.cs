using System.IO;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

public class FetchDefinitionsCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("FetchDefinitionsCommand") as IKampaiLogger;

	private Signal<IResponse> downloadResponseSignal = new Signal<IResponse>();

	private string definitionPath;

	[Inject]
	public ConfigurationDefinition config { get; set; }

	[Inject]
	public IDownloadService downloadService { get; set; }

	[Inject]
	public ILocalPersistanceService localPersistanceService { get; set; }

	[Inject]
	public DefinitionsFetchedSignal definitionsFetchedSignal { get; set; }

	[Inject]
	public LoadDefinitionsSignal loadDefinitionsSignal { get; set; }

	[Inject]
	public IRequestFactory requestFactory { get; set; }

	public override void Execute()
	{
		logger.EventStart("FetchDefinitionsCommand.Execute");
		if (ABTestModel.abtestEnabled && ABTestModel.definitionURL != null)
		{
			config.definitions = ABTestModel.definitionURL;
		}
		definitionPath = GetDefinitionsPath();
		downloadResponseSignal.AddListener(DownloadResponseHandler);
		logger.Error("FetchDefinitionsCommand:: Definitions URL: {0}", config.definitions);
		downloadService.Perform(requestFactory.Resource(config.definitions).WithOutputFile(definitionPath).WithGZip(true)
			.WithResponseSignal(downloadResponseSignal)
			.WithRetry()
			.WithAvoidBackup(true));
		logger.Debug("FetchDefinitionsCommand:Execute config.definitions=" + config.definitions);
	}

	public static string GetDefinitionsPath()
	{
		return Path.Combine(GameConstants.PERSISTENT_DATA_PATH, "definitions.json");
	}

	private void DownloadResponseHandler(IResponse response)
	{
		logger.Debug("FetchDefinitionsCommand:DownloadResponseHandler received response");
		if (!response.Success)
		{
			logger.FatalNoThrow(FatalCode.GS_ERROR_FETCH_DEFINITIONS, "GET {0} : status code {1}", response.Request.Uri, response.Code);
			return;
		}
		localPersistanceService.PutData("DefinitionsUrl", response.Request.Uri);
		logger.Debug("Load definitions");
		definitionsFetchedSignal.Dispatch();
		LoadDefinitionsCommand.LoadDefinitionsData loadDefinitionsData = new LoadDefinitionsCommand.LoadDefinitionsData();
		loadDefinitionsData.Path = GetDefinitionsPath();
		loadDefinitionsSignal.Dispatch(false, loadDefinitionsData);
		logger.EventStop("FetchDefinitionsCommand.Execute");
	}
}
