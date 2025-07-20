using System.Collections.Generic;
using Kampai.Game.View;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class InitBuildingObjectSignal : Signal<BuildingObject, Dictionary<string, RuntimeAnimatorController>, Building>
	{
	}
}
