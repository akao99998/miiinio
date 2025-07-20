using Kampai.Main;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ShowNeedXMinionsCommand : Command
	{
		[Inject]
		public int numMinionsNeeded { get; set; }

		[Inject]
		public PopupMessageSignal messageSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		public override void Execute()
		{
			string @string = localService.GetString("NeedsXMinions*", numMinionsNeeded);
			messageSignal.Dispatch(@string, PopupMessageType.NORMAL);
		}
	}
}
