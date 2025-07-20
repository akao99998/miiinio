using strange.extensions.context.impl;

namespace Kampai.Game
{
	public class GameRoot : ContextView
	{
		private void Awake()
		{
			context = new GameContext(this, true);
			context.Start();
		}
	}
}
