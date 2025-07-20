using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class CameraAutoMoveSignal : Signal<Vector3, Boxed<ScreenPosition>, CameraMovementSettings, bool>
	{
	}
}
