using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	internal sealed class InitCharacterObjectCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("InitCharacterObjectCommand") as IKampaiLogger;

		[Inject]
		public CharacterObject characterObject { get; set; }

		[Inject]
		public Character character { get; set; }

		public override void Execute()
		{
			MonoBehaviour[] components = characterObject.GetComponents<MonoBehaviour>();
			MonoBehaviour[] array = components;
			foreach (MonoBehaviour target in array)
			{
				base.injectionBinder.injector.Inject(target, false);
			}
			characterObject.Init(character, logger);
		}
	}
}
