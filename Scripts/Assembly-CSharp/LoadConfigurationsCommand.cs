using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

public class LoadConfigurationsCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("LoadConfigurationsCommand") as IKampaiLogger;

	private Signal<IResponse> downloadResponse = new Signal<IResponse>();

	[Inject]
	public bool init { get; set; }

	[Inject]
	public IDownloadService downloadService { get; set; }

	[Inject]
	public IConfigurationsService ConfigurationsService { get; set; }

	[Inject]
	public ILocalPersistanceService localPersistService { get; set; }

	[Inject]
	public IRequestFactory requestFactory { get; set; }

	public override void Execute()
	{
		logger.EventStart("LoadConfigurationsCommand.Execute");
		logger.Info("Executing LoadConfigurationsCommand:{0}", init);
		TimeProfiler.StartSection("retrieve config");
		string configURL = ConfigurationsService.GetConfigURL();
		logger.Info("ClientConfigUrl: {0}", configURL);
		localPersistService.PutData("configURL", configURL);
		downloadResponse.AddListener(ConfigurationsService.GetConfigurationCallback);
		ConfigurationsService.setInitonCallback(init);
		downloadService.Perform(requestFactory.Resource(configURL).WithResponseSignal(downloadResponse).WithRetry());
		logger.EventStop("LoadConfigurationsCommand.Execute");
	}
}
