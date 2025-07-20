using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using strange.extensions.signal.impl;

namespace Kampai.BuildingsSizeToolbox
{
	public class BuildingsSizeToolkitValueUpdateButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler
	{
		public float Sign = 1f;

		public float WaitBeforeRepeat = 0.5f;

		public float RepeatRate = 60f;

		public Signal<float> UpdateValueSignal = new Signal<float>();

		private Coroutine repeatCoroutine;

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			if (repeatCoroutine == null)
			{
				repeatCoroutine = StartCoroutine(buttonPressedCoroutine());
			}
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			if (repeatCoroutine != null)
			{
				StopCoroutine(repeatCoroutine);
				repeatCoroutine = null;
			}
		}

		private IEnumerator buttonPressedCoroutine()
		{
			UpdateValueSignal.Dispatch(Sign);
			yield return new WaitForSeconds(WaitBeforeRepeat);
			UpdateValueSignal.Dispatch(Sign);
			while (true)
			{
				yield return new WaitForSeconds(1f / RepeatRate);
				UpdateValueSignal.Dispatch(Sign);
			}
		}
	}
}
