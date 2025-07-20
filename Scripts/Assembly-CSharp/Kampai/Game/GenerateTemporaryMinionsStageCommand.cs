using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class GenerateTemporaryMinionsStageCommand : Command
	{
		public const float RANDOM_MINION_START_OFFSET_X = 4.8f;

		public const float RANDOM_MINION_START_OFFSET_Y = 0f;

		public IKampaiLogger logger = LogManager.GetClassLogger("GenerateTemporaryMinionsStageCommand") as IKampaiLogger;

		public int count;

		[Inject]
		public GenerateTemporaryMinionSignal generateTemporaryMinionSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public MoveMinionFinishedSignal moveMinionFinishedSignal { get; set; }

		[Inject]
		public TemporaryMinionsService temporaryMinionsService { get; set; }

		public override void Execute()
		{
			count = 0;
			moveMinionFinishedSignal.AddListener(MinionMoved);
			StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
			int temporaryMinionNum = firstInstanceByDefinitionId.Definition.temporaryMinionNum;
			float temporaryMinionsOffset = firstInstanceByDefinitionId.Definition.temporaryMinionsOffset;
			float num = (0f - (float)temporaryMinionNum * 0.5f) * temporaryMinionsOffset - temporaryMinionsOffset * 0.5f;
			for (int i = 0; i < temporaryMinionNum; i++)
			{
				GenerateTemporaryMinionCommand.TemporaryMinionProperties type = default(GenerateTemporaryMinionCommand.TemporaryMinionProperties);
				type.TempID = ((i != 0) ? (-i - 1) : (-1));
				type.startX = (float)firstInstanceByDefinitionId.Location.x + 4.8f;
				type.startY = (float)firstInstanceByDefinitionId.Location.y + num;
				type.finishX = firstInstanceByDefinitionId.Location.x;
				type.finishY = (float)firstInstanceByDefinitionId.Location.y + num;
				generateTemporaryMinionSignal.Dispatch(type);
				num += temporaryMinionsOffset;
			}
		}

		public void MinionMoved(int id)
		{
			if (id <= 0)
			{
				IDictionary<int, MinionObject> temporaryMinions = temporaryMinionsService.getTemporaryMinions();
				if (temporaryMinions.Count > count)
				{
					StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
					MinionObject minionObject = temporaryMinions[id];
					minionObject.ClearActionQueue();
					RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>(firstInstanceByDefinitionId.Definition.temporaryMinionASM);
					minionObject.EnqueueAction(new SetAnimatorAction(minionObject, controller, logger));
				}
				count++;
			}
		}
	}
}
