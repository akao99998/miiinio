using Newtonsoft.Json;

namespace Kampai.Game
{
	public class SocialEventInvitation
	{
		[JsonProperty("eventId")]
		public int EventID { get; set; }

		[JsonProperty("team")]
		public SocialTeamInvitationView Team { get; set; }

		[JsonProperty("inviter")]
		public UserIdentity inviter { get; set; }
	}
}
