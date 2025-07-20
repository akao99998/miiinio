using Kampai.Game.View;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MignetteCallMinionsCommand : Command
	{
		private bool ownsMinigamePack;

		private int numMinionsToCall;

		private MignetteBuilding mignetteBuilding;

		private GameObject signalSender;

		[Inject]
		public MignetteCallMinionsModel model { get; set; }

		[Inject]
		public EnableSkrimButtonSignal enableSkrimButtonSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public GenerateTemporaryMinionSignal generateTemporaryMinionSignal { get; set; }

		[Inject]
		public TemporaryMinionsService tempMinionService { get; set; }

		[Inject]
		public StartMinionTaskSignal startMinionTaskSignal { get; set; }

		[Inject]
		public CallMinionSignal callMinionSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		private void Init()
		{
			ownsMinigamePack = playerService.HasPurchasedMinigamePack();
			numMinionsToCall = model.NumberOfMinionsToCall;
			mignetteBuilding = model.Building;
			signalSender = model.SignalSender;
		}

		public override void Execute()
		{
			Init();
			enableSkrimButtonSignal.Dispatch(false);
			hideAllWayFindersSignal.Dispatch();
			showHUDSignal.Dispatch(false);
			showStoreSignal.Dispatch(false);
			if (ownsMinigamePack)
			{
				for (int i = 0; i < numMinionsToCall; i++)
				{
					GenerateTemporaryMinionCommand.TemporaryMinionProperties type = default(GenerateTemporaryMinionCommand.TemporaryMinionProperties);
					type.TempID = -100 - i;
					type.startX = mignetteBuilding.Location.x;
					type.startY = mignetteBuilding.Location.y;
					type.finishX = mignetteBuilding.Location.x;
					type.finishY = mignetteBuilding.Location.y;
					generateTemporaryMinionSignal.Dispatch(type);
				}
			}
			for (int j = 0; j < numMinionsToCall; j++)
			{
				if (ownsMinigamePack)
				{
					MinionObject minion = tempMinionService.GetMinion(-100 - j);
					if (mignetteBuilding.Definition.ID == 3509)
					{
						minion.transform.GetComponentInChildren<BoxCollider>().enabled = true;
					}
					startMinionTaskSignal.Dispatch(new Tuple<int, MinionObject, int>(mignetteBuilding.ID, minion, timeService.CurrentTime()));
				}
				else
				{
					callMinionSignal.Dispatch(mignetteBuilding, signalSender);
				}
			}
		}
	}
}
