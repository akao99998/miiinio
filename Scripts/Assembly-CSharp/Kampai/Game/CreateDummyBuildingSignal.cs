using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class CreateDummyBuildingSignal : Signal<BuildingDefinition, Vector3, bool>
	{
	}
}
