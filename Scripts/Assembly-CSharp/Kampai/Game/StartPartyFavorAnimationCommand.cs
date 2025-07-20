using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartPartyFavorAnimationCommand : Command
	{
		[Inject]
		public IPartyFavorAnimationService partyFavorService { get; set; }

		public override void Execute()
		{
			partyFavorService.CreateRandomPartyFavor();
		}
	}
}
