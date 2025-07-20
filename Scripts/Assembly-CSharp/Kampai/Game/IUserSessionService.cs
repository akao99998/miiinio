using Ea.Sharkbite.HttpPlugin.Http.Api;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public interface IUserSessionService
	{
		UserSession UserSession { get; set; }

		void LoginRequestCallback(IResponse response);

		void RegisterRequestCallback(IResponse response);

		void UserUpdateRequestCallback(string synergyID, IResponse response);

		void OpenURL(string url);

		void setLoginCallback(Signal a);
	}
}
