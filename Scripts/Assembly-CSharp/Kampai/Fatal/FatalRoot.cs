using UnityEngine;
using strange.extensions.context.impl;

namespace Kampai.Fatal
{
	public class FatalRoot : ContextView
	{
		private void Awake()
		{
			Screen.sleepTimeout = -2;
		}

		private void Start()
		{
			context = new FatalContext(this, false);
			context.Start();
		}
	}
}
