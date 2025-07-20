using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class AddMinionToTikiBarCommand : Command
	{
		[Inject]
		public Character minionCharacter { get; set; }

		[Inject]
		public TikiBarBuilding tikiBar { get; set; }

		[Inject]
		public int openSlot { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		public override void Execute()
		{
			Minion minion = minionCharacter as Minion;
			if (minion != null)
			{
				AddCostumedMinionToTikiBar(minion);
				return;
			}
			NamedCharacter namedCharacter = minionCharacter as NamedCharacter;
			if (namedCharacter != null)
			{
				AddNamedCharacterToTikiBar(namedCharacter);
			}
		}

		private void AddCostumedMinionToTikiBar(Minion minion)
		{
			if (minion.State != MinionState.Tasking)
			{
				prestigeService.AddMinionToTikiBarSlot(minion, openSlot, tikiBar, true);
			}
		}

		private void AddNamedCharacterToTikiBar(NamedCharacter namedCharacter)
		{
			prestigeService.AddMinionToTikiBarSlot(namedCharacter, openSlot, tikiBar);
		}
	}
}
