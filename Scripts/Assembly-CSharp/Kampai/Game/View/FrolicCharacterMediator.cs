using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class FrolicCharacterMediator : Mediator
	{
		public NamedCharacterManagerView NamedCharacterManagerView;

		public Signal AnimationCallbackSignal = new Signal();

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CharacterArrivedAtDestinationSignal arrivedAtDestinationSignal { get; set; }

		[Inject]
		public FrolicSignal frolicSignal { get; set; }

		[Inject]
		public IdleInTownHallSignal idleInTownHallSignal { get; set; }

		[Inject]
		public PostMinionPartyStartSignal postMinionPartyStartSignal { get; set; }

		[Inject]
		public PostMinionPartyEndSignal postMinionPartyEndSignal { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject NamedCharacterManager { get; set; }

		public FrolicCharacterView view { get; set; }

		public override void OnRegister()
		{
			NamedCharacterManagerView = NamedCharacterManager.GetComponent<NamedCharacterManagerView>();
			view = GetComponent<FrolicCharacterView>();
			view.SetAnimationCallback(AnimationCallbackSignal);
			arrivedAtDestinationSignal.AddListener(ArrivedAtWayPoint);
			idleInTownHallSignal.AddListener(IdleInTownHall);
			AnimationCallbackSignal.AddListener(AnimationCallback);
			postMinionPartyStartSignal.AddListener(StartParty);
			postMinionPartyEndSignal.AddListener(EndParty);
		}

		public override void OnRemove()
		{
			arrivedAtDestinationSignal.RemoveListener(ArrivedAtWayPoint);
			idleInTownHallSignal.RemoveListener(IdleInTownHall);
			AnimationCallbackSignal.RemoveListener(AnimationCallback);
			postMinionPartyStartSignal.RemoveListener(StartParty);
			postMinionPartyEndSignal.RemoveListener(EndParty);
		}

		private void ArrivedAtWayPoint(int minionID)
		{
			if (view.ID == minionID)
			{
				view.ArrivedAtWayPoint();
			}
		}

		private void AnimationCallback()
		{
			frolicSignal.Dispatch(view.ID);
		}

		private void IdleInTownHall(int characterId, LocationIncidentalAnimationDefinition animationDefinition)
		{
			if (characterId == view.ID)
			{
				FrolicCharacter byInstanceId = playerService.GetByInstanceId<FrolicCharacter>(view.ID);
				byInstanceId.CurrentFrolicLocation = animationDefinition.Location;
				MinionAnimationDefinition mad = definitionService.Get<MinionAnimationDefinition>(animationDefinition.AnimationId);
				view.IdleInTownHall(animationDefinition, mad);
			}
		}

		protected virtual void StartParty()
		{
			view.SetIsInParty(true);
		}

		protected virtual void EndParty()
		{
			view.SetIsInParty(false);
			if (view.IsWandering())
			{
				frolicSignal.Dispatch(view.ID);
			}
		}
	}
}
