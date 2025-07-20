using System.Collections.Generic;
using Kampai.Game;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class BuildMenuDefinitionLoadedSignal : Signal<Dictionary<StoreItemType, List<Definition>>>
	{
	}
}
