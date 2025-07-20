using UnityEngine.EventSystems;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class DropAreaMediator : Mediator
	{
		[Inject]
		public DropAreaView DropAreaView { get; set; }

		[Inject]
		public OnDragItemOverDropAreaSignal OnDragItemOverDropAreaSignal { get; set; }

		[Inject]
		public OnDropItemOverDropAreaSignal OnDropItemOverDropAreaSignal { get; set; }

		[Inject]
		public OnDragItemSignal OnDragItemSignal { get; set; }

		[Inject]
		public OnDropItemSignal OnDropItemSignal { get; set; }

		public override void OnRegister()
		{
			OnDragItemSignal.AddListener(OnDragItem);
			OnDropItemSignal.AddListener(OnDropItem);
			DropAreaView.OnDragItemOverDropAreaSignal.AddListener(OnDragItemOverDropArea);
			DropAreaView.OnDropItemOverDropAreaSignal.AddListener(OnDropItemOverDropArea);
		}

		public override void OnRemove()
		{
			OnDragItemSignal.RemoveListener(OnDragItem);
			OnDropItemSignal.RemoveListener(OnDropItem);
			DropAreaView.OnDragItemOverDropAreaSignal.RemoveListener(OnDragItemOverDropArea);
			DropAreaView.OnDropItemOverDropAreaSignal.RemoveListener(OnDropItemOverDropArea);
		}

		private void OnDragItem(DragDropItemView item, PointerEventData data)
		{
			DropAreaView.OnDragItem(item, data);
		}

		private void OnDropItem(DragDropItemView item, PointerEventData data)
		{
			DropAreaView.OnEndDragItem(item, data);
		}

		private void OnDropItemOverDropArea(DragDropItemView item, bool successful)
		{
			OnDropItemOverDropAreaSignal.Dispatch(item, successful);
		}

		private void OnDragItemOverDropArea(DragDropItemView item, bool over)
		{
			OnDragItemOverDropAreaSignal.Dispatch(item, over);
		}
	}
}
