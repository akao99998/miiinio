using System;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public interface ISocialService
	{
		string userID { get; }

		string userName { get; }

		bool isLoggedIn { get; }

		string accessToken { get; }

		DateTime tokenExpiry { get; }

		SocialServices type { get; }

		string LoginSource { get; set; }

		bool isKillSwitchEnabled { get; }

		string locKey { get; }

		void Init(Signal<ISocialService> successSignal, Signal<ISocialService> failureSignal);

		void Login(Signal<ISocialService> successSignal, Signal<ISocialService> failureSignal, Action callback);

		void Logout();

		void SendLoginTelemetry(string loginLocation);

		void updateKillSwitchFlag();

		void incrementAchievement(string achievementID, float percentComplete);

		void ShowAchievements();
	}
}
