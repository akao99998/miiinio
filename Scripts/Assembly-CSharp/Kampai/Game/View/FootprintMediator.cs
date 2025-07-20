using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class FootprintMediator : Mediator
	{
		[Inject]
		public ShowBuildingFootprintSignal showSignal { get; set; }

		[Inject]
		public UpdateMovementValidity updateSignal { get; set; }

		[Inject]
		public FootprintView view { get; set; }

		private void Init()
		{
			view.Init();
		}

		public override void OnRegister()
		{
			Init();
			showSignal.AddListener(ToggleFootprint);
			updateSignal.AddListener(UpdateFootprint);
		}

		public override void OnRemove()
		{
			showSignal.RemoveListener(ToggleFootprint);
			updateSignal.RemoveListener(UpdateFootprint);
		}

		private void UpdateFootprint(bool valid)
		{
			view.UpdateFootprint(valid);
		}

		private void ToggleFootprint(ActionableObject parent, Transform parentTransform, Tuple<int, int> size, bool enable)
		{
			if (enable)
			{
				view.ParentFootprint(parent, parentTransform, size.Item1, size.Item2);
			}
			else
			{
				view.Reset();
			}
			view.ToggleFootprint(enable);
		}
	}
}
