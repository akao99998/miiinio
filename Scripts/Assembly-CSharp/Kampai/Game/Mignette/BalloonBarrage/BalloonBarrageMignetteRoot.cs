using strange.extensions.context.impl;

namespace Kampai.Game.Mignette.BalloonBarrage
{
	public class BalloonBarrageMignetteRoot : ContextView
	{
		private void Awake()
		{
			context = new BalloonBarrageMignetteContext(this, true);
			context.Start();
		}
	}
}
