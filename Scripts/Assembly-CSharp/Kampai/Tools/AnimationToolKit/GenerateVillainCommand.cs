using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class GenerateVillainCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("GenerateVillainCommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public INamedCharacterBuilder villainBuilder { get; set; }

		[Inject(AnimationToolKitElement.VILLAINS)]
		public GameObject VillainGroup { get; set; }

		[Inject]
		public VillainCreatedSignal villainCreatedSignal { get; set; }

		[Inject]
		public int VillainId { get; set; }

		public override void Execute()
		{
			VillainDefinition villainDefinition = definitionService.Get<VillainDefinition>(VillainId);
			Villain villain = new Villain(villainDefinition);
			playerService.Add(villain);
			NamedCharacterObject namedCharacterObject = villainBuilder.Build(villain, VillainGroup);
			namedCharacterObject.transform.parent = VillainGroup.transform;
			namedCharacterObject.Init(villain, logger);
			RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(villainDefinition.CharacterAnimations.StateMachine);
			namedCharacterObject.SetDefaultAnimController(runtimeAnimatorController);
			namedCharacterObject.SetAnimController(runtimeAnimatorController);
			VillainView component = namedCharacterObject.GetComponent<VillainView>();
			if (component != null)
			{
				villainCreatedSignal.Dispatch(component);
			}
		}
	}
}
