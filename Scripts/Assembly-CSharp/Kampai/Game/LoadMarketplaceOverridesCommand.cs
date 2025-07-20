using System.IO;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class LoadMarketplaceOverridesCommand : Command
	{
		public class MarketplaceOverrides
		{
			public MarketplaceDefinition marketplaceDefinition;
		}

		public IKampaiLogger logger = LogManager.GetClassLogger("LoadMarketplaceOverridesCommand") as IKampaiLogger;

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			if (!marketplaceService.isServerKillSwitchEnabled && !coppaService.Restricted())
			{
				logger.Info("Loading marketplace overrides");
				Signal<IResponse> signal = new Signal<IResponse>();
				signal.AddListener(OnDownloadComplete);
				string uri = Path.Combine(GameConstants.Marketplace.OVERRIDES_SERVER, Path.Combine("marketplace", "marketplace.json"));
				downloadService.Perform(requestFactory.Resource(uri).WithResponseSignal(signal));
			}
		}

		private void OnDownloadComplete(IResponse response)
		{
			if (response.Success)
			{
				try
				{
					MarketplaceOverrides marketplaceOverrides = JsonConvert.DeserializeObject<MarketplaceOverrides>(response.Body);
					if (marketplaceOverrides != null)
					{
						MarketplaceDefinition marketplaceDefinition = marketplaceOverrides.marketplaceDefinition;
						if (marketplaceDefinition != null)
						{
							foreach (MarketplaceItemDefinition itemDefinition2 in marketplaceDefinition.itemDefinitions)
							{
								MarketplaceItemDefinition itemDefinition;
								if (marketplaceService.GetItemDefinitionByItemID(itemDefinition2.ItemID, out itemDefinition))
								{
									itemDefinition.StartingStrikePrice = itemDefinition2.StartingStrikePrice;
									itemDefinition.MaxStrikePrice = itemDefinition2.MaxStrikePrice;
									itemDefinition.MinStrikePrice = itemDefinition2.MinStrikePrice;
									itemDefinition.FloorPrice = itemDefinition2.FloorPrice;
									itemDefinition.CeilingPrice = itemDefinition2.CeilingPrice;
									itemDefinition.ProbabilityWeight = itemDefinition2.ProbabilityWeight;
									itemDefinition.HighPriceBuyTimeSeconds = itemDefinition2.HighPriceBuyTimeSeconds;
									itemDefinition.LowPriceBuyTimeSeconds = itemDefinition2.LowPriceBuyTimeSeconds;
									itemDefinition.PriceTrend = itemDefinition2.PriceTrend;
								}
								else
								{
									logger.Info("Marketplace overrides: invalid itemID downloaded: {0}", itemDefinition2.ItemID.ToString());
								}
							}
							return;
						}
					}
					return;
				}
				catch (JsonSerializationException ex)
				{
					logger.Info("Marketplace overrides JsonSerializationException: {0}", ex.Message);
					return;
				}
				catch (JsonReaderException ex2)
				{
					logger.Info("Marketplace overrides JsonReaderException: {0}", ex2.Message);
					return;
				}
			}
			logger.Error("Error downloading marketplace overrides");
		}
	}
}
