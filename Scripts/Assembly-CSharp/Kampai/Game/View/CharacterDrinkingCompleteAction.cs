using Kampai.Util;

namespace Kampai.Game.View
{
	internal sealed class CharacterDrinkingCompleteAction : KampaiAction
	{
		private CharacterObject characterObject;

		private CharacterDrinkingCompleteSignal drinkingCompleteSignal;

		public CharacterDrinkingCompleteAction(CharacterObject minionObj, CharacterDrinkingCompleteSignal drinkingCompleteSignal, IKampaiLogger logger)
			: base(logger)
		{
			characterObject = minionObj;
			this.drinkingCompleteSignal = drinkingCompleteSignal;
		}

		public override void Execute()
		{
			drinkingCompleteSignal.Dispatch(characterObject.ID);
			base.Done = true;
		}
	}
}
