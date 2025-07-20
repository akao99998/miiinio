using System.Collections.Generic;
using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class StartMinionPartyCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public PostMinionPartyStartSignal postMinionPartyStartSignal { get; set; }

		[Inject]
		public PlayGlobalMusicSignal musicSignal { get; set; }

		[Inject]
		public DisplayDiscoGlobeSignal displayDiscoGlobeSignal { get; set; }

		[Inject]
		public SetupEndMinionPartyTimerSignal setupEndTimerSignal { get; set; }

		public override void Execute()
		{
			setupEndTimerSignal.Dispatch();
			displayDiscoGlobeSignal.Dispatch(true);
			postMinionPartyStartSignal.Dispatch();
			playerService.UpdateMinionPartyPointValues();
			uiContext.injectionBinder.GetInstance<SetXPSignal>().Dispatch();
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			dictionary.Add("endParty", 0f);
			Dictionary<string, float> type = dictionary;
			musicSignal.Dispatch("Play_partyMeterMusic_01", type);
		}
	}
}
