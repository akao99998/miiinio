using strange.extensions.context.impl;

namespace Kampai.Game.Mignette.ButterflyCatch
{
	public class ButterflyCatchMignetteRoot : ContextView
	{
		private void Awake()
		{
			context = new ButterflyCatchMignetteContext(this, true);
			context.Start();
		}
	}
}
