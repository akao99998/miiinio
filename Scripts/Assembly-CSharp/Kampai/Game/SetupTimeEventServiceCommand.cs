using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	internal sealed class SetupTimeEventServiceCommand : Command
	{
		[Inject]
		public GameObject contextView { get; set; }

		public override void Execute()
		{
			GameObject gameObject = new GameObject("TimeEventService");
			TimeEventService o = gameObject.AddComponent<TimeEventService>();
			base.injectionBinder.Bind<ITimeEventService>().ToValue(o).CrossContext()
				.Weak();
			gameObject.transform.parent = contextView.transform;
		}
	}
}
