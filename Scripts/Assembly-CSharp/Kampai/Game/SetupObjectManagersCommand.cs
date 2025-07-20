using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class SetupObjectManagersCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SetupObjectManagersCommand") as IKampaiLogger;

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		public override void Execute()
		{
			MinionManagerView o = CreateManager<MinionManagerView>("Minions", GameElement.MINION_MANAGER);
			base.injectionBinder.Bind<MinionIdleNotifier>().ToValue(o);
			logger.Debug("SetupObjectManagersCommand: Created Minions Manager");
			CreateManager<NamedCharacterManagerView>("NamedCharacters", GameElement.NAMED_CHARACTER_MANAGER);
			logger.Debug("SetupObjectManagersCommand: Created Named Characters Manager");
			CreateManager<VillainManagerView>("Villains", GameElement.VILLAIN_MANAGER);
			logger.Debug("SetupObjectManagersCommand: Created Villain Manager");
		}

		private T CreateManager<T>(string goName, GameElement bindingName) where T : MonoBehaviour
		{
			GameObject gameObject = new GameObject(goName);
			T result = gameObject.AddComponent<T>();
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(bindingName);
			gameObject.transform.parent = contextView.transform;
			return result;
		}
	}
}
