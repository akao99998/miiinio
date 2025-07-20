using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class PelvisAnimationCompleteAction : KampaiAction
	{
		private CharacterObject characterObj;

		private RuntimeAnimatorController controller;

		public PelvisAnimationCompleteAction(IKampaiLogger logger, CharacterObject characterObj, RuntimeAnimatorController controller = null)
			: base(logger)
		{
			this.characterObj = characterObj;
			this.controller = controller;
		}

		public override void Execute()
		{
			characterObj.MoveToPelvis();
			if (controller != null)
			{
				characterObj.SetAnimController(controller);
			}
		}

		public override void LateUpdate()
		{
			characterObj.UpdateBlobShadowPosition();
			base.Done = true;
		}
	}
}
