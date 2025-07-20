using System.Collections.Generic;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class StartMinionPartyUnlockSequenceCommand : Command
	{
		[Inject]
		public UpdatePartyPointButtonsSignal updatePartyPointButtonsSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public MinionPartyUnlockedSignal unlockSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSoundFXSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public CheckMinionPartyLevelSignal checkMinionPartyLevelSignal { get; set; }

		public override void Execute()
		{
			TransactionDefinition transactionDefinition = new TransactionDefinition();
			transactionDefinition.ID = int.MaxValue;
			transactionDefinition.Outputs = new List<QuantityItem>();
			QuantityItem quantityItem = new QuantityItem();
			quantityItem.ID = 387;
			quantityItem.Quantity = 1u;
			transactionDefinition.Outputs.Add(quantityItem);
			transactionDefinition.Inputs = new List<QuantityItem>();
			playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.NO_VISUAL, null);
			checkMinionPartyLevelSignal.Dispatch(false);
			updatePartyPointButtonsSignal.Dispatch();
			playerService.UpdateMinionPartyPointValues();
			unlockSignal.Dispatch();
			DisplayOnBoardingSkrim();
			uiContext.injectionBinder.GetInstance<StartLeisurePartyPointsFinishedSignal>().AddOnce(DisplayFunMeter);
		}

		private void DisplayFunMeter()
		{
			playSoundFXSignal.Dispatch("Play_partyMeter_barSpawn_01");
			uiContext.injectionBinder.GetInstance<CreateFunMeterSignal>().Dispatch();
			uiContext.injectionBinder.GetInstance<CreatePartyMeterSignal>().Dispatch();
		}

		private void DisplayOnBoardingSkrim()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "PartyOnboarding");
			iGUICommand.skrimScreen = "PartyOnboardSkrim";
			iGUICommand.darkSkrim = true;
			guiService.Execute(iGUICommand);
		}
	}
}
