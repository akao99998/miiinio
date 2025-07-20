using strange.extensions.context.impl;

namespace Kampai.ForcedUpgrade
{
	public class ForcedUpgradeRoot : ContextView
	{
		private void Awake()
		{
			context = new ForcedUpgradeContext(this, true);
			context.Start();
		}
	}
}
