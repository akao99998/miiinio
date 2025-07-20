using Kampai.Game.Mignette.View;
using UnityEngine;

namespace Kampai.Game.Mignette.EdwardMinionHands.View
{
	public class EdwardMinionHandsMignetteManagerMediator : MignetteManagerMediator<EdwardMinionHandsMignetteManagerView>
	{
		private const float MINION_REACT_RADIUS = 15f;

		private bool PreviousPressed;

		public override string MusicEventName
		{
			get
			{
				return "Play_MUS_topiary_01";
			}
		}

		protected override void RequestStopMignette(bool showScore)
		{
			base.view.ResetTree();
			base.view.ResetCameraAndStopMignette(showScore);
		}

		protected override void OnPress(Vector3 pos, int input, bool pressed)
		{
			if (pressed && !PreviousPressed)
			{
				base.view.OnInputDown(pos);
			}
			PreviousPressed = pressed;
		}
	}
}
