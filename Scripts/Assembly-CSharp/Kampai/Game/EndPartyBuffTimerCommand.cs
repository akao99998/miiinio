using System;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class EndPartyBuffTimerCommand : Command
	{
		[Inject]
		public EndPartyBuffTimerWithCallbackSignal endPartyBuffTimerWithCallBackSignal { get; set; }

		[Inject]
		public UnloadPartyAssetsSignal unloadPartyAssetsSignal { get; set; }

		public override void Execute()
		{
			endPartyBuffTimerWithCallBackSignal.Dispatch(new Boxed<Action>(null));
			unloadPartyAssetsSignal.Dispatch();
		}
	}
}
