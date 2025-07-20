using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Util
{
	public class ABTestCommand : Command
	{
		public class GameMetaData
		{
			public string configurationVariant { get; set; }

			public string definitionId { get; set; }

			public string definitionVariants { get; set; }

			public bool debugConsoleTest { get; set; }
		}

		public IKampaiLogger logger = LogManager.GetClassLogger("ABTestCommand") as IKampaiLogger;

		[Inject(MainElement.CONTEXT)]
		public ICrossContextCapable mainContext { get; set; }

		[Inject]
		public GameMetaData defData { get; set; }

		[Inject("cdn.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		public override void Execute()
		{
			ABTestModel.resetState();
			ABTestModel.abtestEnabled = true;
			ABTestModel.debugConsoleTest = defData.debugConsoleTest;
			if (defData.configurationVariant != null)
			{
				ABTestModel.configurationVariant = defData.configurationVariant;
			}
			else if (defData.definitionId != null)
			{
				ABTestModel.definitionURL = string.Format("{0}/rest/def/{1}/definitions", ServerUrl, defData.definitionId);
				if (defData.definitionVariants != null)
				{
					ABTestModel.definitionVariants = defData.definitionVariants;
					ABTestModel.definitionURL = string.Format("{0}/rest/def/{1}/definitions/{2}", ServerUrl, defData.definitionId, defData.definitionVariants);
				}
			}
			else if (defData.definitionVariants != null)
			{
				ABTestModel.definitionVariants = defData.definitionVariants;
				string text = configurationsService.GetConfigurations().definitions;
				if (text != null)
				{
					int num = text.LastIndexOf("/definitions");
					if (num != -1)
					{
						text = text.Substring(0, num);
					}
					ABTestModel.definitionURL = string.Format("{0}{1}/{2}", text, "/definitions", defData.definitionVariants);
				}
				else
				{
					logger.Error("ABTestCommand: unexpected null definitions in configuration. Skip setting of definitions URL for A/B testing");
				}
			}
			if (defData.debugConsoleTest)
			{
				mainContext.injectionBinder.GetInstance<ReloadGameSignal>().Dispatch();
			}
		}
	}
}
