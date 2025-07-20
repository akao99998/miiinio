using strange.extensions.context.impl;

namespace Kampai.Game.Mignette.EdwardMinionHands
{
	public class EdwardMinionHandsMignetteRoot : ContextView
	{
		private void Awake()
		{
			context = new EdwardMinionHandsMignetteContext(this, true);
			context.Start();
		}
	}
}
