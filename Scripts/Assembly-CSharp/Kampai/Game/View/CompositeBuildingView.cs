using System.Collections;
using System.Collections.Generic;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class CompositeBuildingView : KampaiView
	{
		public List<Transform> SpawnPoints;

		public Transform Placement01VFXPrefab;

		public Transform Placement02VFXPrefab;

		public Transform ShuffleVFXPrefab;

		public Vector3 MidShuffleOffsetTop = new Vector3(-1.732f, 0f, 1f);

		public Vector3 MidShuffleOffsetNotTop = new Vector3(0f, 4f, 0f);

		private IList<CompositeBuildingPieceObject> pieceObjects = new List<CompositeBuildingPieceObject>();

		public void SetupPieces(IList<CompositeBuildingPiece> compositePieces)
		{
			for (int i = 0; i < compositePieces.Count; i++)
			{
				AddPiece(compositePieces[i]);
			}
			RefreshColliderSize();
			RefreshPieceAppearance();
		}

		public void AddNewlyCreatedPiece(CompositeBuildingPiece piece, PlayGlobalSoundFXSignal playSFXSignal)
		{
			StartCoroutine(WaitThenAddNewPiece(piece, playSFXSignal));
		}

		private IEnumerator WaitThenAddNewPiece(CompositeBuildingPiece piece, PlayGlobalSoundFXSignal playSFXSignal)
		{
			yield return new WaitForSeconds(2.2f);
			CompositeBuildingPieceObject newPiece = AddPiece(piece);
			newPiece.PlayFallInAnimation();
			playSFXSignal.Dispatch("Play_totem_fallIn_01");
			RefreshColliderSize();
			yield return new WaitForSeconds(0.4f);
			RefreshPieceAppearance();
			Transform vfxInstanceP = Object.Instantiate(Placement01VFXPrefab);
			vfxInstanceP.SetParent(newPiece.transform, false);
			vfxInstanceP.transform.localPosition = new Vector3(0f, 0.1f, 0f);
			Transform vfxInstanceP2 = Object.Instantiate(Placement02VFXPrefab);
			vfxInstanceP2.SetParent(newPiece.transform, false);
			vfxInstanceP2.transform.localPosition = new Vector3(0f, 0.1f, 0f);
			Transform vfxInstanceP3 = Object.Instantiate(ShuffleVFXPrefab);
			vfxInstanceP3.SetParent(newPiece.transform, false);
			vfxInstanceP3.transform.localPosition = new Vector3(0f, 0.1f, 0f);
		}

		private CompositeBuildingPieceObject AddPiece(CompositeBuildingPiece piece)
		{
			GameObject original = KampaiResources.Load<GameObject>(piece.Definition.PrefabName);
			GameObject gameObject = Object.Instantiate(original);
			CompositeBuildingPieceObject component = gameObject.GetComponent<CompositeBuildingPieceObject>();
			Transform transform = component.transform;
			transform.SetParent(base.transform, false);
			transform.localPosition = SpawnPoints[pieceObjects.Count].localPosition;
			transform.localRotation = SpawnPoints[pieceObjects.Count].localRotation;
			component.PieceID = piece.ID;
			pieceObjects.Add(component);
			return component;
		}

		private void RefreshColliderSize()
		{
			float num = 0f;
			for (int i = 0; i < pieceObjects.Count; i++)
			{
				num = Mathf.Max(num, pieceObjects[i].GetMaxBounds().y);
			}
			num -= base.transform.position.y;
			BoxCollider component = GetComponent<BoxCollider>();
			if (component != null)
			{
				component.size = new Vector3(component.size.x, num, component.size.z);
				component.center = new Vector3(component.center.x, num / 2f, component.center.z);
			}
		}

		private void RefreshPieceAppearance()
		{
			int count = pieceObjects.Count;
			for (int i = 0; i < count; i++)
			{
				bool isOnTop = i == count - 1;
				bool allPiecesCollected = count == SpawnPoints.Count;
				pieceObjects[i].RefreshAppearance(isOnTop, allPiecesCollected);
			}
		}

		public int GetNumPieces()
		{
			return pieceObjects.Count;
		}

		public void PlayShuffleSequence(IList<int> newPieceOrder, PlayGlobalSoundFXSignal playSFXSignal)
		{
			IList<CompositeBuildingPieceObject> list = new List<CompositeBuildingPieceObject>();
			for (int i = 0; i < newPieceOrder.Count; i++)
			{
				list.Add(getPieceObjectByID(newPieceOrder[i]));
			}
			pieceObjects = list;
			StartCoroutine(ShufflePiecesStaggered(playSFXSignal));
		}

		private CompositeBuildingPieceObject getPieceObjectByID(int pieceID)
		{
			for (int i = 0; i < pieceObjects.Count; i++)
			{
				if (pieceObjects[i].PieceID == pieceID)
				{
					return pieceObjects[i];
				}
			}
			return null;
		}

		private IEnumerator ShufflePiecesStaggered(PlayGlobalSoundFXSignal playSFXSignal)
		{
			Vector3 squashMove3 = new Vector3(0f, 0f, 0f);
			float squashAmtUp = 0.38f;
			float squashAmtDown = 0.35f;
			CompositeBuildingPieceObject topPiece = pieceObjects[0];
			topPiece.PlayFallInShuffleTopAnimation();
			TweenTopPieceTo(topPiece, SpawnPoints[0].position, MidShuffleOffsetTop);
			PlayShuffleAudio(playSFXSignal);
			yield return new WaitForSeconds(0.5f);
			for (int j = pieceObjects.Count - 1; j > 0; j--)
			{
				CompositeBuildingPieceObject piece2 = pieceObjects[j];
				float squashAmtCurrent2 = (float)(j - 1) * squashAmtUp;
				squashMove3 = new Vector3(0f, 0f - squashAmtCurrent2, 0f);
				piece2.PlayJumpAnimation();
				TweenNotTopPieceToUp(piece2, MidShuffleOffsetNotTop, squashMove3);
			}
			Transform vfxInstance2 = Object.Instantiate(ShuffleVFXPrefab);
			vfxInstance2.SetParent(topPiece.transform, false);
			vfxInstance2.transform.localPosition = new Vector3(0f, 0.1f, 0f);
			topPiece.RefreshAppearance(false, true);
			yield return new WaitForSeconds(0.2f);
			for (int i = 1; i < pieceObjects.Count; i++)
			{
				CompositeBuildingPieceObject piece = pieceObjects[i];
				float squashAmtCurrent2 = ((i != 1) ? squashAmtDown : 0f);
				squashMove3 = new Vector3(0f, 0f - squashAmtCurrent2, 0f);
				yield return new WaitForSeconds(0.1f);
				piece.PlayFallInShuffleNotTopAnimation();
				TweenNotTopPieceToDown(piece, SpawnPoints[i].position, squashMove3);
			}
			yield return new WaitForSeconds(0.0125f);
			RefreshPieceAppearance();
			yield return new WaitForSeconds(0.25f);
			Transform vfxInstance3 = Object.Instantiate(ShuffleVFXPrefab);
			vfxInstance3.SetParent(pieceObjects[pieceObjects.Count - 1].transform, false);
			vfxInstance3.transform.localPosition = new Vector3(0f, 0.1f, 0f);
		}

		private void PlayShuffleAudio(PlayGlobalSoundFXSignal playSFXSignal)
		{
			string type = string.Empty;
			switch (pieceObjects.Count)
			{
			case 2:
				type = "Play_totem_shuffleOfTwo_01";
				break;
			case 3:
				type = "Play_totem_shuffleOfThree_01";
				break;
			case 4:
				type = "Play_totem_shuffleOfFour_01";
				break;
			case 5:
				type = "Play_totem_shuffleOfFive_01";
				break;
			}
			playSFXSignal.Dispatch(type);
		}

		private void TweenTopPieceTo(CompositeBuildingPieceObject piece, Vector3 targetPosition, Vector3 midTweenOffset)
		{
			Transform pieceTransform = piece.transform;
			Vector3 pieceOrigin = pieceTransform.position;
			Vector3 AnticAmt = new Vector3(0.3464f, 0f, -0.2f);
			Go.to(pieceTransform, 0.075f, new GoTweenConfig().setEaseType(GoEaseType.SineIn).position(pieceOrigin + AnticAmt).onComplete(delegate(AbstractGoTween thisTweenA)
			{
				thisTweenA.destroy();
				Go.to(pieceTransform, 0.112500004f, new GoTweenConfig().setEaseType(GoEaseType.SineInOut).position(pieceOrigin + midTweenOffset).onComplete(delegate(AbstractGoTween thisTween)
				{
					thisTween.destroy();
					Go.to(pieceTransform, 7.5E-06f, new GoTweenConfig().setEaseType(GoEaseType.Linear).position(pieceOrigin + midTweenOffset).onComplete(delegate(AbstractGoTween thisTween2)
					{
						thisTween2.destroy();
						Go.to(pieceTransform, 0.2625f, new GoTweenConfig().setEaseType(GoEaseType.SineIn).position(targetPosition + midTweenOffset).onComplete(delegate(AbstractGoTween thisTween3)
						{
							thisTween3.destroy();
							Go.to(pieceTransform, 0.16874999f, new GoTweenConfig().setEaseType(GoEaseType.Linear).position(targetPosition + midTweenOffset).onComplete(delegate(AbstractGoTween thisTween4)
							{
								thisTween4.destroy();
								Go.to(pieceTransform, 3f / 32f, new GoTweenConfig().setEaseType(GoEaseType.SineInOut).position(targetPosition + AnticAmt).onComplete(delegate(AbstractGoTween thisTweenF)
								{
									thisTweenF.destroy();
									Go.to(pieceTransform, 0.112500004f, new GoTweenConfig().setEaseType(GoEaseType.SineOut).position(targetPosition));
								}));
							}));
						}));
					}));
				}));
			}));
		}

		private void TweenNotTopPieceToUp(CompositeBuildingPieceObject piece, Vector3 midTweenOffset, Vector3 squashOffset)
		{
			Transform pieceTransform = piece.transform;
			Vector3 pieceOrigin = pieceTransform.position;
			Go.to(pieceTransform, 0.09f, new GoTweenConfig().setEaseType(GoEaseType.SineIn).position(pieceTransform.position + squashOffset).onComplete(delegate(AbstractGoTween thisTween)
			{
				thisTween.destroy();
				Go.to(pieceTransform, 0.21000001f, new GoTweenConfig().setEaseType(GoEaseType.SineOut).position(pieceOrigin + midTweenOffset));
			}));
		}

		private void TweenNotTopPieceToDown(CompositeBuildingPieceObject piece, Vector3 targetPosition, Vector3 squashOffset)
		{
			Vector3 bounceAmt = new Vector3(0f, 0.2f, 0f);
			Transform pieceTransform = piece.transform;
			Go.to(pieceTransform, 0.21000001f, new GoTweenConfig().setEaseType(GoEaseType.SineIn).position(targetPosition + squashOffset).onComplete(delegate(AbstractGoTween thisTween)
			{
				thisTween.destroy();
				Go.to(pieceTransform, 0.060000002f, new GoTweenConfig().setEaseType(GoEaseType.SineOut).position(targetPosition + bounceAmt).onComplete(delegate(AbstractGoTween thisTween2)
				{
					thisTween2.destroy();
					Go.to(pieceTransform, 0.030000001f, new GoTweenConfig().setEaseType(GoEaseType.SineIn).position(targetPosition));
				}));
			}));
		}
	}
}
