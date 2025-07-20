using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class ShowBuildingFootprintSignal : Signal<ActionableObject, Transform, Tuple<int, int>, bool>
	{
	}
}
