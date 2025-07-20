using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class SavePlayerSignal : Signal<Tuple<SaveLocation, string, bool>>
	{
	}
}
