using System.Collections.Generic;
using System.IO;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Main
{
	public class HindsightService : IHindsightService
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("HindsightService") as IKampaiLogger;

		private bool isInitialized;

		private HashSet<HindsightCampaignDefinition> campaignCache;

		private List<HindsightCampaignDefinition> campaignDefinitions;

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public IDefinitionService definitionSerivce { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject(MainElement.CONTEXT)]
		public ICrossContextCapable mainContext { get; set; }

		[Inject]
		public HindsightContentRequestSignal contentRequestSignal { get; set; }

		[Inject]
		public HindsightContentPreloadSignal hindsightContentPreloadSignal { get; set; }

		[Inject]
		public MainCompleteSignal mainCompleteSignal { get; set; }

		public void Initialize()
		{
			if (coppaService.Restricted())
			{
				logger.Info("Hindsight disabled by COPPA");
				return;
			}
			if (configurationsService.isKillSwitchOn(KillSwitch.HINDSIGHT))
			{
				logger.Info("Hindsight disabled by kill switch.");
				return;
			}
			campaignDefinitions = definitionSerivce.GetAll<HindsightCampaignDefinition>();
			if (campaignDefinitions == null)
			{
				logger.Warning("Hindsight Campaign Definitions is undefined (null)");
				return;
			}
			mainCompleteSignal.AddOnce(RegisterAppResume);
			campaignCache = new HashSet<HindsightCampaignDefinition>();
			isInitialized = true;
			ContentSync();
		}

		public void UpdateCache()
		{
			List<HindsightCampaign> instancesByType = playerService.GetInstancesByType<HindsightCampaign>();
			foreach (HindsightCampaign item in instancesByType)
			{
				if (!campaignCache.Contains(item.Definition))
				{
					campaignCache.Add(item.Definition);
				}
			}
		}

		public HindsightCampaign GetCachedContent(HindsightCampaign.Scope scope)
		{
			if (!isInitialized)
			{
				logger.Warning("HindsightService is not initialized");
				return null;
			}
			if (scope == HindsightCampaign.Scope.unknown)
			{
				logger.Error("Hindsight campaign scope is unknown");
				return null;
			}
			if (coppaService.Restricted())
			{
				logger.Info("Hindsight disabled by COPPA (scope = {0})", scope.ToString());
				return null;
			}
			if (configurationsService.isKillSwitchOn(KillSwitch.HINDSIGHT))
			{
				logger.Info("Hindsight disabled by kill switch.");
				return null;
			}
			if (campaignDefinitions == null || campaignDefinitions.Count == 0)
			{
				logger.Info("Hindsight campaign definition is undefined or contains no definitions");
				return null;
			}
			List<HindsightCampaignDefinition> list = campaignDefinitions.FindAll((HindsightCampaignDefinition c) => c.Scope == scope.ToString());
			if (list == null || list.Count == 0)
			{
				logger.Info("Hindsight campaign scope {0} is not found in definitions.", scope.ToString());
				return null;
			}
			string languageKey = localizationService.GetLanguageKey();
			foreach (HindsightCampaignDefinition item in list)
			{
				if (!campaignCache.Contains(item))
				{
					continue;
				}
				string contentCachePath = HindsightUtil.GetContentCachePath(item, languageKey);
				if (!File.Exists(contentCachePath))
				{
					continue;
				}
				int num = timeService.CurrentTime();
				if (item.UTCStartDate <= num && num <= item.UTCEndDate)
				{
					HindsightCampaign firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<HindsightCampaign>(item.ID);
					if (firstInstanceByDefinitionId != null && (item.Limit == -1 || firstInstanceByDefinitionId.ViewCount < item.Limit))
					{
						return firstInstanceByDefinitionId;
					}
				}
			}
			return null;
		}

		private void RegisterAppResume()
		{
			mainContext.injectionBinder.GetInstance<AppResumeCompletedSignal>().AddListener(ContentSync);
		}

		private void ContentSync()
		{
			if (!isInitialized)
			{
				logger.Error("Hindsight is not initalized while attempting to sync content");
				return;
			}
			if (Directory.Exists(GameConstants.IMAGE_PATH))
			{
				string languageKey = localizationService.GetLanguageKey();
				string[] files = Directory.GetFiles(GameConstants.IMAGE_PATH);
				foreach (string path in files)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
					if (string.IsNullOrEmpty(fileNameWithoutExtension))
					{
						continue;
					}
					string[] array = fileNameWithoutExtension.Split('_');
					int campaignId;
					if (!int.TryParse(array[0], out campaignId))
					{
						continue;
					}
					HindsightCampaignDefinition hindsightCampaignDefinition = campaignDefinitions.Find((HindsightCampaignDefinition c) => c.ID == campaignId);
					if (hindsightCampaignDefinition == null || !languageKey.Equals(array[1]))
					{
						File.Delete(path);
						HindsightCampaign firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<HindsightCampaign>(campaignId);
						if (firstInstanceByDefinitionId != null)
						{
							playerService.Remove(firstInstanceByDefinitionId);
						}
					}
					else
					{
						campaignCache.Add(hindsightCampaignDefinition);
						HindsightCampaign.Scope scope = HindsightUtil.GetScope(hindsightCampaignDefinition);
						hindsightContentPreloadSignal.Dispatch(scope);
					}
				}
			}
			foreach (HindsightCampaignDefinition campaignDefinition in campaignDefinitions)
			{
				if (!campaignCache.Contains(campaignDefinition))
				{
					contentRequestSignal.Dispatch(campaignDefinition);
				}
			}
		}
	}
}
