using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class DisplayDLCDialogCommand : Command
	{
		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject(MainElement.CONTEXT)]
		public ICrossContextCapable mainContext { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public SavePlayerSignal savePlayerSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSoundFXSignal { get; set; }

		public override void Execute()
		{
			Signal<bool> signal = new Signal<bool>();
			signal.AddListener(delegate(bool result)
			{
				ConfirmationCallback(result);
			});
			PopupConfirmationSetting type = new PopupConfirmationSetting("popupConfirmationDefaultTitle", "DLCConfirmationDialog", "img_char_Min_FeedbackChecklist01", signal);
			gameContext.injectionBinder.GetInstance<QueueConfirmationSignal>().Dispatch(type);
			playSoundFXSignal.Dispatch("Play_training_popUp_01");
		}

		public void ConfirmationCallback(bool result)
		{
			if (result)
			{
				savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, string.Empty, true));
				string displayQualityLevel = dlcService.GetDisplayQualityLevel();
				if (displayQualityLevel.Equals("DLCHDPack"))
				{
					dlcService.SetDisplayQualityLevel("DLCSDPack");
				}
				else
				{
					dlcService.SetDisplayQualityLevel("DLCHDPack");
				}
				mainContext.injectionBinder.GetInstance<ReloadGameSignal>().Dispatch();
			}
		}
	}
}
