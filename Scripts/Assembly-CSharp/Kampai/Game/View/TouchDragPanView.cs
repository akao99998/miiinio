using UnityEngine;

namespace Kampai.Game.View
{
	public class TouchDragPanView : DragPanView
	{
		public override void CalculateBehaviour(Vector3 position)
		{
			if (position.x < xThreshold || position.x > screenWidth - xThreshold || position.y < yThreshold || position.y > screenHeight - yThreshold)
			{
				pan = true;
			}
			else
			{
				pan = false;
			}
		}
	}
}
