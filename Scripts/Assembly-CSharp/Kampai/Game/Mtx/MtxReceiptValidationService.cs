using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game.Mtx
{
	public class MtxReceiptValidationService : IMtxReceiptValidationService
	{
		private string TAG = "[NCS] ";

		public IKampaiLogger logger = LogManager.GetClassLogger("MtxReceiptValidationService") as IKampaiLogger;

		private List<ReceiptValidationRequest> pendingReceiptValidationRequests;

		private ReceiptValidationRequest requestInProgress;

		[Inject]
		public StartMtxReceiptValidationSignal startMtxReceiptValidationSignal { get; set; }

		[Inject]
		public FinishMtxReceiptValidationSignal finishMtxReceiptValidationSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistence { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			pendingReceiptValidationRequests = LoadFromPersistence();
		}

		public void AddPendingReceipt(string sku, string nimbleTransactionId, string platformStoreTransactionId, IMtxReceipt receipt)
		{
			if (pendingReceiptValidationRequests.Find((ReceiptValidationRequest r) => r.nimbleTransactionId.Equals(nimbleTransactionId)) != null)
			{
				logger.Warning("{0}MtxReceiptValidationService.AddPendingReceipt() receipt for tr-n Id is already exist. No-op for sku {1}, nimble tr-n ID {2}", TAG, sku, nimbleTransactionId);
			}
			else
			{
				logger.Debug("{0}MtxReceiptValidationService.AddPendingReceipt: sku {1}, nimble tr-n ID {2}", TAG, sku, nimbleTransactionId);
				pendingReceiptValidationRequests.Add(new ReceiptValidationRequest(sku, nimbleTransactionId, platformStoreTransactionId, receipt));
				SaveToPersistence(pendingReceiptValidationRequests);
			}
		}

		public void ValidatePendingReceipt()
		{
			if (requestInProgress == null && pendingReceiptValidationRequests.Count > 0)
			{
				requestInProgress = pendingReceiptValidationRequests[0];
				logger.Debug("{0}MtxReceiptValidationService.ValidatePendingReceipt for sku {1}", TAG, requestInProgress.sku);
				startMtxReceiptValidationSignal.Dispatch(requestInProgress);
			}
		}

		public void ValidationResultCallback(ReceiptValidationResult result)
		{
			logger.Debug("{0}MtxReceiptValidationService.ValidationResultCallback for sku {1}", TAG, requestInProgress.sku);
			requestInProgress = null;
			finishMtxReceiptValidationSignal.Dispatch(result);
		}

		public void RemovePendingReceipt(string nimbleTransactionId)
		{
			logger.Debug("{0}MtxReceiptValidationService.RemovePendingReceipt: nimble tr-n ID {1}", TAG, nimbleTransactionId);
			pendingReceiptValidationRequests.RemoveAll((ReceiptValidationRequest r) => r.nimbleTransactionId.Equals(nimbleTransactionId));
			SaveToPersistence(pendingReceiptValidationRequests);
		}

		public bool HasPendingReceipts()
		{
			return pendingReceiptValidationRequests.Count > 0;
		}

		private void SaveToPersistence(List<ReceiptValidationRequest> pendingReceiptValidationRequests)
		{
			try
			{
				string data = JsonConvert.SerializeObject(pendingReceiptValidationRequests);
				localPersistence.PutData("MtxPendingReceipts", data);
			}
			catch (JsonSerializationException ex)
			{
				logger.Error("{0}SaveToPersistence(): Json Parse Err: {1}", TAG, ex);
			}
			catch (Exception ex2)
			{
				logger.Error("{0}SaveToPersistence(): error: {1}", TAG, ex2);
			}
		}

		private List<ReceiptValidationRequest> LoadFromPersistence()
		{
			List<ReceiptValidationRequest> list = null;
			string data = localPersistence.GetData("MtxPendingReceipts");
			if (data != null)
			{
				try
				{
					list = JsonConvert.DeserializeObject<List<ReceiptValidationRequest>>(data, new JsonConverter[1]
					{
						new MtxReceiptConverter()
					});
				}
				catch (JsonSerializationException e)
				{
					HandleLoadJsonException(e);
				}
				catch (JsonReaderException e2)
				{
					HandleLoadJsonException(e2);
				}
				catch (Exception ex)
				{
					logger.Error("{0}SaveToPersistence(): error: {1}", TAG, ex);
				}
			}
			return list ?? new List<ReceiptValidationRequest>();
		}

		private void HandleLoadJsonException(Exception e)
		{
			logger.Error("{0}LoadFromPersistence(): Json Parse Err: {1}", TAG, e);
		}
	}
}
