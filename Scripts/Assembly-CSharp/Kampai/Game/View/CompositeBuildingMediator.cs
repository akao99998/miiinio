using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class CompositeBuildingMediator : Mediator
	{
		private const float MINION_REACT_RADIUS = 20f;

		private IKampaiLogger logger = LogManager.GetClassLogger("CompositeBuildingMediator") as IKampaiLogger;

		private CompositeBuilding compositeBuilding;

		[Inject]
		public CompositeBuildingView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CompositeBuildingPieceAddedSignal compositeBuildingPieceAddedSignal { get; set; }

		[Inject]
		public MinionReactInRadiusSignal minionReactInRadiusSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			compositeBuilding = GetCompositeBuilding();
			view.SetupPieces(GetAttachedPieces());
			compositeBuildingPieceAddedSignal.AddListener(OnCompositePieceAdded);
		}

		public override void OnRemove()
		{
			compositeBuildingPieceAddedSignal.RemoveListener(OnCompositePieceAdded);
			base.OnRemove();
		}

		private CompositeBuilding GetCompositeBuilding()
		{
			CompositeBuildingObject component = GetComponent<CompositeBuildingObject>();
			if (component != null)
			{
				return component.compositeBuilding;
			}
			logger.Error("CompositeBuildingMediator: could not find CompositeBuilding for building " + base.gameObject.name);
			return null;
		}

		private IList<CompositeBuildingPiece> GetAttachedPieces()
		{
			IList<CompositeBuildingPiece> list = new List<CompositeBuildingPiece>();
			foreach (int attachedCompositePieceID in compositeBuilding.AttachedCompositePieceIDs)
			{
				CompositeBuildingPiece byInstanceId = playerService.GetByInstanceId<CompositeBuildingPiece>(attachedCompositePieceID);
				if (byInstanceId == null)
				{
					logger.Log(KampaiLogLevel.Error, "CompositeBuildingObject, Piece ID not owned: " + attachedCompositePieceID);
				}
				else
				{
					list.Add(byInstanceId);
				}
			}
			return list;
		}

		private void OnCompositePieceAdded(CompositeBuilding buildingAddedTo)
		{
			if (compositeBuilding == buildingAddedTo)
			{
				int numPieces = view.GetNumPieces();
				IList<CompositeBuildingPiece> attachedPieces = GetAttachedPieces();
				for (int i = numPieces; i < attachedPieces.Count; i++)
				{
					view.AddNewlyCreatedPiece(attachedPieces[i], playSFXSignal);
				}
				minionReactInRadiusSignal.Dispatch(20f, base.transform.position);
			}
		}

		public void PlayShuffleSequence(IList<int> newPieceOrder)
		{
			view.PlayShuffleSequence(newPieceOrder, playSFXSignal);
		}
	}
}
