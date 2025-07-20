using Kampai.Game.Mignette.View;
using Kampai.UI.View;
using UnityEngine;

namespace Kampai.Game.Mignette.WaterSlide.View
{
	public class WaterSlideMignetteManagerMediator : MignetteManagerMediator<WaterSlideMignetteManagerView>
	{
		private bool mouseDown;

		[Inject]
		public MignetteDooberSpawnedSignal mignetteDooberSpawnedSignal { get; set; }

		public override string MusicEventName
		{
			get
			{
				return "Play_MUS_waterslide_01";
			}
		}

		public override void OnRegister()
		{
			base.OnRegister();
			mignetteDooberSpawnedSignal.AddListener(TrackMignetteDoober);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			mignetteDooberSpawnedSignal.RemoveListener(TrackMignetteDoober);
		}

		protected override void RequestStopMignette(bool showScore)
		{
			base.view.ResetMignetteObjects();
			base.view.ResetCameraAndStopMignette(showScore);
		}

		protected override void OnPress(Vector3 pos, int input, bool pressed)
		{
			if (pressed && !mouseDown)
			{
				mouseDown = true;
				base.view.OnScreenTapped();
			}
			else if (!pressed && mouseDown)
			{
				mouseDown = false;
			}
		}

		private void TrackMignetteDoober(GameObject gameObject)
		{
			base.view.mignetteDooberGO = gameObject;
		}
	}
}
