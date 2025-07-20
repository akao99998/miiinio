using System;
using UnityEngine;

namespace Kampai.Game.Mignette.View
{
	public class MignetteCollectableViewObject : MonoBehaviour
	{
		public enum CollectableStates
		{
			None = 0,
			Spawn = 1,
			Bounce = 2,
			Collected = 3
		}

		[Serializable]
		public class PointsAndMaterials
		{
			public Material materialForPoint;

			public int minPointsForMaterial;
		}

		public CollectableStates CollectableState;

		public Renderer CollectableRenderer;

		public Animator CollectableAnimator;

		public ParticleSystem CollectedParticle;

		public ParticleSystem SparkleParticle;

		public ParticleSystem GlowParticle;

		public TrailRenderer CollectableTrail;

		public Vector3 collectibleOffset = new Vector3(0.1f, 0f, 0f);

		private Transform mignetteCameraTransform;

		private Transform collectableTransform;

		public PointsAndMaterials[] MaterialsList;

		public void UpdateMaterialForPointValue(int points)
		{
			for (int i = 0; i < MaterialsList.Length; i++)
			{
				if (points >= MaterialsList[i].minPointsForMaterial)
				{
					CollectableRenderer.material = MaterialsList[i].materialForPoint;
					break;
				}
			}
		}

		public void ToggleModel()
		{
			CollectableRenderer.enabled = !CollectableRenderer.enabled;
		}

		public void Update()
		{
			if (mignetteCameraTransform != null && collectableTransform != null)
			{
				Vector3 vector = base.transform.position - mignetteCameraTransform.position;
				GlowParticle.transform.position = collectableTransform.position + vector.normalized * 0.3f;
				SparkleParticle.transform.position = collectableTransform.position - vector.normalized * 1f;
				CollectedParticle.transform.position = collectableTransform.position + vector.normalized * collectibleOffset.x;
			}
		}

		public void SetState(CollectableStates newState, Camera mignetteCamera)
		{
			mignetteCameraTransform = mignetteCamera.transform;
			collectableTransform = CollectableRenderer.gameObject.transform;
			if (CollectableState != newState)
			{
				CollectableState = newState;
				switch (newState)
				{
				case CollectableStates.Spawn:
					CollectableAnimator.SetTrigger("OnSpawn");
					CollectableTrail.enabled = true;
					break;
				case CollectableStates.Bounce:
					CollectableAnimator.SetTrigger("OnBounce");
					CollectableTrail.enabled = false;
					break;
				case CollectableStates.Collected:
					CollectableAnimator.SetTrigger("OnCollected");
					CollectedParticle.Play();
					CollectableTrail.enabled = false;
					break;
				}
			}
		}
	}
}
