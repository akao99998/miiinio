using UnityEngine;

namespace Kampai.Game.Mignette.AlligatorSkiing.View
{
	public class AlligatorSkiingObstacleViewObject : MonoBehaviour
	{
		public bool IsCollectable;

		public CollectibleType CollectibleType;

		public bool DestroyOnCollision = true;

		public GameObject ImpactVFX;

		public Transform VFXPositionHolder;

		public int CollectablePoints = 10;

		public bool CollectableObstacleCancel;

		private AlligatorSkiingMinionHardpointViewObject minionHardpoint;

		private void OnTriggerEnter(Collider other)
		{
			minionHardpoint = other.GetComponent<AlligatorSkiingMinionHardpointViewObject>();
			if (minionHardpoint != null)
			{
				if (IsCollectable && !CollectableObstacleCancel)
				{
					HandleCollectable();
					minionHardpoint = null;
				}
				else if (!IsCollectable)
				{
					minionHardpoint.OnObstacleCollision();
					minionHardpoint = null;
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (minionHardpoint != null && IsCollectable && CollectableObstacleCancel && !minionHardpoint.Agent.View.IsOnPenalty)
			{
				HandleCollectable();
				minionHardpoint = null;
			}
		}

		private void HandleCollectable()
		{
			Vector3 vector = ((!(VFXPositionHolder != null)) ? base.transform.position : VFXPositionHolder.position);
			minionHardpoint.OnCollectableCollision(vector, CollectablePoints, CollectibleType);
			if (!minionHardpoint.Agent.View.IsOnPenalty)
			{
				if (DestroyOnCollision)
				{
					Object.Destroy(base.gameObject);
				}
				if (ImpactVFX != null)
				{
					GameObject gameObject = Object.Instantiate(ImpactVFX);
					gameObject.transform.position = vector;
				}
			}
		}
	}
}
