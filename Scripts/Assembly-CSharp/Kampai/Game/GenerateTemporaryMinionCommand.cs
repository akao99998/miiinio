using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class GenerateTemporaryMinionCommand : Command
	{
		public struct TemporaryMinionProperties
		{
			public int TempID;

			public float startX;

			public float startY;

			public float finishX;

			public float finishY;
		}

		public IKampaiLogger logger = LogManager.GetClassLogger("GenerateTemporaryMinionCommand") as IKampaiLogger;

		private int[] CostumeIDs = new int[1] { 99 };

		[Inject]
		public TemporaryMinionProperties randomMinionProperties { get; set; }

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject ContextView { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IMinionBuilder minionBuilder { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public MoveMinionFinishedSignal moveMinionFinishedSignal { get; set; }

		[Inject]
		public TemporaryMinionsService temporaryMinionsService { get; set; }

		public override void Execute()
		{
			int id = GameConstants.MINION_DEFINITION_IDS[Random.Range(0, GameConstants.MINION_DEFINITION_IDS.Length)];
			MinionDefinition def = definitionService.Get<MinionDefinition>(id);
			Minion minion = new Minion(def);
			minion.ID = randomMinionProperties.TempID;
			int id2 = CostumeIDs[Random.Range(0, CostumeIDs.Length)];
			CostumeItemDefinition costume = definitionService.Get<CostumeItemDefinition>(id2);
			MinionObject minionObject = minionBuilder.BuildMinion(costume, "asm_minion_movement", ContextView);
			minion.Name = minionObject.name;
			minionObject.Init(minion, logger);
			Agent component = minionObject.GetComponent<Agent>();
			component.enabled = false;
			minionObject.transform.GetComponentInChildren<BoxCollider>().enabled = false;
			RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>("asm_minion_movement");
			minionObject.SetDefaultAnimController(runtimeAnimatorController);
			minionObject.SetAnimController(runtimeAnimatorController);
			minionObject.transform.parent = minionManager.transform;
			float num = Random.Range(0f, 1f);
			minionObject.transform.position = new Vector3(randomMinionProperties.startX + num, 0f, randomMinionProperties.startY);
			Vector3 position = minionObject.transform.position;
			Vector3 goalPos = new Vector3(randomMinionProperties.finishX, position.y, randomMinionProperties.finishY);
			bool muteStatus = true;
			IList<Vector3> path = pathFinder.FindPath(position, goalPos, 4, true);
			MinionManagerView component2 = minionManager.GetComponent<MinionManagerView>();
			component2.StartPathing(minionObject, path, 4.5f, muteStatus, moveMinionFinishedSignal, 0f);
			temporaryMinionsService.addTemporaryMinion(minionObject);
		}
	}
}
