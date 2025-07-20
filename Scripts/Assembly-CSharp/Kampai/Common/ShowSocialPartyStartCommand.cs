using Kampai.Game;
using Kampai.UI.View;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Common
{
	public class ShowSocialPartyStartCommand : Command
	{
		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public ShowSocialPartyFillOrderSignal showSocialPartyFillOrderSignal { get; set; }

		public override void Execute()
		{
			Signal<SocialTeamResponse, ErrorResponse> signal = new Signal<SocialTeamResponse, ErrorResponse>();
			signal.AddListener(OnCreateTeamResponse);
			timedSocialEventService.CreateSocialTeam(timedSocialEventService.GetCurrentSocialEvent().ID, signal);
		}

		public void OnCreateTeamResponse(SocialTeamResponse response, ErrorResponse error)
		{
			if (error != null)
			{
				networkConnectionLostSignal.Dispatch();
			}
			else
			{
				showSocialPartyFillOrderSignal.Dispatch(0);
			}
		}
	}
}
