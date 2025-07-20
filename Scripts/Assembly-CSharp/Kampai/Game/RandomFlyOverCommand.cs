using Kampai.Common;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RandomFlyOverCommand : Command
	{
		private FlyOverDefinition def;

		[Inject]
		public int index { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFinderSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFinderSignal { get; set; }

		[Inject]
		public ShowAllResourceIconsSignal showAllResourceIconsSignal { get; set; }

		[Inject]
		public HideAllResourceIconsSignal hideAllResourceIconsSignal { get; set; }

		[Inject]
		public RandomFlyOverCompleteSignal randomFlyOverCompleteSignal { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera camera { get; set; }

		public override void Execute()
		{
			def = PickRandomFlyOver();
			GoSpline spline = CreateSpline(def);
			AttachCameraToSpline(spline);
		}

		private FlyOverDefinition PickRandomFlyOver()
		{
			if (index > -1)
			{
				return definitionService.GetAll<FlyOverDefinition>()[index];
			}
			QuantityItem quantityItem = playerService.GetWeightedInstance(4014).NextPick(randomService);
			return definitionService.Get<FlyOverDefinition>(quantityItem.ID);
		}

		private GoSpline CreateSpline(FlyOverDefinition def)
		{
			int count = def.path.Count;
			Vector3[] array = new Vector3[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = new Vector3(def.path[i].x, def.path[i].y, def.path[i].z);
			}
			GoSpline goSpline = new GoSpline(array);
			goSpline.buildPath();
			return goSpline;
		}

		private void AttachCameraToSpline(GoSpline spline)
		{
			PositionPathTweenProperty tweenProp = new PositionPathTweenProperty(spline, false, false, GoLookAtType.TargetTransform);
			GoTweenConfig goTweenConfig = new GoTweenConfig();
			goTweenConfig.addTweenProperty(tweenProp);
			GoTween goTween = new GoTween(camera.transform, def.time, goTweenConfig);
			goTween.setOnUpdateHandler(TweenUpdate);
			goTween.setOnBeginHandler(TweenBegin);
			goTween.setOnCompleteHandler(TweenComplete);
			goTween.easeType = GoEaseType.QuadInOut;
			Go.addTween(goTween);
		}

		private void TweenUpdate(AbstractGoTween tween)
		{
			float num = camera.transform.position.y - 13f;
			float num2 = num / 17f;
			float num3 = 30f;
			camera.transform.eulerAngles = new Vector3(25f + num2 * num3, camera.transform.eulerAngles.y, camera.transform.eulerAngles.z);
			float num4 = 31f;
			camera.fieldOfView = 9f + num2 * num4;
		}

		private void TweenBegin(AbstractGoTween tween)
		{
			hideAllWayFinderSignal.Dispatch();
			hideAllResourceIconsSignal.Dispatch();
			showHUDSignal.Dispatch(false);
			showStoreSignal.Dispatch(false);
			pickControllerModel.PanningCameraBlocked = true;
			pickControllerModel.ZoomingCameraBlocked = true;
		}

		private void TweenComplete(AbstractGoTween tween)
		{
			showHUDSignal.Dispatch(true);
			showStoreSignal.Dispatch(true);
			showAllWayFinderSignal.Dispatch();
			showAllResourceIconsSignal.Dispatch();
			pickControllerModel.PanningCameraBlocked = false;
			pickControllerModel.ZoomingCameraBlocked = false;
			randomFlyOverCompleteSignal.Dispatch();
		}
	}
}
