using UnityEngine;

namespace Kampai.Game.View
{
	public class TouchPanView : PanView
	{
		public override void CalculateBehaviour(Vector3 position)
		{
			mouseRay = mainCamera.ScreenPointToRay(position);
			groundPlane.Raycast(mouseRay, out hitDistance);
			if (!initialized)
			{
				initialized = true;
				hitPosition = mouseRay.GetPoint(hitDistance);
			}
			currentPosition = mouseRay.GetPoint(hitDistance);
			velocity = hitPosition - currentPosition;
		}
	}
}
