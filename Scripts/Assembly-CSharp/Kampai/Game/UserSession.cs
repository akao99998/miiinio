using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class UserSession
	{
		[JsonProperty("userId")]
		public string UserID { get; set; }

		[JsonProperty("sessionId")]
		public string SessionID { get; set; }

		[JsonProperty("synergyId")]
		public string SynergyID { get; set; }

		[JsonProperty("socialIdentities")]
		public IList<UserIdentity> SocialIdentities { get; set; }

		[JsonProperty("logEnabled")]
		public bool LogEnabled { get; set; }

		[JsonProperty("logLevel")]
		public int LogLevel { get; set; }
	}
}
