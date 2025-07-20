using UnityEngine;

namespace Kampai.UI.View
{
	public class KampaiIngoreRaycastImage : KampaiImage
	{
		public override bool Raycast(Vector2 sp, Camera eventCamera)
		{
			return false;
		}
	}
}
