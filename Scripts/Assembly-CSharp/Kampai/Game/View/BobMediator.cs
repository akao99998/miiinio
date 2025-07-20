using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class BobMediator : Mediator
	{
		private Signal animationCallback = new Signal();

		[Inject]
		public BobView view { get; set; }

		[Inject]
		public BobPointsAtSignSignal pointAtSignSignal { get; set; }

		[Inject]
		public IdleInTownHallSignal idleInTownHallSignal { get; set; }

		[Inject]
		public CharacterArrivedAtDestinationSignal arrivedAtDestinationSignal { get; set; }

		[Inject]
		public BobCelebrateLandExpansionSignal celebrateLandExpansionSignal { get; set; }

		[Inject]
		public BobCelebrateLandExpansionCompleteSignal celebrateLandExpansionCompleteSignal { get; set; }

		[Inject]
		public BobReturnToTownSignal bobReturnToTown { get; set; }

		[Inject]
		public PointBobLandExpansionSignal pointBobLandExpansionSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public FrolicSignal frolicSignal { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		public override void OnRegister()
		{
			view.Init();
			view.SetAnimationCallback(animationCallback);
			pointAtSignSignal.AddListener(PointAtSign);
			idleInTownHallSignal.AddListener(IdleInTownHall);
			arrivedAtDestinationSignal.AddListener(ArrivedAtWayPoint);
			celebrateLandExpansionSignal.AddListener(CelebrateLandExpansion);
			animationCallback.AddListener(AnimationCallback);
			bobReturnToTown.AddListener(ReturnToTown);
			pointBobLandExpansionSignal.Dispatch();
		}

		public override void OnRemove()
		{
			pointAtSignSignal.RemoveListener(PointAtSign);
			idleInTownHallSignal.RemoveListener(IdleInTownHall);
			arrivedAtDestinationSignal.RemoveListener(ArrivedAtWayPoint);
			celebrateLandExpansionSignal.RemoveListener(CelebrateLandExpansion);
			animationCallback.RemoveListener(AnimationCallback);
			bobReturnToTown.RemoveListener(ReturnToTown);
		}

		private void PointAtSign(Vector3 position)
		{
			view.PointAtSign(position);
		}

		private void IdleInTownHall(int characterId, LocationIncidentalAnimationDefinition animationDefinition)
		{
			if (characterId == view.ID)
			{
				BobCharacter byInstanceId = playerService.GetByInstanceId<BobCharacter>(view.ID);
				byInstanceId.CurrentFrolicLocation = animationDefinition.Location;
				MinionAnimationDefinition mad = definitionService.Get<MinionAnimationDefinition>(animationDefinition.AnimationId);
				view.IdleInTownHall(animationDefinition, mad);
			}
		}

		private void ArrivedAtWayPoint(int minionID)
		{
			if (view.ID == minionID)
			{
				view.ArrivedAtWayPoint();
			}
		}

		private void CelebrateLandExpansion()
		{
			BobCharacter byInstanceId = playerService.GetByInstanceId<BobCharacter>(view.ID);
			Prestige prestigeFromMinionInstance = prestigeService.GetPrestigeFromMinionInstance(byInstanceId);
			if (prestigeFromMinionInstance.state != PrestigeState.Questing)
			{
				view.CelebrateLandExpansion(celebrateLandExpansionCompleteSignal);
			}
			else
			{
				celebrateLandExpansionCompleteSignal.Dispatch();
			}
		}

		private void AnimationCallback()
		{
			frolicSignal.Dispatch(view.ID);
		}

		private void ReturnToTown()
		{
			view.ReturnToTown();
		}
	}
}
