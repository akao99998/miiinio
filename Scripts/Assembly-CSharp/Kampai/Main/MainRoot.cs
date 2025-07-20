using Kampai.Util;
using strange.extensions.context.impl;

namespace Kampai.Main
{
	public class MainRoot : ContextView
	{
		private void Start()
		{
			TimeProfiler.StartSection("main");
			context = new MainContext(this, true);
			context.Start();
		}
	}
}
