using strange.extensions.context.impl;

namespace Kampai.UI.View
{
	public class UIRoot : ContextView
	{
		private void Awake()
		{
			context = new UIContext(this, true);
			context.Start();
		}
	}
}
