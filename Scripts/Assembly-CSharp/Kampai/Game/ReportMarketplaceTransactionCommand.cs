using Ea.Sharkbite.HttpPlugin.Http.Api;
using Kampai.Common;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ReportMarketplaceTransactionCommand : Command
	{
		public class Report : IFastJSONSerializable
		{
			public int itemId;

			public int quantity;

			public int price;

			public bool buyTransaction;

			public void Serialize(JsonWriter writer)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("itemId");
				writer.WriteValue(itemId);
				writer.WritePropertyName("quantity");
				writer.WriteValue(quantity);
				writer.WritePropertyName("price");
				writer.WriteValue(price);
				writer.WritePropertyName("buyTransaction");
				writer.WriteValue(buyTransaction);
				writer.WriteEndObject();
			}
		}

		[Inject]
		public Instance<MarketplaceItemDefinition> item { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		public override void Execute()
		{
			if (!marketplaceService.isServerKillSwitchEnabled && !coppaService.Restricted())
			{
				UserSession userSession = userSessionService.UserSession;
				Report report = new Report();
				report.itemId = item.Definition.ItemID;
				MarketplaceBuyItem marketplaceBuyItem = item as MarketplaceBuyItem;
				if (marketplaceBuyItem != null)
				{
					report.quantity = marketplaceBuyItem.BuyQuantity;
					report.price = marketplaceBuyItem.BuyPrice;
					report.buyTransaction = true;
				}
				MarketplaceSaleItem marketplaceSaleItem = item as MarketplaceSaleItem;
				if (marketplaceSaleItem != null)
				{
					report.quantity = marketplaceSaleItem.QuantitySold;
					report.price = marketplaceSaleItem.SalePrice;
					report.buyTransaction = false;
				}
				downloadService.Perform(requestFactory.Resource(GameConstants.Marketplace.STAT_REPORTING_SERVER + "/rest/marketplace").WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
					.WithContentType("application/json")
					.WithMethod("POST")
					.WithEntity(report));
			}
		}
	}
}
