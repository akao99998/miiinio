using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	internal sealed class CharacterIntroCompleteAction : KampaiAction
	{
		private int slotIndex;

		private CharacterObject characterObject;

		private RuntimeAnimatorController stateMachine;

		private CharacterIntroCompleteSignal introCompleteSignal;

		public CharacterIntroCompleteAction(CharacterObject minionObj, int slotIndex, RuntimeAnimatorController stateMachine, CharacterIntroCompleteSignal introCompleteSignal, IKampaiLogger logger)
			: base(logger)
		{
			this.slotIndex = slotIndex;
			characterObject = minionObj;
			this.stateMachine = stateMachine;
			this.introCompleteSignal = introCompleteSignal;
		}

		public override void Execute()
		{
			characterObject.MoveToPelvis();
			characterObject.SetAnimController(stateMachine);
			introCompleteSignal.Dispatch(characterObject, slotIndex);
			base.Done = true;
		}
	}
}
