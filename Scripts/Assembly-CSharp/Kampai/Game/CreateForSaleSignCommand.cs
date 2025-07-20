using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreateForSaleSignCommand : Command
	{
		private const int AVAILABLE_SIGN_DEFINITION_ID = 3551;

		private const int LOCKED_SIGN_DEFINITION_ID = 3561;

		[Inject]
		public int expansionID { get; set; }

		[Inject]
		public bool available { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILandExpansionConfigService landExpansionConfigService { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.FOR_SALE_SIGN_PARENT)]
		public GameObject parent { get; set; }

		public override void Execute()
		{
			if (!playerService.IsExpansionPurchased(expansionID))
			{
				BuildingDefinition buildingDefinition = ((!available) ? (definitionService.Get(3561) as BuildingDefinition) : (definitionService.Get(3551) as BuildingDefinition));
				if (landExpansionService.HasForSaleSign(expansionID))
				{
					landExpansionService.RemoveForSaleSign(expansionID);
				}
				LandExpansionConfig expansionConfig = landExpansionConfigService.GetExpansionConfig(expansionID);
				if (expansionConfig.routingSlot != null)
				{
					GameObject gameObject = Object.Instantiate(KampaiResources.Load<GameObject>(buildingDefinition.Prefab));
					gameObject.transform.parent = parent.transform;
					gameObject.transform.position = new Vector3(expansionConfig.routingSlot.x, 0f, expansionConfig.routingSlot.y);
					landExpansionService.AddForSaleSign(expansionID, gameObject);
				}
			}
		}
	}
}
