using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	internal sealed class PathToBuildingCompleteAction : KampaiAction
	{
		private CharacterObject characterObject;

		private int slotIndex;

		private Signal<CharacterObject, int> addToBuildingSignal;

		public PathToBuildingCompleteAction(CharacterObject minionObj, int slotIndex, Signal<CharacterObject, int> addSignal, IKampaiLogger logger)
			: base(logger)
		{
			characterObject = minionObj;
			this.slotIndex = slotIndex;
			addToBuildingSignal = addSignal;
		}

		public override void Execute()
		{
			addToBuildingSignal.Dispatch(characterObject, slotIndex);
			base.Done = true;
		}
	}
}
