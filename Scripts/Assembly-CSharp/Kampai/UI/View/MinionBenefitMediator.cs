using Kampai.Game;
using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class MinionBenefitMediator : Mediator
	{
		[Inject]
		public MinionBenefitView view { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public RefreshAllOfTypeArgsSignal refreshAllOfTypeArgsSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void OnRegister()
		{
			view.Init(definitionService, playerService);
			refreshAllOfTypeArgsSignal.AddListener(view.RefreshAllOfTypeArgsCallback);
			view.triggerAbilityBarAudio.AddListener(PlayAbilityAudio);
		}

		public override void OnRemove()
		{
			refreshAllOfTypeArgsSignal.RemoveListener(view.RefreshAllOfTypeArgsCallback);
			view.triggerAbilityBarAudio.RemoveListener(PlayAbilityAudio);
		}

		private void PlayAbilityAudio()
		{
			soundFXSignal.Dispatch("Play_minionUpgrade_minionAbilityBarActivate_01");
		}
	}
}
