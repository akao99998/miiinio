using Kampai.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Tools.AnimationToolKit
{
	public class LoadEventSystemCommand : Command
	{
		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject ContextView { get; set; }

		public override void Execute()
		{
			GameObject gameObject = new GameObject("Event System");
			gameObject.transform.parent = ContextView.transform;
			EventSystem o = gameObject.AddComponent<EventSystem>();
			base.injectionBinder.Bind<EventSystem>().ToValue(o);
			gameObject.AddComponent<StandaloneInputModule>();
			gameObject.AddComponent<KampaiTouchInputModule>();
		}
	}
}
