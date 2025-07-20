using System;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreateNamedCharacterViewCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CreateNamedCharacterViewCommand") as IKampaiLogger;

		[Inject]
		public NamedCharacter namedCharacter { get; set; }

		[Inject]
		public INamedCharacterBuilder builder { get; set; }

		[Inject]
		public InitCharacterObjectSignal initSignal { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject namedCharacterManager { get; set; }

		[Inject]
		public AddNamedCharacterSignal addCharacterSignal { get; set; }

		[Inject]
		public MapAnimationEventSignal mapAnimationEventSignal { get; set; }

		public override void Execute()
		{
			if (namedCharacter == null || namedCharacter.Definition == null)
			{
				logger.Error("Unable to load named character");
				return;
			}
			namedCharacter.Created = true;
			NamedCharacterDefinition definition = namedCharacter.Definition;
			NamedCharacterObject namedCharacterObject = builder.Build(namedCharacter);
			namedCharacterObject.transform.parent = namedCharacterManager.transform;
			initSignal.Dispatch(namedCharacterObject, namedCharacter);
			addCharacterSignal.Dispatch(namedCharacterObject);
			int boundBuildingId = definition.VFXBuildingID;
			if (boundBuildingId <= 0)
			{
				return;
			}
			AnimEventHandler component = namedCharacterObject.GetComponent<AnimEventHandler>();
			if (component != null)
			{
				Action<AnimEventHandler> vFXScriptBinder = delegate(AnimEventHandler animEventHandler)
				{
					mapAnimationEventSignal.Dispatch(animEventHandler, boundBuildingId);
				};
				component.SetVFXScriptBinder(vFXScriptBinder);
			}
			else
			{
				logger.Error("Unable to map VFXBuildingID for {0} because there is no AnimEventHandler", definition.ID);
			}
		}
	}
}
