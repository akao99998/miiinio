using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class DropAreaView : KampaiView
	{
		internal Signal<DragDropItemView, bool> OnDragItemOverDropAreaSignal = new Signal<DragDropItemView, bool>();

		internal Signal<DragDropItemView, bool> OnDropItemOverDropAreaSignal = new Signal<DragDropItemView, bool>();

		internal void OnDragItem(DragDropItemView item, PointerEventData pointerEventData)
		{
			GameObject gameObject = pointerEventData.pointerCurrentRaycast.gameObject;
			if (gameObject != null)
			{
				OnDragItemOverDropAreaSignal.Dispatch(item, base.name == gameObject.name);
			}
			else
			{
				OnDragItemOverDropAreaSignal.Dispatch(item, false);
			}
		}

		internal void OnEndDragItem(DragDropItemView item, PointerEventData pointerEventData)
		{
			GameObject gameObject = pointerEventData.pointerCurrentRaycast.gameObject;
			if (gameObject != null)
			{
				OnDropItemOverDropAreaSignal.Dispatch(item, base.name == gameObject.name);
			}
			else
			{
				OnDropItemOverDropAreaSignal.Dispatch(item, false);
			}
		}
	}
}
