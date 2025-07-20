using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class FloatingTextMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("FloatingTextMediator") as IKampaiLogger;

		private ButtonView buttonView;

		[Inject]
		public FloatingTextView view { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public RemoveFloatingTextSignal removeFloatingTextSignal { get; set; }

		[Inject]
		public OpenStorageBuildingSignal openStorageBuildingSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(positionService, gameContext, logger, playerService, localizationService);
			buttonView = GetComponent<ButtonView>();
			buttonView.ClickedSignal.AddListener(OnClick);
			view.OnRemoveSignal.AddListener(OnRemoveFloatingText);
		}

		public override void OnRemove()
		{
			view.OnRemoveSignal.RemoveListener(OnRemoveFloatingText);
			buttonView.ClickedSignal.RemoveListener(OnClick);
		}

		private void OnClick()
		{
			if (view.TrackedId == 314)
			{
				StorageBuilding byInstanceId = playerService.GetByInstanceId<StorageBuilding>(314);
				openStorageBuildingSignal.Dispatch(byInstanceId, true);
			}
		}

		private void OnRemoveFloatingText()
		{
			removeFloatingTextSignal.Dispatch(view.TrackedId);
		}
	}
}
