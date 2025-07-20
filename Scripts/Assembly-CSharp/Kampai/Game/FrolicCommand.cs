using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class FrolicCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("FrolicCommand") as IKampaiLogger;

		[Inject]
		public int CharacterID { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IdleInTownHallSignal idleInTownHallSignal { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		public override void Execute()
		{
			FrolicCharacter byInstanceId = playerService.GetByInstanceId<FrolicCharacter>(CharacterID);
			if (byInstanceId != null)
			{
				LocationIncidentalAnimationDefinition locationIncidentalAnimationDefinition = NextAnimation(byInstanceId);
				if (locationIncidentalAnimationDefinition != null)
				{
					idleInTownHallSignal.Dispatch(CharacterID, locationIncidentalAnimationDefinition);
				}
				else
				{
					logger.Error("Cannot find an animation for frolic");
				}
			}
			else
			{
				logger.Error("Cannot find frolic");
			}
		}

		private LocationIncidentalAnimationDefinition NextAnimation(FrolicCharacter character)
		{
			FloatLocation currentFrolicLocation = character.CurrentFrolicLocation;
			FrolicCharacterDefinition definition = character.Definition;
			WeightedDefinition wanderWeightedDeck = definition.WanderWeightedDeck;
			if (playerService.GetMinionPartyInstance().IsPartyHappening)
			{
				foreach (LocationIncidentalAnimationDefinition wanderAnimation in definition.WanderAnimations)
				{
					if (wanderAnimation.ID == 1000009613)
					{
						return wanderAnimation;
					}
				}
			}
			WeightedInstance weightedInstance = playerService.GetWeightedInstance(wanderWeightedDeck.ID, wanderWeightedDeck);
			int num = 10;
			LocationIncidentalAnimationDefinition locationIncidentalAnimationDefinition = null;
			do
			{
				QuantityItem quantityItem = weightedInstance.NextPick(randomService);
				foreach (LocationIncidentalAnimationDefinition wanderAnimation2 in definition.WanderAnimations)
				{
					if (wanderAnimation2.ID == quantityItem.ID)
					{
						locationIncidentalAnimationDefinition = wanderAnimation2;
						break;
					}
				}
			}
			while (num-- > 0 && locationIncidentalAnimationDefinition != null && locationIncidentalAnimationDefinition.Location.Equals(currentFrolicLocation));
			if (locationIncidentalAnimationDefinition == null)
			{
				logger.Error("Weighted deck {0} has illegal location animation defs", wanderWeightedDeck.ID);
			}
			return locationIncidentalAnimationDefinition;
		}
	}
}
