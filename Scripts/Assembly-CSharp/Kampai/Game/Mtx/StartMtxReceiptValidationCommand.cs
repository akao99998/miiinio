using System;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.command.impl;

namespace Kampai.Game.Mtx
{
	public class StartMtxReceiptValidationCommand : Command
	{
		public const string VALIDATION_ENDPOINT_APPLE_APPSTORE = "/rest/transaction/verify/apple";

		public const string VALIDATION_ENDPOINT_GOOGLE_PLAY = "/rest/transaction/verify/google";

		public IKampaiLogger logger = LogManager.GetClassLogger("StartMtxReceiptValidationCommand") as IKampaiLogger;

		[Inject]
		public ReceiptValidationRequest request { get; set; }

		[Inject]
		public IMtxReceiptValidationService receiptValidationService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject("game.server.host")]
		public string serverUrl { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			string userId = GetUserId();
			if (string.IsNullOrEmpty(userId))
			{
				logger.Error("[NCS] StartMtxReceiptValidationCommand.Execute(): unable to validate receipt: user is unknown at the moment.");
				ReceiptValidationResult result = new ReceiptValidationResult(request.sku, request.nimbleTransactionId, request.platformStoreTransactionId, ReceiptValidationResult.Code.VALIDATION_UNAVAILABLE);
				receiptValidationService.ValidationResultCallback(result);
			}
			else if (!CreateAndSendHttpRequest(userId, request))
			{
				logger.Error("[NCS] StartMtxReceiptValidationCommand.Execute(): can't prepare validation request. Possible reason: unsupported receipt type(Amazon for example) or invalid receipt.");
				ReceiptValidationResult result2 = new ReceiptValidationResult(request.sku, request.nimbleTransactionId, request.platformStoreTransactionId, ReceiptValidationResult.Code.RECEIPT_INVALID);
				receiptValidationService.ValidationResultCallback(result2);
			}
			else
			{
				Retain();
			}
		}

		private void OnHttpResponse(IResponse response)
		{
			ReceiptValidationResult.Code code = ReceiptValidationResult.Code.VALIDATION_UNAVAILABLE;
			if (response.Success)
			{
				code = ReceiptValidationResult.Code.SUCCESS;
			}
			else
			{
				logger.Error("[NCS] StartMtxReceiptValidationCommand.Execute(): validation request failure. Response status code: {0}, response error msg: {1}", response.Code, response.Body ?? "null");
				ServerValidationError validationErrorFromHttpBody = GetValidationErrorFromHttpBody(response.Body);
				if (validationErrorFromHttpBody != null)
				{
					logger.Error("[NCS] StartMtxReceiptValidationCommand.Execute(): validation request failure. Logical server error: {0}", validationErrorFromHttpBody.code);
					switch (validationErrorFromHttpBody.code)
					{
					case ServerValidationError.Code.RECEIPT_DUPLICATE:
						code = ReceiptValidationResult.Code.RECEIPT_DUPLICATE;
						break;
					case ServerValidationError.Code.RECEIPT_INVALID:
						code = ReceiptValidationResult.Code.RECEIPT_INVALID;
						break;
					case ServerValidationError.Code.VALIDATION_UNAVAILABLE:
						code = ReceiptValidationResult.Code.VALIDATION_UNAVAILABLE;
						break;
					default:
						logger.Error("[NCS] StartMtxReceiptValidationCommand.Execute(): unsupported server error code: {0}", validationErrorFromHttpBody.code);
						code = ReceiptValidationResult.Code.VALIDATION_UNAVAILABLE;
						break;
					}
				}
				else
				{
					logger.Error("[NCS] StartMtxReceiptValidationCommand.Execute(): can't extract logical server error from response body");
				}
			}
			ReceiptValidationResult result = new ReceiptValidationResult(request.sku, request.nimbleTransactionId, request.platformStoreTransactionId, code);
			receiptValidationService.ValidationResultCallback(result);
			Release();
		}

		private ServerValidationError GetValidationErrorFromHttpBody(string body)
		{
			if (string.IsNullOrEmpty(body))
			{
				logger.Error("[NCS] StartMtxReceiptValidationCommand.GetValidationErrorFromHttpBody(): null http response body");
				return null;
			}
			ServerValidationError result = null;
			try
			{
				ServerValidationErrorResponse serverValidationErrorResponse = JsonConvert.DeserializeObject<ServerValidationErrorResponse>(body);
				result = ((serverValidationErrorResponse == null) ? null : serverValidationErrorResponse.error);
			}
			catch (JsonSerializationException e)
			{
				HandleJsonException(e);
			}
			catch (JsonReaderException e2)
			{
				HandleJsonException(e2);
			}
			return result;
		}

		private void HandleJsonException(Exception e)
		{
			logger.Error("[NCS] StartMtxReceiptValidationCommand.GetValidationErrorFromHttpBody(): ServerValidationError deserialization error {0}", e.Message);
		}

		private string GetUserId()
		{
			string text = null;
			UserSession userSession = userSessionService.UserSession;
			if (userSession != null && !string.IsNullOrEmpty(userSession.UserID))
			{
				text = userSession.UserID;
				logger.Info("[NCS] StartMtxReceiptValidationCommand.GetUserId(): use user ID from user session: {0}", text);
			}
			if (text == null)
			{
				string text2 = localPersistService.GetData("UserID");
				if (!string.IsNullOrEmpty(text2))
				{
					text = text2;
					logger.Info("[NCS] StartMtxReceiptValidationCommand.GetUserId(): use user ID from persistance: {0}.", text);
				}
			}
			return text;
		}

		private bool CreateAndSendHttpRequest(string userId, ReceiptValidationRequest request)
		{
			IMtxReceipt receipt = request.receipt;
			if (receipt == null)
			{
				logger.Error("[NCS] StartMtxReceiptValidationCommand.CreateHttpRequest(): null receipt");
				return false;
			}
			if (receipt is GooglePlayReceipt)
			{
				GooglePlayReceipt googlePlayReceipt = receipt as GooglePlayReceipt;
				GooglePlayReceiptValidationRequest googlePlayReceiptValidationRequest = new GooglePlayReceiptValidationRequest();
				googlePlayReceiptValidationRequest.userId = userId;
				googlePlayReceiptValidationRequest.signedData = googlePlayReceipt.signedData;
				googlePlayReceiptValidationRequest.signature = googlePlayReceipt.signature;
				GooglePlayReceiptValidationRequest googlePlayReceiptValidationRequest2 = googlePlayReceiptValidationRequest;
				string url = serverUrl + "/rest/transaction/verify/google";
				logger.Debug("[NCS] CreateAndSendHttpRequest(): userId: {0}, signedData: {1}, signature: {2}", googlePlayReceiptValidationRequest2.userId, googlePlayReceiptValidationRequest2.signedData, googlePlayReceiptValidationRequest2.signature);
				SendHttpRequest(url, googlePlayReceiptValidationRequest2);
				return true;
			}
			logger.Error("[NCS] StartMtxReceiptValidationCommand.CreateHttpRequest(): unsupported receipt type: {0}", receipt);
			return false;
		}

		private void SendHttpRequest(string url, object body)
		{
			DownloadResponseSignal downloadResponseSignal = new DownloadResponseSignal();
			downloadResponseSignal.AddListener(OnHttpResponse);
			downloadService.Perform(requestFactory.Resource(url).WithHeaderParam("user_id", userSessionService.UserSession.UserID).WithHeaderParam("session_key", userSessionService.UserSession.SessionID)
				.WithMethod("POST")
				.WithEntity(body)
				.WithContentType("application/json")
				.WithResponseSignal(downloadResponseSignal));
		}
	}
}
