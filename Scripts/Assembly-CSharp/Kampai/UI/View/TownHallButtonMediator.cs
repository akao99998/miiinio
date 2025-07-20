using Kampai.Game;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class TownHallButtonMediator : Mediator
	{
		[Inject]
		public TownHallButtonView view { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void OnRegister()
		{
			view.ClickedSignal.AddListener(ButtonClicked);
		}

		public override void OnRemove()
		{
			view.ClickedSignal.RemoveListener(ButtonClicked);
		}

		private void ButtonClicked()
		{
			PanInstructions type = new PanInstructions(313);
			gameContext.injectionBinder.GetInstance<CameraAutoMoveToInstanceSignal>().Dispatch(type, new Boxed<ScreenPosition>(new ScreenPosition()));
		}
	}
}
