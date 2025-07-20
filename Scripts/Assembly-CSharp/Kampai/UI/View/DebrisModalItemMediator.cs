using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class DebrisModalItemMediator : Mediator
	{
		[Inject]
		public DebrisModalItemView DebrisModalItemView { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera UICamera { get; set; }

		public override void OnRegister()
		{
			DebrisModalItemView.Init(UICamera);
		}
	}
}
