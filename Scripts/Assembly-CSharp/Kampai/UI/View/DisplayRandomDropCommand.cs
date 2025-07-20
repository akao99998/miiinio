using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class DisplayRandomDropCommand : Command
	{
		private Vector3 offset = new Vector3(1.5f, 4f, 0f);

		[Inject]
		public Tuple<int, int> values { get; set; }

		[Inject(MainElement.UI_WORLDCANVAS)]
		public GameObject worldCanvas { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(values.Item2);
			if (byInstanceId != null)
			{
				GameObject gameObject = Object.Instantiate(KampaiResources.Load("HarvestButton")) as GameObject;
				RandomDropView component = gameObject.GetComponent<RandomDropView>();
				component.ItemDefinitionId = values.Item1;
				Transform transform = gameObject.transform;
				transform.SetParent(worldCanvas.transform, false);
				transform.position = new Vector3((float)byInstanceId.Location.x + offset.x, offset.y, byInstanceId.Location.y);
				KampaiImage image = component.image;
				ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(values.Item1);
				image.sprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
				image.maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
			}
		}
	}
}
