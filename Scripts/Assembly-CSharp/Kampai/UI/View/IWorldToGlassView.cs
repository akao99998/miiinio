using UnityEngine;

namespace Kampai.UI.View
{
	public interface IWorldToGlassView
	{
		int TrackedId { get; }

		GameObject GameObject { get; }

		void SetForceHide(bool forceHide);

		Vector3 GetIndicatorPosition();
	}
}
