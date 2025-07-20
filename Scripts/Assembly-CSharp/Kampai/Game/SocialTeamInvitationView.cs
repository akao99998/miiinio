using Newtonsoft.Json;

namespace Kampai.Game
{
	public class SocialTeamInvitationView
	{
		[JsonProperty("id")]
		public long TeamID { get; set; }

		public int SocialEventId { get; set; }

		public int MembersCount { get; set; }

		public int CompletedOrdersCount { get; set; }
	}
}
