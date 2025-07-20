using System;
using Ea.Sharkbite.HttpPlugin.Http.Api;

namespace Kampai.Game
{
	public interface IDCNService
	{
		void Perform(Func<IRequest> request, bool isTokenRequest = false);

		void SetToken(DCNToken token);

		string GetToken();

		bool SetFeaturedContent(int featuredContentId, string htmlUrl);

		int GetFeaturedContentId();

		void OpenFeaturedContent(bool open);

		bool HasFeaturedContent();

		string GetLaunchURL();
	}
}
