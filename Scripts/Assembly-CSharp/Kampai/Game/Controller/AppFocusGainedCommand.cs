using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game.Controller
{
	public class AppFocusGainedCommand : Command
	{
		[Inject]
		public MuteVolumeSignal muteVolumeSignal { get; set; }

		[Inject]
		public AppFocusGainedCompletedSignal appFocusGainedCompleteSignal { get; set; }

		public override void Execute()
		{
			ScreenUtils.ToggleAutoRotation(true);
			appFocusGainedCompleteSignal.Dispatch();
		}
	}
}
