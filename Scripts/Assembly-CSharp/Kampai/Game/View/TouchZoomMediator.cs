using Kampai.Common;
using UnityEngine;

namespace Kampai.Game.View
{
	public class TouchZoomMediator : ZoomMediator
	{
		[Inject]
		public TouchZoomView view { get; set; }

		[Inject]
		public CameraModel model { get; set; }

		[Inject]
		public CameraUtils cameraUtils { get; set; }

		[Inject]
		public ZoomPercentageSignal zoomSignal { get; set; }

		[Inject]
		public RequestZoomPercentageSignal requestSignal { get; set; }

		public override void OnRegister()
		{
			view.zoomSignal.AddListener(DispatchZoom);
			requestSignal.AddListener(RequestZoom);
			base.OnRegister();
		}

		public override void OnRemove()
		{
			base.OnRemove();
			view.zoomSignal.RemoveListener(DispatchZoom);
			requestSignal.RemoveListener(RequestZoom);
		}

		public override void OnGameInput(Vector3 position, int input)
		{
			if (!blocked)
			{
				if (input > 1)
				{
					view.CalculateBehaviour(position);
				}
				else
				{
					view.ResetBehaviour();
				}
				view.PerformBehaviour(cameraUtils);
				view.Decay();
			}
		}

		public override void OnDisableBehaviour(int behaviour)
		{
			int num = 2;
			if ((behaviour & num) == num)
			{
				if (!blocked)
				{
					blocked = true;
					view.ResetBehaviour();
				}
				if ((model.CurrentBehaviours & num) == num)
				{
					model.CurrentBehaviours ^= num;
				}
			}
		}

		public override void OnEnableBehaviour(int behaviour)
		{
			int num = 2;
			if ((behaviour & num) == num)
			{
				if (blocked)
				{
					blocked = false;
				}
				if ((model.CurrentBehaviours & num) != num)
				{
					model.CurrentBehaviours ^= num;
				}
				view.UpdateFraction();
			}
		}

		private void DispatchZoom(float zoomPercent)
		{
			zoomSignal.Dispatch(zoomPercent);
		}

		private void RequestZoom()
		{
			zoomSignal.Dispatch(view.fraction);
		}

		public override void SetupAutoZoom(float zoomTo)
		{
			view.SetupAutoZoom(zoomTo);
		}

		public override void PerformAutoZoom(float delta)
		{
			view.PerformAutoZoom(delta);
		}

		public override ZoomView GetView()
		{
			return view;
		}
	}
}
