namespace Kampai.Game
{
	public class SocialEventError
	{
		public enum ErrorType
		{
			TEAM_FULL = 0,
			ORDER_ALREADY_FILLED = 1
		}

		public ErrorType Type { get; set; }
	}
}
