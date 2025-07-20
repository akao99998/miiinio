using Ea.Sharkbite.HttpPlugin.Http.Api;
using strange.extensions.signal.impl;

namespace Kampai.Splash
{
	public interface IDownloadService
	{
		void Perform(IRequest request, bool forceRequest = false);

		void AddGlobalResponseListener(Signal<IResponse> response);

		void ProcessQueue();

		void Shutdown();

		void Abort();

		void Restart();
	}
}
