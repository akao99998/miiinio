using UnityEngine;

namespace Kampai.Game
{
	public interface ZoomableBuildingDefinition
	{
		Vector3 zoomOffset { get; set; }

		Vector3 zoomEulers { get; set; }

		float zoomFOV { get; set; }
	}
}
