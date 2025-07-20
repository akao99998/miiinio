using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class BeginCharacterIntroLoopCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("BeginCharacterIntroLoopCommand") as IKampaiLogger;

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject namedCharacterManager { get; set; }

		[Inject]
		public BeginCharacterLoopAnimationSignal characterLoopAnimationSignal { get; set; }

		[Inject]
		public Character minionCharacter { get; set; }

		public override void Execute()
		{
			CharacterObject characterObject = null;
			if (minionCharacter is Minion)
			{
				MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
				characterObject = component.Get(minionCharacter.ID);
			}
			else if (minionCharacter is NamedCharacter)
			{
				NamedCharacterManagerView component2 = namedCharacterManager.GetComponent<NamedCharacterManagerView>();
				characterObject = component2.Get(minionCharacter.ID);
			}
			if (characterObject == null)
			{
				logger.Error("AddMinionToTikiBarSlot: ao as MinionObject and NamedCharacterObject == null");
			}
			else
			{
				characterLoopAnimationSignal.Dispatch(characterObject);
			}
		}
	}
}
