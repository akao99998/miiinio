using System.Collections;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class StartPartyPopupMediator : UIStackMediator<StartPartyPopupView>
	{
		private bool haveAPrestigeCharacter;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IGuestOfHonorService guestOfHonorService { get; set; }

		[Inject]
		public StopAutopanSignal stopPan { get; set; }

		[Inject]
		public ShowMinionPartySkipButtonSignal showSkipButtonSignal { get; set; }

		public override void OnRegister()
		{
			base.view.accept.ClickedSignal.AddListener(Proceed);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			if (!guestOfHonorService.PartyShouldProduceBuff())
			{
				base.view.PulseStartButton();
				return;
			}
			haveAPrestigeCharacter = true;
			base.view.accept.gameObject.SetActive(false);
			base.view.CenterHeader();
			StartCoroutine(TransitionOut());
		}

		public override void OnRemove()
		{
			base.view.accept.ClickedSignal.RemoveListener(Proceed);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
		}

		public override void Initialize(GUIArguments args)
		{
			Init();
			soundFXSignal.Dispatch("Play_main_menu_open_01");
			soundFXSignal.Dispatch("Play_startParty_popUp_01");
		}

		protected override void OnCloseAllMenu(GameObject exception)
		{
		}

		protected override void Close()
		{
		}

		private void Proceed()
		{
			soundFXSignal.Dispatch("Play_main_menu_close_01");
			base.view.Close();
			guestOfHonorService.SelectGuestOfHonor(0);
			StartMinionParty();
		}

		private void StartMinionParty()
		{
			gameContext.injectionBinder.GetInstance<StartMinionPartyIntroSignal>().Dispatch();
		}

		private void Init()
		{
			stopPan.Dispatch();
			base.closeAllOtherMenuSignal.Dispatch(base.gameObject);
			base.view.Init();
		}

		private IEnumerator TransitionOut()
		{
			yield return new WaitForSeconds(3f);
			IGUICommand command = guiService.BuildCommand(GUIOperation.Queue, "screen_GuestOfHonorSelection");
			guiService.Execute(command);
			base.view.Close();
		}

		private void OnMenuClose()
		{
			guiService.Execute(GUIOperation.Unload, "screen_StartPartyPopup");
			if (!haveAPrestigeCharacter)
			{
				hideSkrim.Dispatch("StartPartySkirm");
				showSkipButtonSignal.Dispatch(true);
			}
		}
	}
}
