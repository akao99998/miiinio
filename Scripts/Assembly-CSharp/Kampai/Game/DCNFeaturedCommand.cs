using System;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class DCNFeaturedCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DCNFeaturedCommand") as IKampaiLogger;

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		[Inject]
		public IDCNService dcnService { get; set; }

		[Inject]
		public ShowDCNScreenSignal showDCNScreenSignal { get; set; }

		public override void Execute()
		{
			dcnService.Perform(Request);
		}

		private IRequest Request()
		{
			string uri = string.Format("{0}{1}", GameConstants.DCN.SERVER, "/contents/featured");
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(Response);
			return requestFactory.Resource(uri).WithMethod("GET").WithHeaderParam("X-DCN-TOKEN", dcnService.GetToken())
				.WithResponseSignal(signal);
		}

		private void Response(IResponse response)
		{
			if (response == null)
			{
				logger.Log(KampaiLogLevel.Error, "DCNFeaturedCommand response is null");
			}
			else if (!response.Success)
			{
				logger.Log(KampaiLogLevel.Error, string.Format("DCNFeaturedCommand failed with response code: {0}", response.Code));
			}
			else
			{
				Deserialize(response.Body);
			}
		}

		private void Deserialize(string json)
		{
			DCNContent dCNContent = null;
			try
			{
				dCNContent = JsonConvert.DeserializeObject<DCNContent>(json);
			}
			catch (JsonSerializationException e)
			{
				HandleJsonException(e);
			}
			catch (JsonReaderException e2)
			{
				HandleJsonException(e2);
			}
			string value;
			if (dCNContent == null)
			{
				logger.Log(KampaiLogLevel.Error, "Error: content is null");
			}
			else if (dCNContent.Urls.TryGetValue("html5", out value) && !string.IsNullOrEmpty(value))
			{
				if (dcnService.SetFeaturedContent(dCNContent.Id, value))
				{
					showDCNScreenSignal.Dispatch(true);
				}
			}
			else
			{
				logger.Log(KampaiLogLevel.Warning, "HTML5 URL does not exist in the response!");
			}
		}

		private void HandleJsonException(Exception e)
		{
			logger.Error("[Error]\n{0}", e.Message);
			logger.Error("[StackTrace]\n{0}", e.StackTrace);
		}
	}
}
