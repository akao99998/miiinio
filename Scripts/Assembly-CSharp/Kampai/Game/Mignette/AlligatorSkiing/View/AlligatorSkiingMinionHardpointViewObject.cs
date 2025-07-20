using UnityEngine;

namespace Kampai.Game.Mignette.AlligatorSkiing.View
{
	public class AlligatorSkiingMinionHardpointViewObject : MonoBehaviour
	{
		public AlligatorAgent Agent;

		public Transform MinionTriggerVFXMarker;

		public void OnObstacleCollision()
		{
			Agent.OnMinionHitObstacle();
		}

		public void OnCollectableCollision(Vector3 collectablePosition, int collectablePoints, CollectibleType type)
		{
			Agent.OnMinionHitCollectable(collectablePosition, collectablePoints, type);
		}
	}
}
