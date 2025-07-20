using Kampai.Util;
using UnityEngine;
using strange.extensions.context.impl;

namespace Kampai.Splash
{
	public class SplashRoot : ContextView
	{
		private void Awake()
		{
			TimeProfiler.Reset(Debug.isDebugBuild);
			TimeProfiler.StartMonoProfiler("loading");
		}

		private void Start()
		{
			context = new SplashContext(this, true);
			context.Start();
		}
	}
}
