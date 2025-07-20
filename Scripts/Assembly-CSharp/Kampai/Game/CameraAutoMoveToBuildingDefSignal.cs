using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class CameraAutoMoveToBuildingDefSignal : Signal<BuildingDefinition, Vector3, PanInstructions>
	{
	}
}
