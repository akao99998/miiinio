using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class NamedCharacterManagerMediator : EventMediator
	{
		[Inject]
		public NamedCharacterManagerView view { get; set; }

		[Inject]
		public AddNamedCharacterSignal addSignal { get; set; }

		public override void OnRegister()
		{
			addSignal.AddListener(AddNamedCharacter);
		}

		public override void OnRemove()
		{
			addSignal.RemoveListener(AddNamedCharacter);
		}

		private void AddNamedCharacter(NamedCharacterObject character)
		{
			view.Add(character);
		}
	}
}
