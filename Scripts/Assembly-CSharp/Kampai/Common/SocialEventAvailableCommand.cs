using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Common
{
	public class SocialEventAvailableCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialEventAvailableCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
			if (firstInstanceByDefinitionId == null)
			{
				return;
			}
			Prestige firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<Prestige>(40003);
			if (firstInstanceByDefinitionId2 == null)
			{
				return;
			}
			BuildingState state = firstInstanceByDefinitionId.State;
			if (state == BuildingState.Inactive || state == BuildingState.Construction || state == BuildingState.Disabled || state == BuildingState.Broken || state == BuildingState.Complete || state == BuildingState.Inaccessible || (firstInstanceByDefinitionId2.state != PrestigeState.Taskable && firstInstanceByDefinitionId2.state != PrestigeState.TaskableWhileQuesting))
			{
				return;
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			if (quantity < firstInstanceByDefinitionId.Definition.SocialEventMinimumLevel)
			{
				return;
			}
			ICrossContextInjectionBinder crossContextInjectionBinder = gameContext.injectionBinder;
			TimedSocialEventDefinition currentSocialEvent = timedSocialEventService.GetCurrentSocialEvent();
			if (currentSocialEvent != null)
			{
				crossContextInjectionBinder.GetInstance<SocialEventWayfinderSignal>().Dispatch();
			}
			else
			{
				IList<int> pastEventsWithUnclaimedReward = timedSocialEventService.GetPastEventsWithUnclaimedReward();
				if (pastEventsWithUnclaimedReward.Count > 0)
				{
					crossContextInjectionBinder.GetInstance<SocialEventWayfinderSignal>().Dispatch();
				}
			}
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(firstInstanceByDefinitionId.ID);
			StageBuildingObject stageBuildingObject = buildingObject as StageBuildingObject;
			if (stageBuildingObject != null && currentSocialEvent != null)
			{
				SocialTeamResponse socialEventStateCached = timedSocialEventService.GetSocialEventStateCached(timedSocialEventService.GetCurrentSocialEvent().ID);
				if (socialEventStateCached != null && socialEventStateCached.UserEvent != null && socialEventStateCached.UserEvent.RewardClaimed)
				{
					stageBuildingObject.UpdateStageState(BuildingState.Idle);
					return;
				}
				logger.Warning("Social Event is Available");
				stageBuildingObject.UpdateStageState(BuildingState.SocialAvailable);
				crossContextInjectionBinder.GetInstance<AddStuartToStageSignal>().Dispatch(StuartStageAnimationType.IDLEONSTAGE);
			}
		}
	}
}
