using UnityEngine;

namespace Kampai.Game.Mignette.BalloonBarrage.View
{
	public class BalloonBarrageColliderViewObject : MonoBehaviour
	{
		public enum TargetTypes
		{
			Basket = 0,
			Minion = 1,
			Balloon = 2
		}

		public TargetTypes TargetType;

		public BalloonBarrageTargetAnimatorViewObject ParentTargetBalloonViewObject;
	}
}
