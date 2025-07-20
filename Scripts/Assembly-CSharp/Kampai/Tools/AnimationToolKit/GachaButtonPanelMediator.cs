using System.Collections.Generic;
using Kampai.Game;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Tools.AnimationToolKit
{
	internal sealed class GachaButtonPanelMediator : Mediator
	{
		[Inject]
		public GachaButtonPanelView view { get; set; }

		[Inject]
		public AnimationToolkitModel model { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public PlayMinionAnimationSignal playGachaSignal { get; set; }

		[Inject]
		public EnableInterfaceSignal enableUISignal { get; set; }

		public override void OnRegister()
		{
			ICollection<MinionAnimationDefinition> all = definitionService.GetAll<MinionAnimationDefinition>();
			ICollection<GachaAnimationDefinition> all2 = definitionService.GetAll<GachaAnimationDefinition>();
			if (model.Mode == AnimationToolKitMode.Character)
			{
				all2.Clear();
				float num = (float)Screen.height - 135f;
				RectTransform rectTransform = base.transform as RectTransform;
				rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0f - num);
			}
			view.Init(all, all2);
			view.SetButtonCallback(OnGachaButtonPressed);
			enableUISignal.AddListener(EnableUI);
		}

		public override void OnRemove()
		{
			enableUISignal.RemoveListener(EnableUI);
		}

		private void OnGachaButtonPressed(AnimationDefinition def)
		{
			playGachaSignal.Dispatch(def);
		}

		private void EnableUI(bool enabled)
		{
			base.gameObject.SetActive(enabled);
		}
	}
}
