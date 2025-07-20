using UnityEngine;

namespace Kampai.Game.View
{
	public class TouchZoomView : ZoomView
	{
		private Vector3 touch1PreviousPosition;

		private Vector3 touch2PreviousPosition;

		protected override bool IsInputStationary()
		{
			int touchCount = Input.touchCount;
			if (touchCount <= 0)
			{
				return false;
			}
			if (touchCount == 1)
			{
				Touch touch = Input.GetTouch(0);
				return touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Began;
			}
			Touch touch2 = Input.GetTouch(0);
			Touch touch3 = Input.GetTouch(1);
			return ((touch2.phase == TouchPhase.Stationary || touch2.phase == TouchPhase.Began) && touch3.phase != TouchPhase.Moved) || ((touch3.phase == TouchPhase.Stationary || touch3.phase == TouchPhase.Began) && touch2.phase != TouchPhase.Moved);
		}

		protected override bool IsInputDone()
		{
			int touchCount = Input.touchCount;
			if (touchCount <= 0)
			{
				return true;
			}
			if (touchCount == 1)
			{
				Touch touch = Input.GetTouch(0);
				return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
			}
			Touch touch2 = Input.GetTouch(0);
			Touch touch3 = Input.GetTouch(1);
			return (touch2.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Canceled) && (touch3.phase == TouchPhase.Ended || touch3.phase == TouchPhase.Canceled);
		}

		public override void CalculateBehaviour(Vector3 position)
		{
			Touch touch = Input.GetTouch(0);
			Touch touch2 = Input.GetTouch(1);
			if (touch.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
			{
				if (touch1PreviousPosition == Vector3.zero && touch2PreviousPosition == Vector3.zero)
				{
					touch1PreviousPosition = touch.position;
					touch2PreviousPosition = touch2.position;
					return;
				}
				float magnitude = (touch1PreviousPosition - touch2PreviousPosition).magnitude;
				float magnitude2 = (touch.position - touch2.position).magnitude;
				float y = magnitude2 - magnitude;
				velocity = new Vector3(0f, y, 0f);
				touch1PreviousPosition = touch.position;
				touch2PreviousPosition = touch2.position;
			}
		}

		public override void ResetBehaviour()
		{
			touch1PreviousPosition = Vector3.zero;
			touch2PreviousPosition = Vector3.zero;
		}
	}
}
