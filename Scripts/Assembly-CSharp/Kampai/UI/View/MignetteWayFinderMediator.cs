using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class MignetteWayFinderMediator : AbstractWayFinderMediator
	{
		[Inject]
		public MignetteWayFinderView mignetteWayFinderView { get; set; }

		[Inject(MainElement.CAMERA)]
		public GameObject cameraGO { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return mignetteWayFinderView;
			}
		}

		protected override void PanToInstance()
		{
			if (base.ZoomCameraModel.ZoomedIn)
			{
				return;
			}
			Building byInstanceId = base.PlayerService.GetByInstanceId<Building>(GetTrackedId());
			if (byInstanceId == null)
			{
				return;
			}
			PanInstructions panInstructions = new PanInstructions(byInstanceId);
			if (byInstanceId.Definition.ID == 3502)
			{
				float currentPercentage = cameraGO.GetComponent<ZoomView>().GetCurrentPercentage();
				float zoom = byInstanceId.Definition.ScreenPosition.zoom;
				float num = ((!(zoom < 0f)) ? zoom : currentPercentage);
				if (num > 0.01f)
				{
					panInstructions.Offset = new Boxed<Vector3>(GameConstants.Mignettes.ALLIGATOR_SKIING_WAYFINDER_CAMERA_OFFSET * num);
				}
			}
			base.CameraAutoMoveToInstanceSignal.Dispatch(panInstructions, new Boxed<ScreenPosition>(new ScreenPosition()));
		}
	}
}
