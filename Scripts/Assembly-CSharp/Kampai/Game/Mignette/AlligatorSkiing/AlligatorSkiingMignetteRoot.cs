using strange.extensions.context.impl;

namespace Kampai.Game.Mignette.AlligatorSkiing
{
	public class AlligatorSkiingMignetteRoot : ContextView
	{
		private void Awake()
		{
			context = new AlligatorSkiingMignetteContext(this, true);
			context.Start();
		}
	}
}
