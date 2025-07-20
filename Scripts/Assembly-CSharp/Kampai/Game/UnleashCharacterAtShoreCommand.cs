using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class UnleashCharacterAtShoreCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UnleashCharacterAtShoreCommand") as IKampaiLogger;

		[Inject]
		public Character minionCharacter { get; set; }

		[Inject]
		public int openSlot { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject namedCharacterManager { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public PhilPlayIntroSignal playIntroSignal { get; set; }

		[Inject]
		public PopUnleashedCharacterToTikiBarSignal popCustomerToTikiBarSignal { get; set; }

		public override void Execute()
		{
			CharacterObject characterObject = null;
			bool type = false;
			if (minionCharacter is Minion)
			{
				MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
				characterObject = component.Get(minionCharacter.ID);
				Minion minion = minionCharacter as Minion;
				if (minion.HasPrestige)
				{
					type = true;
				}
			}
			else if (minionCharacter is NamedCharacter)
			{
				NamedCharacterManagerView component2 = namedCharacterManager.GetComponent<NamedCharacterManagerView>();
				characterObject = component2.Get(minionCharacter.ID);
				type = true;
			}
			if (characterObject == null)
			{
				logger.Error("AddMinionToTikiBarSlot: ao as MinionObject and NamedCharacterObject == null");
				return;
			}
			popCustomerToTikiBarSignal.Dispatch(characterObject, openSlot);
			playIntroSignal.Dispatch(type);
		}
	}
}
