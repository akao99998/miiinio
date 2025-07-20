using System.Collections;
using UnityEngine;

namespace Kampai.Game.View
{
	public class CompositeBuildingPieceObject : MonoBehaviour
	{
		public Renderer BoundsRenderer;

		public GameObject[] TopPieceEnabledObjects;

		public GameObject[] TopPieceDisabledObjects;

		public GameObject[] AllCollectedEnabledObjects;

		public GameObject[] AllCollectedDisabledObjects;

		private Animator animator;

		public int PieceID { get; set; }

		private void Awake()
		{
			animator = GetComponent<Animator>();
		}

		public Vector3 GetMaxBounds()
		{
			return BoundsRenderer.bounds.max;
		}

		public void PlayFallInAnimation()
		{
			animator.Play("FallIn", 0, 0f);
		}

		public void PlayFallInShuffleTopAnimation()
		{
			animator.Play("FallInShuffleTop", 0, 0f);
		}

		public void PlayFallInShuffleNotTopAnimation()
		{
			animator.Play("FallInShuffleNotTop", 0, 0f);
		}

		public void PlayJumpAnimation()
		{
			animator.Play("Jump", 0, 0f);
		}

		public void RefreshAppearance(bool isOnTop, bool allPiecesCollected)
		{
			animator.SetBool("IsTopPiece", isOnTop);
			animator.SetBool("AllPiecesCollected", allPiecesCollected);
			SetGameObjectsActive(TopPieceEnabledObjects, isOnTop);
			SetGameObjectsActive(AllCollectedEnabledObjects, allPiecesCollected);
			SetGameObjectsActive(TopPieceDisabledObjects, !isOnTop);
			SetGameObjectsActive(AllCollectedDisabledObjects, !allPiecesCollected);
		}

		private void SetGameObjectsActive(GameObject[] gameObjects, bool isActive)
		{
			for (int i = 0; i < gameObjects.Length; i++)
			{
				ParticleSystem component = gameObjects[i].GetComponent<ParticleSystem>();
				if (component != null)
				{
					if (isActive)
					{
						gameObjects[i].SetActive(true);
						component.Play();
					}
					else
					{
						component.Stop();
						StartCoroutine(waitThenDisableParticleSystem(gameObjects[i], component.startLifetime));
					}
				}
				else
				{
					gameObjects[i].SetActive(isActive);
				}
			}
		}

		private IEnumerator waitThenDisableParticleSystem(GameObject gameObject, float secondsToWait)
		{
			yield return new WaitForSeconds(secondsToWait);
			if (gameObject != null)
			{
				ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
				if (particleSystem != null && particleSystem.isStopped)
				{
					gameObject.SetActive(false);
				}
			}
		}
	}
}
