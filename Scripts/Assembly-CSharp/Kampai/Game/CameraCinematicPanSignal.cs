using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class CameraCinematicPanSignal : Signal<Tuple<Vector3, float>, CameraMovementSettings, Boxed<Building>, Boxed<Quest>>
	{
	}
}
