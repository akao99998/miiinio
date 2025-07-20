using Kampai.Game;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Main
{
	public class PlayMignetteMusicSignal : Signal<GameObject, string, PlayMignetteMusicCommand.MusicEvent>
	{
	}
}
