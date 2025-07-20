namespace Kampai.Game
{
	public class SocialTeamResponse
	{
		public int EventId { get; set; }

		public SocialTeam Team { get; set; }

		public SocialTeamUserEvent UserEvent { get; set; }

		public SocialEventError Error { get; set; }
	}
}
