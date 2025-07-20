using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Trigger;
using Kampai.Main;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class TSMGiftModalMediator : UIStackMediator<TSMGiftModalView>, IDefinitionsHotSwapHandler
	{
		private TriggerInstance instance;

		private RewardTriggerSignal rewardTriggerSignal;

		private CloseTSMModalSignal closeModalSignal;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public MoveAudioListenerSignal moveAudioListenerSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void Initialize(GUIArguments args)
		{
			base.Initialize(args);
			instance = args.Get<TriggerInstance>();
			base.view.InitializeView(instance, fancyUIService, guiService, moveAudioListenerSignal);
			playSFXSignal.Dispatch("Play_menu_popUp_01");
		}

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.OnRewardCollected.AddListener(OnCollectReward);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			rewardTriggerSignal = gameContext.injectionBinder.GetInstance<RewardTriggerSignal>();
			closeModalSignal = gameContext.injectionBinder.GetInstance<CloseTSMModalSignal>();
			closeModalSignal.AddListener(OnMenuClose);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.OnRewardCollected.RemoveListener(OnCollectReward);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			if (closeModalSignal != null)
			{
				closeModalSignal.RemoveListener(OnMenuClose);
			}
		}

		protected override void Close()
		{
			moveAudioListenerSignal.Dispatch(true, null);
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			base.view.Release();
			base.view.Close();
		}

		private void OnCollectReward(TriggerRewardDefinition triggerReward)
		{
			if (rewardTriggerSignal != null && triggerReward != null)
			{
				rewardTriggerSignal.Dispatch(instance, triggerReward);
			}
		}

		private void OnMenuClose()
		{
			hideSkrimSignal.Dispatch("ProceduralTaskSkrim");
			guiService.Execute(GUIOperation.Unload, "popup_TSM_Gift_Upsell");
		}

		public void OnDefinitionsHotSwap(IDefinitionService definitionService)
		{
			Close();
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "popup_TSM_Gift_Upsell");
			iGUICommand.skrimScreen = "ProceduralTaskSkrim";
			iGUICommand.darkSkrim = true;
			IList<TriggerDefinition> triggerDefinitions = definitionService.GetTriggerDefinitions();
			TriggerDefinition definition = null;
			foreach (TriggerDefinition item in triggerDefinitions)
			{
				if (item.ID == instance.ID)
				{
					definition = item;
					break;
				}
			}
			instance.OnDefinitionHotSwap(definition);
			iGUICommand.Args.Add(typeof(TriggerInstance), instance);
			guiService.Execute(iGUICommand);
		}
	}
}
