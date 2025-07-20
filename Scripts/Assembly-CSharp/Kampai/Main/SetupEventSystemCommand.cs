using Elevation.Logging;
using Kampai.Game;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	internal sealed class SetupEventSystemCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SetupEventSystemCommand") as IKampaiLogger;

		private GameObject eventSystem;

		[Inject]
		public LoadUICompleteSignal uiLoadCompleteSignal { get; set; }

		public override void Execute()
		{
			logger.EventStart("SetupEventSystemCommand.Execute");
			eventSystem = new GameObject("EventSystem");
			eventSystem.AddComponent<EventSystem>();
			eventSystem.AddComponent<StandaloneInputModule>();
			eventSystem.AddComponent<KampaiTouchInputModule>();
			base.injectionBinder.Bind<GameObject>().ToValue(eventSystem).ToName(MainElement.UI_EVENTSYSTEM)
				.CrossContext();
			uiLoadCompleteSignal.AddListener(SetParent);
			logger.EventStop("SetupEventSystemCommand.Execute");
		}

		private void SetParent(GameObject contextView)
		{
			if (!(eventSystem == null))
			{
				eventSystem.transform.parent = contextView.transform;
			}
		}
	}
}
