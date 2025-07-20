using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class MinionLevelSelectorMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MinionLevelSelectorMediator") as IKampaiLogger;

		private PostMinionUpgradeSignal upgradeSignal;

		[Inject]
		public MinionLevelSelectorView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RefreshFromIndexArgsSignal refreshFromIndexArgsSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public override void OnRegister()
		{
			view.Init(playerService, definitionService);
			view.SelectionButton.ClickedSignal.AddListener(Selected);
			refreshFromIndexArgsSignal.AddListener(view.RefreshAllOfTypeArgsCallback);
			upgradeSignal = gameContext.injectionBinder.GetInstance<PostMinionUpgradeSignal>();
			upgradeSignal.AddListener(UpdateCount);
		}

		public override void OnRemove()
		{
			view.SelectionButton.ClickedSignal.RemoveListener(Selected);
			refreshFromIndexArgsSignal.RemoveListener(view.RefreshAllOfTypeArgsCallback);
			upgradeSignal.RemoveListener(UpdateCount);
			upgradeSignal = null;
		}

		private void Selected()
		{
			refreshFromIndexArgsSignal.Dispatch(view.GetType(), view.index, new GUIArguments(logger));
			soundFXSignal.Dispatch("Play_button_click_01");
		}

		private void UpdateCount()
		{
			view.UpdateMinionCountText();
		}
	}
}
