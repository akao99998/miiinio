using System.Collections.Generic;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Common
{
	public class ProgressBarSignal : Signal<string, GameObject, KeyValuePair<int, int>, int>
	{
	}
}
