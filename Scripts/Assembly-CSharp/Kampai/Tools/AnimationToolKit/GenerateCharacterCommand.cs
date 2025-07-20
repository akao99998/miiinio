using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class GenerateCharacterCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("GenerateCharacterCommand") as IKampaiLogger;

		[Inject(AnimationToolKitElement.CHARACTERS)]
		public GameObject characterGroup { get; set; }

		[Inject]
		public INamedCharacterBuilder builder { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CharacterCreatedSignal characterCreatedSignal { get; set; }

		[Inject]
		public int namedCharacterId { get; set; }

		public override void Execute()
		{
			NamedCharacterDefinition namedCharacterDefinition = definitionService.Get<NamedCharacterDefinition>(namedCharacterId);
			NamedCharacter namedCharacter = CreateNamedCharacter(namedCharacterDefinition);
			playerService.Add(namedCharacter);
			NamedCharacterObject namedCharacterObject = builder.Build(namedCharacter);
			namedCharacterObject.Init(namedCharacter, logger);
			namedCharacterObject.transform.parent = characterGroup.transform;
			namedCharacterObject.transform.localEulerAngles = new Vector3(0f, 135f, 0f);
			RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(namedCharacterDefinition.CharacterAnimations.StateMachine);
			namedCharacterObject.SetDefaultAnimController(runtimeAnimatorController);
			namedCharacterObject.SetAnimController(runtimeAnimatorController);
			if (namedCharacterDefinition.Location != null)
			{
				Location location = namedCharacterDefinition.Location;
				namedCharacterObject.transform.position = new Vector3(location.x, 0f, location.y);
			}
			Agent component = namedCharacterObject.GetComponent<Agent>();
			if (component != null)
			{
				component.enabled = false;
			}
			base.injectionBinder.injector.Inject(namedCharacterObject, false);
			characterCreatedSignal.Dispatch(namedCharacterObject);
		}

		private NamedCharacter CreateNamedCharacter(NamedCharacterDefinition namedCharacterDefinition)
		{
			NamedCharacter result = null;
			PhilCharacterDefinition philCharacterDefinition = namedCharacterDefinition as PhilCharacterDefinition;
			if (philCharacterDefinition != null)
			{
				result = new PhilCharacter(philCharacterDefinition);
			}
			BobCharacterDefinition bobCharacterDefinition = namedCharacterDefinition as BobCharacterDefinition;
			if (bobCharacterDefinition != null)
			{
				result = new BobCharacter(bobCharacterDefinition);
			}
			KevinCharacterDefinition kevinCharacterDefinition = namedCharacterDefinition as KevinCharacterDefinition;
			if (kevinCharacterDefinition != null)
			{
				result = new KevinCharacter(kevinCharacterDefinition);
			}
			StuartCharacterDefinition stuartCharacterDefinition = namedCharacterDefinition as StuartCharacterDefinition;
			if (stuartCharacterDefinition != null)
			{
				result = new StuartCharacter(stuartCharacterDefinition);
			}
			SpecialEventCharacterDefinition specialEventCharacterDefinition = namedCharacterDefinition as SpecialEventCharacterDefinition;
			if (specialEventCharacterDefinition != null)
			{
				result = new SpecialEventCharacter(specialEventCharacterDefinition);
			}
			return result;
		}
	}
}
