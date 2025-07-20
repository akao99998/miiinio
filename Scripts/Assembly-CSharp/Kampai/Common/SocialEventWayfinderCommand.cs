using System.Collections.Generic;
using Kampai.Game;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class SocialEventWayfinderCommand : Command
	{
		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		public override void Execute()
		{
			StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
			BuildingState state = firstInstanceByDefinitionId.State;
			if (state == BuildingState.Inactive || state == BuildingState.Construction || state == BuildingState.Disabled || state == BuildingState.Broken || state == BuildingState.Complete || state == BuildingState.Inaccessible)
			{
				return;
			}
			IList<int> pastEventsWithUnclaimedReward = timedSocialEventService.GetPastEventsWithUnclaimedReward();
			if (pastEventsWithUnclaimedReward.Count > 0)
			{
				createWayFinderSignal.Dispatch(new WayFinderSettings(firstInstanceByDefinitionId.ID));
			}
			else if (timedSocialEventService.GetCurrentSocialEvent() != null)
			{
				SocialTeamResponse socialEventStateCached = timedSocialEventService.GetSocialEventStateCached(timedSocialEventService.GetCurrentSocialEvent().ID);
				if (socialEventStateCached == null || socialEventStateCached.UserEvent == null || !socialEventStateCached.UserEvent.RewardClaimed || socialEventStateCached.Team == null || socialEventStateCached.Team.OrderProgress == null || socialEventStateCached.Team.OrderProgress.Count != timedSocialEventService.GetCurrentSocialEvent().Orders.Count)
				{
					createWayFinderSignal.Dispatch(new WayFinderSettings(firstInstanceByDefinitionId.ID));
				}
			}
		}
	}
}
