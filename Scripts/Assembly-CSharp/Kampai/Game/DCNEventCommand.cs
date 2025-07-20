using System.Text;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class DCNEventCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DCNEventCommand") as IKampaiLogger;

		[Inject]
		public IDCNService dcnService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			dcnService.Perform(Request);
		}

		private IRequest Request()
		{
			string value = string.Format("/contents/{0}/events", dcnService.GetFeaturedContentId());
			string uri = new StringBuilder(GameConstants.DCN.SERVER).Append(value).ToString();
			string s = string.Format("{{ \"{0}\": \"{1}\" }}", "type", "display");
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(Response);
			IRequest result = requestFactory.Resource(uri).WithMethod("POST").WithContentType("application/json")
				.WithHeaderParam("X-DCN-TOKEN", dcnService.GetToken())
				.WithBody(bytes)
				.WithResponseSignal(signal);
			dcnService.SetFeaturedContent(-1, null);
			return result;
		}

		private void Response(IResponse response)
		{
			if (response == null)
			{
				logger.Log(KampaiLogLevel.Error, "DCNEventCommand response is null");
				return;
			}
			int code = response.Code;
			if (!response.Success)
			{
				logger.Log(KampaiLogLevel.Error, string.Format("DCNEventCommand failed with response code: {0}", code));
			}
			else if (response.Code != 204)
			{
				logger.Log(KampaiLogLevel.Error, string.Format("DCNEventCommand did not register event, response code: {0}", code));
			}
		}
	}
}
