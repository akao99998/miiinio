using UnityEngine;

namespace Kampai.Game.View
{
	public class MasterPlanObject : ActionableObject
	{
		private Collider lairCollider;

		public void Init(int masterPlanInstanceID)
		{
			ID = masterPlanInstanceID;
		}

		public Vector3 GetIndicatorPosition()
		{
			if (lairCollider == null)
			{
				lairCollider = base.gameObject.GetComponent<Collider>();
			}
			return new Vector3(lairCollider.bounds.center.x, lairCollider.bounds.max.y, lairCollider.bounds.center.z);
		}
	}
}
