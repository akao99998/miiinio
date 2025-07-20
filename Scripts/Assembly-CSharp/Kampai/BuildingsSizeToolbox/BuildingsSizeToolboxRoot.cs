using strange.extensions.context.impl;

namespace Kampai.BuildingsSizeToolbox
{
	public class BuildingsSizeToolboxRoot : ContextView
	{
		private void Awake()
		{
			context = new BuildingsSizeToolboxContext(this, true);
			context.Start();
		}
	}
}
