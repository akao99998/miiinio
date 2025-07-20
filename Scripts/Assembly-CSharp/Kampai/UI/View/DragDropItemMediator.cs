using Kampai.Main;
using UnityEngine.EventSystems;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class DragDropItemMediator : Mediator
	{
		[Inject]
		public DragDropItemView View { get; set; }

		[Inject]
		public OnDragItemSignal OnDragItemSignal { get; set; }

		[Inject]
		public OnDropItemSignal OnDropItemSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		public override void OnRegister()
		{
			View.OnDragSignal.AddListener(OnDrag);
			View.OnDropSignal.AddListener(OnDrop);
			View.OnStartSignal.AddListener(OnStart);
			View.Init();
		}

		public override void OnRemove()
		{
			View.OnDragSignal.RemoveListener(OnDrag);
			View.OnDropSignal.RemoveListener(OnDrop);
			View.OnStartSignal.AddListener(OnStart);
		}

		private void OnStart(PointerEventData eventData)
		{
			playSFXSignal.Dispatch("Play_pick_item_01");
		}

		private void OnDrag(PointerEventData eventData)
		{
			OnDragItemSignal.Dispatch(View, eventData);
		}

		private void OnDrop(PointerEventData eventData)
		{
			playSFXSignal.Dispatch("Play_place_item_01");
			OnDropItemSignal.Dispatch(View, eventData);
		}
	}
}
