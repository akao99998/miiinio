using UnityEngine;

namespace Kampai.Game.View
{
	public class PalmTreeRotationConstraint : MonoBehaviour
	{
		public enum Orientation
		{
			NONE = 0,
			UP = 1,
			DOWN = 2,
			LEFT = 3,
			RIGHT = 4
		}

		public Transform Ground;

		public Orientation OrentationDirection;

		public Vector2 RandomXRotationRange;

		public Vector2 RandomYRotationRange;

		public Vector2 RandomZRotationRange;

		private void Start()
		{
			switch (OrentationDirection)
			{
			case Orientation.UP:
				base.transform.up = Ground.up;
				break;
			case Orientation.DOWN:
				base.transform.up = -Ground.up;
				break;
			case Orientation.LEFT:
				base.transform.up = -Ground.right;
				break;
			case Orientation.RIGHT:
				base.transform.up = Ground.right;
				break;
			}
			base.transform.Rotate(Random.Range(RandomXRotationRange.x, RandomXRotationRange.y), Random.Range(RandomYRotationRange.x, RandomYRotationRange.y), Random.Range(RandomZRotationRange.x, RandomZRotationRange.y));
		}
	}
}
