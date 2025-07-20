using System;
using Kampai.Common;
using Kampai.Main;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kampai.Game.Mignette
{
	public class MignetteService : IPickService, IMignetteService
	{
		private EventSystem eventSystem;

		private Action<Vector3, int, bool> inputEvent;

		[Inject(MainElement.UI_EVENTSYSTEM)]
		public GameObject uiEventSystem { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			eventSystem = uiEventSystem.GetComponent<EventSystem>();
		}

		public void RegisterListener(Action<Vector3, int, bool> obj)
		{
			inputEvent = (Action<Vector3, int, bool>)Delegate.Combine(inputEvent, obj);
		}

		public void UnregisterListener(Action<Vector3, int, bool> obj)
		{
			inputEvent = (Action<Vector3, int, bool>)Delegate.Remove(inputEvent, obj);
		}

		public void OnGameInput(Vector3 inputPosition, int input, bool pressed)
		{
			if (inputEvent != null && !eventSystem.IsPointerOverGameObject())
			{
				inputEvent(inputPosition, input, pressed);
			}
		}

		public void SetIgnoreInstanceInput(int instanceId, bool isIgnored)
		{
			throw new NotImplementedException();
		}

		public PickState GetPickState()
		{
			throw new NotImplementedException();
		}
	}
}
