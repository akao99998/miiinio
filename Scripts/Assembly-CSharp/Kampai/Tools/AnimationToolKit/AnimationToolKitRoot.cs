using strange.extensions.context.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class AnimationToolKitRoot : ContextView
	{
		private void Start()
		{
			context = new AnimationToolKitContext(this, true);
			context.Start();
		}
	}
}
