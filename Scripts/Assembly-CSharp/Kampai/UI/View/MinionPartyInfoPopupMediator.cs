using Kampai.Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kampai.UI.View
{
	public class MinionPartyInfoPopupMediator : AbstractGenericPopupMediator<MinionPartyInfoPopupView>
	{
		[Inject]
		public SetXPSignal setXPSignal { get; set; }

		[Inject]
		public ExtendItemPopupSignal extendPopupSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
			setXPSignal.AddListener(SetPartyPoints);
			base.view.pointerDownSignal.AddListener(PointerDown);
			base.view.pointerUpSignal.AddListener(PointerUp);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			setXPSignal.RemoveListener(SetPartyPoints);
			base.view.pointerDownSignal.RemoveListener(PointerDown);
			base.view.pointerUpSignal.RemoveListener(PointerUp);
		}

		public override void Initialize(GUIArguments args)
		{
			base.soundFXSignal.Dispatch("Play_menu_popUp_02");
			Vector3 itemCenter = args.Get<Vector3>();
			SetPartyPoints();
			Register(null, itemCenter);
		}

		public override void Register(ItemDefinition itemDef, Vector3 itemCenter)
		{
			base.view.Display(itemCenter);
			SetPartyPoints(false);
		}

		private void SetPartyPoints()
		{
			SetPartyPoints(true);
		}

		internal void SetPartyPoints(bool animate)
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			base.view.SetPartyPoints(minionPartyInstance.CurrentPartyPoints, minionPartyInstance.CurrentPartyPointsRequired, animate);
		}

		private void PointerDown(PointerEventData eventData)
		{
			extendPopupSignal.Dispatch();
		}

		private void PointerUp(PointerEventData eventData)
		{
		}
	}
}
