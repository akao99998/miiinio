using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class CameraAutoPanSignal : Signal<Vector3, CameraMovementSettings, Boxed<Building>, Boxed<Quest>>
	{
	}
}
