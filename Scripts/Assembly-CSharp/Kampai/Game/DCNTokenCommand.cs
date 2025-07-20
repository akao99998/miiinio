using System;
using System.Text;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class DCNTokenCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DCNTokenCommand") as IKampaiLogger;

		[Inject]
		public IDCNService dcnService { get; set; }

		[Inject]
		public IConfigurationsService configurationService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			if (configurationService.isKillSwitchOn(KillSwitch.DCN))
			{
				logger.Info("DCN disabled by killswitch");
			}
			else
			{
				dcnService.Perform(Request, true);
			}
		}

		private IRequest Request()
		{
			string uri = string.Format("{0}{1}", GameConstants.DCN.SERVER, "/token");
			string s = string.Format("{{ \"{0}\": \"{1}\" }}", "app_token", GameConstants.DCN.APP_TOKEN);
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(Response);
			return requestFactory.Resource(uri).WithMethod("POST").WithContentType("application/json")
				.WithBody(bytes)
				.WithResponseSignal(signal);
		}

		private void Response(IResponse response)
		{
			if (response == null)
			{
				logger.Log(KampaiLogLevel.Error, "DCNTokenCommand response is null");
			}
			else if (!response.Success)
			{
				logger.Log(KampaiLogLevel.Error, string.Format("DCNTokenCommand failed with response code: {0}", response.Code));
			}
			else
			{
				Deserialize(response.Body);
			}
		}

		private void Deserialize(string json)
		{
			DCNToken dCNToken = null;
			try
			{
				dCNToken = JsonConvert.DeserializeObject<DCNToken>(json);
			}
			catch (JsonSerializationException e)
			{
				HandleJsonException(e);
			}
			catch (JsonReaderException e2)
			{
				HandleJsonException(e2);
			}
			if (dCNToken == null)
			{
				logger.Log(KampaiLogLevel.Error, "Error: token is null");
			}
			else
			{
				dcnService.SetToken(dCNToken);
			}
		}

		private void HandleJsonException(Exception e)
		{
			logger.Error("[Error]\n{0}", e.Message);
			logger.Error("[StackTrace]\n{0}", e.StackTrace);
		}
	}
}
