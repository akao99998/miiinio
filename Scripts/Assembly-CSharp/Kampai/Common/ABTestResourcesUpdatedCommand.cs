using System.IO;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ABTestResourcesUpdatedCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("ABTestResourcesUpdatedCommand") as IKampaiLogger;

		[Inject]
		public bool resourcesUpdateSucceed { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public FetchDefinitionsSignal fetchDefinitionsSignal { get; set; }

		[Inject]
		public LoadDefinitionsSignal loadDefinitionsSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		public override void Execute()
		{
			TryFetchDefinitionsBasedOnCurrentABTest();
		}

		private void TryFetchDefinitionsBasedOnCurrentABTest()
		{
			logger.Debug("FetchDefinitionsBasedOnCurrentABTest(): resourcesUpdateSucceed = {0}", resourcesUpdateSucceed);
			string definitionsToFetchUrl;
			if (NeedFetchDefinitions(out definitionsToFetchUrl))
			{
				logger.Debug("FetchDefinitionsBasedOnCurrentABTest(): fetch definitions, definitionsToFetchUrl = {0}", definitionsToFetchUrl);
				configurationsService.GetConfigurations().definitions = definitionsToFetchUrl;
				fetchDefinitionsSignal.Dispatch(configurationsService.GetConfigurations());
			}
			else
			{
				logger.Debug("FetchDefinitionsBasedOnCurrentABTest(): definitions are up to date, load definitions.");
				LoadDefinitionsCommand.LoadDefinitionsData loadDefinitionsData = new LoadDefinitionsCommand.LoadDefinitionsData();
				loadDefinitionsData.Path = FetchDefinitionsCommand.GetDefinitionsPath();
				loadDefinitionsSignal.Dispatch(false, loadDefinitionsData);
			}
			logger.Debug("ABTestResourcesUpdatedCommand:TryFetchDefinitionsBasedOnCurrentABTest should be either fetching or loading definitions now");
		}

		private bool NeedFetchDefinitions(out string definitionsToFetchUrl)
		{
			string value = localPersistanceService.GetData("DefinitionsUrl");
			if (!File.Exists(FetchDefinitionsCommand.GetDefinitionsPath()))
			{
				value = null;
			}
			string finalDefinitionsUrl = GetFinalDefinitionsUrl();
			if (!string.IsNullOrEmpty(finalDefinitionsUrl) && !finalDefinitionsUrl.Equals(value))
			{
				definitionsToFetchUrl = finalDefinitionsUrl;
				return true;
			}
			definitionsToFetchUrl = null;
			return false;
		}

		private string GetFinalDefinitionsUrl()
		{
			string result = null;
			ConfigurationDefinition configurations = configurationsService.GetConfigurations();
			if (configurations != null && configurations.definitions != null)
			{
				result = configurations.definitions;
			}
			string aBTestDefinitionsUrl = GetABTestDefinitionsUrl();
			if (!string.IsNullOrEmpty(aBTestDefinitionsUrl))
			{
				result = aBTestDefinitionsUrl;
			}
			return result;
		}

		private string GetABTestDefinitionsUrl()
		{
			return (!ABTestModel.abtestEnabled) ? null : ((ABTestModel.definitionURL == null) ? string.Format("{0}/{1}", configurationsService.GetConfigurations().definitions, ABTestModel.configurationVariant) : ABTestModel.definitionURL);
		}
	}
}
