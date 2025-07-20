using Kampai.Game.Mignette.View;
using UnityEngine;

namespace Kampai.Game.Mignette.AlligatorSkiing.View
{
	public class AlligatorSkiingMignetteManagerMediator : MignetteManagerMediator<AlligatorSkiingMignetteManagerView>
	{
		private bool mouseDown;

		public override string MusicEventName
		{
			get
			{
				return "Play_MUS_alligatorSki_01";
			}
		}

		protected override void OnPress(Vector3 pos, int input, bool pressed)
		{
			if (pressed && !mouseDown)
			{
				mouseDown = true;
				base.view.OnInputDown();
			}
			else if (!pressed && mouseDown)
			{
				mouseDown = false;
				base.view.OnInputUp();
			}
		}

		protected override void RequestStopMignette(bool showScore)
		{
			base.view.ResetMignetteObjects();
			base.view.ResetCameraAndStopMignette(showScore);
		}
	}
}
