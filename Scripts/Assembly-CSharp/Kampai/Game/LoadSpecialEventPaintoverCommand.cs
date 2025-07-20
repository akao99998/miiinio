using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class LoadSpecialEventPaintoverCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadSpecialEventPaintoverCommand") as IKampaiLogger;

		[Inject]
		public SpecialEventItemDefinition specialEventItemDefinition { get; set; }

		[Inject(GameElement.SPECIAL_EVENT_PARENT)]
		public GameObject parent { get; set; }

		public override void Execute()
		{
			string paintover = specialEventItemDefinition.Paintover;
			GameObject gameObject = KampaiResources.Load<GameObject>(paintover);
			if (gameObject == null)
			{
				logger.Debug("Unable to load Special_Event paintover prefab");
				return;
			}
			GameObject gameObject2 = Object.Instantiate(gameObject);
			if (gameObject2 == null)
			{
				logger.Debug("Unable to instantiate Special_Event paintover object");
			}
			else
			{
				gameObject2.transform.parent = parent.transform;
			}
		}
	}
}
