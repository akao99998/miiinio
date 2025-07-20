using UnityEngine;

namespace Kampai.Game.View
{
	public class KeyboardZoomView : ZoomView
	{
		private int frameCount;

		protected override bool IsInputDone()
		{
			if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) * Time.deltaTime < 1E-07f)
			{
				frameCount++;
			}
			else
			{
				frameCount = 0;
			}
			if (frameCount >= 3)
			{
				frameCount = 3;
				return true;
			}
			return false;
		}

		public override void CalculateBehaviour(Vector3 position)
		{
			mouseRay = new Ray(base.transform.position, base.transform.forward);
			groundPlane.Raycast(mouseRay, out hitDistance);
			hitPosition = mouseRay.GetPoint(hitDistance);
			Vector3 v = base.transform.worldToLocalMatrix.MultiplyPoint3x4(hitPosition);
			Vector3 vector = base.transform.localToWorldMatrix.MultiplyVector(v);
			velocity = vector * Input.GetAxis("Mouse ScrollWheel");
		}
	}
}
