using System.Collections.Generic;

namespace Kampai.Game
{
	public class SocialTeamUserEvent
	{
		public bool RewardClaimed { get; set; }

		public IList<SocialEventInvitation> Invitations { get; set; }
	}
}
