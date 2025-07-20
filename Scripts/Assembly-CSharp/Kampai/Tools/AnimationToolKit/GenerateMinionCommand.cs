using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Tools.AnimationToolKit
{
	public class GenerateMinionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("GenerateMinionCommand") as IKampaiLogger;

		private int[] CostumeIDs = new int[3] { 99, 104, 105 };

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject ContextView { get; set; }

		[Inject(AnimationToolKitElement.MINIONS)]
		public GameObject MinionGroup { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IMinionBuilder minionBuilder { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public MinionCreatedSignal minionCreatedSignal { get; set; }

		public override void Execute()
		{
			int id = GameConstants.MINION_DEFINITION_IDS[Random.Range(0, GameConstants.MINION_DEFINITION_IDS.Length)];
			MinionDefinition def = definitionService.Get<MinionDefinition>(id);
			Minion minion = new Minion(def);
			int id2 = CostumeIDs[Random.Range(0, CostumeIDs.Length)];
			CostumeItemDefinition costume = definitionService.Get<CostumeItemDefinition>(id2);
			minionBuilder.SetLOD(TargetPerformance.HIGH);
			MinionObject minionObject = minionBuilder.BuildMinion(costume, "asm_minion_movement", ContextView);
			minionObject.transform.parent = MinionGroup.transform;
			minion.Name = minionObject.name;
			playerService.Add(minion);
			minionObject.Init(minion, logger);
			Agent component = minionObject.GetComponent<Agent>();
			component.enabled = false;
			RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>("asm_minion_movement");
			minionObject.SetDefaultAnimController(runtimeAnimatorController);
			minionObject.SetAnimController(runtimeAnimatorController);
			base.injectionBinder.injector.Inject(minionObject, false);
			minionCreatedSignal.Dispatch(minionObject);
		}
	}
}
