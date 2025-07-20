using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class DisplayMinionSelectedIconCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DisplayMinionSelectedIconCommand") as IKampaiLogger;

		[Inject]
		public int minionId { get; set; }

		[Inject]
		public bool show { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject NamedCharacterManagerView { get; set; }

		public override void Execute()
		{
			NamedCharacterObject namedCharacterObject = NamedCharacterManagerView.GetComponent<NamedCharacterManagerView>().Get(minionId);
			if (namedCharacterObject == null)
			{
				logger.Error("TSM minion does not exist!");
				return;
			}
			Transform transform = namedCharacterObject.transform;
			MinionSelectedIcon componentInChildren = transform.GetComponentInChildren<MinionSelectedIcon>();
			if (show)
			{
				if (componentInChildren != null)
				{
					logger.Warning("Minion Selected Icon already exists!");
					return;
				}
				GameObject gameObject = Object.Instantiate(KampaiResources.Load<GameObject>("MinionSelectedIcon"));
				gameObject.transform.SetParent(transform);
				gameObject.transform.localPosition = Vector3.zero;
			}
			else if (componentInChildren == null)
			{
				logger.Warning("Minion Selected Icon does not exist!");
			}
			else
			{
				Object.Destroy(componentInChildren.gameObject);
			}
		}
	}
}
