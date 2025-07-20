using System.Collections.Generic;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class MinionWalkPathSignal : Signal<int, IList<Vector3>, float, bool>
	{
	}
}
