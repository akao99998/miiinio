using System;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

public class UpdatePurchasedSalesCommand : Command
{
	private const string RESOURCE = "{0}/rest/sales/sale/{1}/incrementPurchasedSales";

	public IKampaiLogger logger = LogManager.GetClassLogger("UpdatePurchasedSalesCommand") as IKampaiLogger;

	[Inject]
	public string serverSaleId { get; set; }

	[Inject]
	public IDownloadService downloadService { get; set; }

	[Inject]
	public IRequestFactory requestFactory { get; set; }

	[Inject]
	public IUserSessionService userSessionService { get; set; }

	public override void Execute()
	{
		if (string.IsNullOrEmpty(serverSaleId))
		{
			logger.Log(KampaiLogLevel.Info, "Unable to update purchased sales: invalid serverSaleId was provided");
			return;
		}
		UserSession userSession = userSessionService.UserSession;
		string uri = string.Format("{0}/rest/sales/sale/{1}/incrementPurchasedSales", GameConstants.UpSell.SALES_SERVER, serverSaleId);
		Action<IResponse> callback = delegate(IResponse response)
		{
			OnResponse(response, serverSaleId);
		};
		Signal<IResponse> signal = new Signal<IResponse>();
		signal.AddListener(callback);
		downloadService.Perform(requestFactory.Resource(uri).WithMethod("POST").WithHeaderParam("user_id", userSession.UserID)
			.WithHeaderParam("session_key", userSession.SessionID)
			.WithContentType("application/json")
			.WithResponseSignal(signal));
	}

	private void OnResponse(IResponse response, string serverSaleId)
	{
		if (response.Success)
		{
			logger.Log(KampaiLogLevel.Info, string.Format("Updated purchased sales for sale {0}", serverSaleId));
		}
		else
		{
			logger.Log(KampaiLogLevel.Error, string.Format("Unable to update purchased sales for sale {0}: request failed with status code {1}", serverSaleId, response.Code));
		}
	}
}
