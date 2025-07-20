using strange.extensions.command.impl;

namespace Kampai.Common.Controller
{
	public class SetupNativeAlertManagerCommand : Command
	{
		public override void Execute()
		{
			NativeAlertManager.Init();
		}
	}
}
