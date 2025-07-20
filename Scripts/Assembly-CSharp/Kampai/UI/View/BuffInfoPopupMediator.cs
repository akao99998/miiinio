using System.Collections;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using UnityEngine;

namespace Kampai.UI.View
{
	public class BuffInfoPopupMediator : AbstractGenericPopupMediator<BuffInfoPopupView>
	{
		private DummyCharacterObject dummyCharacter;

		private IEnumerator closePopupCoroutine;

		[Inject]
		public IGuestOfHonorService guestOfHonorService { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public BuffInfoPopupClosedSignal buffPopupClosedSignal { get; set; }

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
		}

		public override void Initialize(GUIArguments args)
		{
			int num = playerService.GetMinionPartyInstance().lastGuestsOfHonorPrestigeIDs[0];
			BuffDefinition recentBuffDefinition = guestOfHonorService.GetRecentBuffDefinition();
			if (num != 0 && recentBuffDefinition != null && dummyCharacter == null)
			{
				DummyCharacterType characterType = fancyUIService.GetCharacterType(num);
				dummyCharacter = fancyUIService.CreateCharacter(characterType, DummyCharacterAnimationState.Idle, base.view.MinionSlot.transform, base.view.MinionSlot.VillainScale, base.view.MinionSlot.VillainPositionOffset, num);
				PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(num);
				base.view.SetGuestName(prestigeDefinition.LocalizedKey);
				base.view.SetBuff(recentBuffDefinition, guestOfHonorService.GetCurrentBuffMultipler());
				base.soundFXSignal.Dispatch("Play_menu_popUp_02");
				Vector3 itemCenter = args.Get<Vector3>();
				float yInput = args.Get<float>();
				base.view.SetOffset(yInput, glassCanvas, itemCenter);
				base.view.Open();
				closePopupCoroutine = DelayClose();
				StartCoroutine(closePopupCoroutine);
			}
			else
			{
				hideSkrimSignal.Dispatch("GenericPopup");
				Close();
			}
		}

		internal IEnumerator DelayClose()
		{
			yield return new WaitForSeconds(base.view.Duration);
			hideSkrimSignal.Dispatch("GenericPopup");
			Close();
		}

		public override void Close()
		{
			buffPopupClosedSignal.Dispatch();
			base.Close();
			if (closePopupCoroutine != null)
			{
				StopCoroutine(closePopupCoroutine);
			}
			IGUICommand command = guiService.BuildCommand(GUIOperation.Unload, "screen_BuffPopup");
			guiService.Execute(command);
		}
	}
}
