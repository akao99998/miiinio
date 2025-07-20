using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class KeyboardPanMediator : PanMediator
	{
		[Inject]
		public KeyboardPanView view { get; set; }

		[Inject]
		public CameraModel model { get; set; }

		[Inject]
		public CameraUtils cameraUtils { get; set; }

		[Inject]
		public RemoveGagFromPlayerSignal removeGagSignal { get; set; }

		public override void OnResetPanVelocity()
		{
			view.Velocity = Vector3.zero;
		}

		public override void OnGameInput(Vector3 position, int input)
		{
			if (!blocked)
			{
				if (((uint)input & (true ? 1u : 0u)) != 0)
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
			int num = 1;
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
			int num = 1;
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
			}
		}

		public override void SetupAutoPan(Vector3 panTo)
		{
			view.SetupAutoPan(panTo);
		}

		public override void PerformAutoPan(float delta)
		{
			view.PerformAutoPan(delta);
		}

		public override void OnCinematicPan(Tuple<Vector3, float> panInfo, CameraMovementSettings modalSettings, Boxed<Building> building, Boxed<Quest> quest)
		{
			if (isAutoPanning)
			{
				TaskableBuilding taskableBuilding = building.Value as TaskableBuilding;
				if (modalSettings.settings == CameraMovementSettings.Settings.None && taskableBuilding != null)
				{
					removeGagSignal.Dispatch(taskableBuilding.Definition.GagID);
				}
				ReenablePickService();
				return;
			}
			Vector3 panTo = panInfo.Item1;
			float item = panInfo.Item2;
			float num = Vector3.Distance(new Vector3(base.transform.position.x, 0f, base.transform.position.z), panTo);
			if (num <= 1f)
			{
				ShowMenu(modalSettings, building.Value, quest.Value);
				ReenablePickService();
				OnComplete();
				return;
			}
			isAutoPanning = true;
			Go.to(this, item, new GoTweenConfig().floatProp("Fraction", 1f).setEaseType(GoEaseType.SineOut).setUpdateType(GoUpdateType.Update)
				.onBegin(delegate
				{
					toReenable = model.CurrentBehaviours;
					base.disableCameraSignal.Dispatch(model.CurrentBehaviours);
					previousFraction = base.Fraction;
					SetupAutoPan(panTo);
				})
				.onUpdate(delegate
				{
					if (isAutoPanning)
					{
						float delta = base.Fraction - previousFraction;
						PerformAutoPan(delta);
						previousFraction = base.Fraction;
					}
					else
					{
						Go.killAllTweensWithTarget(this);
						base.enableCameraSignal.Dispatch(toReenable);
						base.Fraction = 0f;
						ReenablePickService();
					}
				})
				.onComplete(delegate
				{
					isAutoPanning = false;
					base.enableCameraSignal.Dispatch(toReenable);
					base.Fraction = 0f;
					ReenablePickService();
					if (building.Value != null || quest.Value != null)
					{
						ShowMenu(modalSettings, building.Value, quest.Value);
					}
					OnComplete();
				}));
		}
	}
}
