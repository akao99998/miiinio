using UnityEngine;

namespace Kampai.Game.Mignette.BalloonBarrage.View
{
	public class BalloonBarrageMangoViewObject : MonoBehaviour
	{
		private BalloonBarrageMignetteManagerView parentView;

		public GameObject MangoModel;

		public ParticleSystem[] MangoImpactParticleSystems;

		public ParticleSystem[] MangoImpactSplashParticles;

		public bool waterSplash;

		public bool hasHitGround;

		private void Start()
		{
			ParticleSystem[] mangoImpactSplashParticles = MangoImpactSplashParticles;
			foreach (ParticleSystem particleSystem in mangoImpactSplashParticles)
			{
				particleSystem.Stop();
				particleSystem.Clear();
			}
			MangoModel.SetActive(true);
			hasHitGround = false;
		}

		private void Update()
		{
			if (parentView.IsPaused || !(base.transform.position.y <= 0f))
			{
				return;
			}
			GetComponent<Rigidbody>().isKinematic = true;
			Vector3 position = base.transform.position;
			position.y = 0f;
			base.transform.position = position;
			base.transform.rotation = Quaternion.identity;
			MangoModel.SetActive(false);
			if (!hasHitGround)
			{
				parentView.MangoHasBeenResolved(true);
				hasHitGround = true;
			}
			if (!waterSplash)
			{
				ParticleSystem[] mangoImpactSplashParticles = MangoImpactSplashParticles;
				foreach (ParticleSystem particleSystem in mangoImpactSplashParticles)
				{
					particleSystem.Play();
					waterSplash = true;
				}
			}
		}

		public void ThrowMango(BalloonBarrageMignetteManagerView parent, GameObject referenceGO, Vector3 target, float force)
		{
			parentView = parent;
			base.transform.position = referenceGO.transform.position;
			target.y = base.transform.position.y;
			Vector3 vector = target - base.transform.position;
			base.transform.rotation = referenceGO.transform.rotation;
			GetComponent<Rigidbody>().AddForce(vector.normalized * force);
			GetComponent<Rigidbody>().AddTorque(base.transform.right * 2000f);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (hasHitGround)
			{
				return;
			}
			BalloonBarrageColliderViewObject component = other.gameObject.GetComponent<BalloonBarrageColliderViewObject>();
			if (component != null && parentView.MangoHitMovingTarget(this, component))
			{
				parentView.MangoHasBeenResolved(false);
				Object.Destroy(base.gameObject);
				return;
			}
			BalloonBarrageTargetAnimatorViewObject component2 = other.gameObject.GetComponent<BalloonBarrageTargetAnimatorViewObject>();
			if (component2 != null && !component2.IsAFlyer())
			{
				parentView.MangoHitStaticTarget(this, component2);
				parentView.MangoHasBeenResolved(false);
				Object.Destroy(base.gameObject);
				return;
			}
			BalloonBarrageGroundCollider component3 = other.gameObject.GetComponent<BalloonBarrageGroundCollider>();
			if (component3 != null)
			{
				GetComponent<Rigidbody>().isKinematic = true;
				Vector3 position = base.transform.position;
				position.y = 0f;
				base.transform.position = position;
				base.transform.rotation = Quaternion.identity;
				MangoModel.SetActive(false);
				if (!hasHitGround)
				{
					parentView.MangoHasBeenResolved(true);
					hasHitGround = true;
					GetComponent<Collider>().enabled = false;
				}
				ParticleSystem[] mangoImpactParticleSystems = MangoImpactParticleSystems;
				foreach (ParticleSystem particleSystem in mangoImpactParticleSystems)
				{
					particleSystem.Stop();
					particleSystem.Clear();
					particleSystem.Play();
					waterSplash = true;
				}
				ParticleSystem[] mangoImpactSplashParticles = MangoImpactSplashParticles;
				foreach (ParticleSystem particleSystem2 in mangoImpactSplashParticles)
				{
					particleSystem2.Stop();
				}
			}
		}
	}
}
