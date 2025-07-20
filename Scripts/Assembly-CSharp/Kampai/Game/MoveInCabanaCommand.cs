using System.Collections.Generic;
using Kampai.Common;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MoveInCabanaCommand : Command
	{
		private int villainId;

		private CabanaBuilding cabana;

		private TikiBarBuilding tikiBar;

		[Inject]
		public Prestige prestige { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PromptReceivedSignal receivedSignal { get; set; }

		[Inject]
		public CameraAutoMoveToInstanceSignal cameraMoveSignal { get; set; }

		[Inject]
		public KevinGreetVillainSignal kevinGreetSignal { get; set; }

		[Inject]
		public VillainPlayWelcomeSignal villainPlayWelcomeSignal { get; set; }

		[Inject]
		public VillainGotoCabanaSignal villainGotoCabanaSignal { get; set; }

		[Inject]
		public ShowDialogSignal showDialogSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingStateSignal { get; set; }

		[Inject]
		public RecreateBuildingSignal recreateSignal { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public AddMinionToTikiBarSignal tikiSignal { get; set; }

		[Inject]
		public VillainGotoCarpetSignal gotoCarpetSignal { get; set; }

		public override void Execute()
		{
			int trackedInstanceId = prestige.trackedInstanceId;
			Villain byInstanceId = playerService.GetByInstanceId<Villain>(trackedInstanceId);
			CabanaBuilding emptyCabana = prestigeService.GetEmptyCabana();
			if (emptyCabana != null)
			{
				MoveIn(emptyCabana, byInstanceId);
			}
		}

		private void MoveIn(CabanaBuilding building, Villain villain)
		{
			buildingStateSignal.Dispatch(building.ID, BuildingState.Working);
			recreateSignal.Dispatch(building);
			villain.CabanaBuildingId = building.ID;
			building.Occupied = true;
			gotoCarpetSignal.Dispatch(villain.ID);
			RunWelcomeFlow(villain, building);
		}

		private void RunWelcomeFlow(Villain villain, CabanaBuilding building)
		{
			kevinGreetSignal.Dispatch(true);
			villainPlayWelcomeSignal.Dispatch(villain.ID);
			QuestDialogSetting questDialogSetting = new QuestDialogSetting();
			questDialogSetting.definitionID = prestige.Definition.ID;
			questDialogSetting.type = QuestDialogType.NORMAL;
			QuestDialogSetting type = questDialogSetting;
			villainId = villain.ID;
			cabana = building;
			receivedSignal.AddOnce(HandleReceived);
			showDialogSignal.Dispatch(villain.Definition.WelcomeDialogKey, type, new Tuple<int, int>(-1, -1));
		}

		private void HandleReceived(int questId, int stepId)
		{
			kevinGreetSignal.Dispatch(false);
			villainGotoCabanaSignal.Dispatch(villainId, cabana.ID);
			PanInstructions panInstructions = new PanInstructions(cabana.ID);
			panInstructions.ZoomDistance = new Boxed<float>(0.4f);
			cameraMoveSignal.Dispatch(panInstructions, new Boxed<ScreenPosition>(new ScreenPosition()));
			TeleportToTikiBar();
		}

		private void TeleportToTikiBar()
		{
			Prestige firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Prestige>(40004);
			if (firstInstanceByDefinitionId.state != PrestigeState.Questing)
			{
				return;
			}
			KevinCharacter firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<KevinCharacter>(70003);
			IList<Instance> instancesByDefinition = playerService.GetInstancesByDefinition<TikiBarBuildingDefinition>();
			if (instancesByDefinition != null && instancesByDefinition.Count != 0)
			{
				tikiBar = instancesByDefinition[0] as TikiBarBuilding;
				int minionSlotIndex = tikiBar.GetMinionSlotIndex(firstInstanceByDefinitionId.Definition.ID);
				if (minionSlotIndex != -1)
				{
					tikiSignal.Dispatch(tikiBar, firstInstanceByDefinitionId2, firstInstanceByDefinitionId, minionSlotIndex);
				}
			}
		}
	}
}
