using strange.extensions.context.impl;

namespace Kampai.Game.Mignette.WaterSlide
{
	public class WaterSlideMignetteRoot : ContextView
	{
		private void Awake()
		{
			context = new WaterSlideMignetteContext(this, true);
			context.Start();
		}
	}
}
