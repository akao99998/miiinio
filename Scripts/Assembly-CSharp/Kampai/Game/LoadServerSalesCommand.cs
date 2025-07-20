using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class LoadServerSalesCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadServerSalesCommand") as IKampaiLogger;

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public ReconcileSalesSignal reconcileSalesSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			if (!coppaService.IsBirthdateKnown())
			{
				reconcileSalesSignal.Dispatch(0);
			}
			else
			{
				routineRunner.StartCoroutine(LoadFromServer());
			}
		}

		private IEnumerator LoadFromServer()
		{
			yield return null;
			logger.Info("ServerSales: Getting sales from server");
			UserSession session = userSessionService.UserSession;
			Signal<IResponse> responseSignal = new Signal<IResponse>();
			responseSignal.AddListener(OnDownloadComplete);
			string url = string.Format("{0}/rest/sales/{1}/v2", GameConstants.UpSell.SALES_SERVER, session.UserID);
			downloadService.Perform(requestFactory.Resource(url).WithHeaderParam("user_id", session.UserID).WithHeaderParam("session_key", session.SessionID)
				.WithResponseSignal(responseSignal));
		}

		private void OnDownloadComplete(IResponse response)
		{
			if (response.Success)
			{
				try
				{
					List<UserSale> list = null;
					using (StringReader reader = new StringReader(response.Body))
					{
						using (JsonTextReader reader2 = new JsonTextReader(reader))
						{
							list = ReaderUtil.PopulateList<UserSale>(reader2);
						}
					}
					IList<SalePackDefinition> list2 = new List<SalePackDefinition>();
					foreach (UserSale item in list)
					{
						SalePackDefinition salePackDefinition = FastJSONDeserializer.Deserialize<SalePackDefinition>(item.SaleDefinition);
						salePackDefinition.ServerSaleId = item.SaleId.ToString();
						list2.Add(salePackDefinition);
					}
					foreach (SalePackDefinition item2 in list2)
					{
						TransactionDefinition transactionDefinition = item2.TransactionDefinition.ToDefinition();
						if (definitionService.Has<SalePackDefinition>(item2.ID) || definitionService.Has<TransactionDefinition>(transactionDefinition.ID))
						{
							continue;
						}
						int uTCEndDate = item2.UTCEndDate;
						int num = timeService.CurrentTime();
						if (uTCEndDate > num)
						{
							definitionService.Add(item2);
							if (item2.TransactionDefinition != null)
							{
								definitionService.Add(transactionDefinition);
							}
							logger.Info("ServerSales - Added new SalePackDefinition ID = " + item2.ID);
						}
					}
				}
				catch (JsonSerializationException e)
				{
					HandleJsonException(e);
				}
				catch (JsonReaderException e2)
				{
					HandleJsonException(e2);
				}
			}
			else
			{
				logger.Error("ServerSales - Error downloading sales from server: {0}", response.Body ?? "null body");
			}
			reconcileSalesSignal.Dispatch(0);
		}

		private void HandleJsonException(Exception e)
		{
			logger.Error("ServerSales - Json exception: {0}", e);
		}
	}
}
