using strange.extensions.context.impl;

namespace Kampai.Main
{
	public class WhiteboxRoot : ContextView
	{
		private void Start()
		{
			context = new WhiteboxContext(this, true);
			context.Start();
		}
	}
}
