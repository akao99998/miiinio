using Kampai.Game.View;
using Kampai.UI.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MinionPickController : Command
	{
		private CharacterObject characterObject;

		[Inject]
		public GameObject selectedGameObject { get; set; }

		[Inject]
		public TapMinionSignal tapMinionSignal { get; set; }

		[Inject]
		public TSMCharacterSelectedSignal tsmCharacterSelectedSignal { get; set; }

		[Inject]
		public NamedCharacterSelectedSignal namedCharacterSelectedSignal { get; set; }

		[Inject]
		public GetWayFinderSignal getWayFinderSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			if (minionPartyInstance.IsPartyHappening)
			{
				return;
			}
			characterObject = selectedGameObject.GetComponentInParent(typeof(CharacterObject)) as CharacterObject;
			if (!(characterObject != null))
			{
				return;
			}
			if (characterObject is VillainView)
			{
				Villain byInstanceId = playerService.GetByInstanceId<Villain>(characterObject.ID);
				if (byInstanceId != null)
				{
					getWayFinderSignal.Dispatch(byInstanceId.CabanaBuildingId, OnGetWayFinder);
				}
			}
			else if (characterObject is TSMCharacterView)
			{
				tsmCharacterSelectedSignal.Dispatch();
			}
			else
			{
				getWayFinderSignal.Dispatch(characterObject.ID, OnGetWayFinder);
				namedCharacterSelectedSignal.Dispatch(characterObject);
			}
		}

		private void OnGetWayFinder(int trackedId, IWayFinderView wayFinderView)
		{
			if (wayFinderView != null)
			{
				wayFinderView.SimulateClick();
				return;
			}
			MinionObject minionObject = characterObject as MinionObject;
			if (minionObject != null)
			{
				tapMinionSignal.Dispatch(minionObject.ID);
			}
		}
	}
}
