using System;
using Kampai.Game.View;
using strange.extensions.signal.impl;

namespace Kampai.Common
{
	public class TryHarvestBuildingSignal : Signal<BuildingObject, Action, bool>
	{
	}
}
