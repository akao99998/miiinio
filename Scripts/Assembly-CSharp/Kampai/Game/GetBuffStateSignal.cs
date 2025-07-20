using System;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class GetBuffStateSignal : Signal<BuffType, Action<float>>
	{
	}
}
