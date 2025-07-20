using System.Collections;
using Kampai.Game;
using UnityEngine;

namespace Kampai.UI.View
{
	public class MinionPartyCountDownInfoPopupMediator : AbstractGenericPopupMediator<MinionPartyCountDownInfoPopupView>
	{
		private IEnumerator updateRoutine;

		private bool _isCooldownActive;

		private MinionParty minionParty;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			_isCooldownActive = false;
			if (updateRoutine != null)
			{
				StopCoroutine(updateRoutine);
				updateRoutine = null;
			}
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
		}

		public override void Initialize(GUIArguments args)
		{
			base.soundFXSignal.Dispatch("Play_menu_popUp_02");
			Vector3 itemCenter = args.Get<Vector3>();
			Register(null, itemCenter);
		}

		public override void Register(ItemDefinition itemDef, Vector3 itemCenter)
		{
			base.view.Display(itemCenter);
			minionParty = playerService.GetMinionPartyInstance();
			_isCooldownActive = true;
			updateRoutine = UpdateCooldownTime();
			StartCoroutine(updateRoutine);
		}

		internal IEnumerator UpdateCooldownTime()
		{
			while (_isCooldownActive)
			{
				if (base.view == null)
				{
					_isCooldownActive = false;
					continue;
				}
				int timeRemaining = guestService.GetBuffRemainingTime(minionParty);
				if (timeRemaining <= 0)
				{
					_isCooldownActive = false;
					break;
				}
				base.view.UpdateCountDownText(string.Format("{0}", UIUtils.FormatTime(timeRemaining, base.localizationService)));
				yield return new WaitForSeconds(1f);
			}
		}
	}
}
