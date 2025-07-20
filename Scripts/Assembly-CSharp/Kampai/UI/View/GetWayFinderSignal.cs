using System;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class GetWayFinderSignal : Signal<int, Action<int, IWayFinderView>>
	{
	}
}
