using UnityEngine;

namespace Kampai.Game.View
{
	public abstract class BuildingDefinitionObject : ActionableObject, FootprintProperties
	{
		public int Width { get; protected set; }

		public int Depth { get; protected set; }

		public bool HasSidewalk { get; protected set; }

		public Vector3 ResourceIconPosition
		{
			get
			{
				return GetResourceIconPosition();
			}
		}

		private Vector3 GetResourceIconPosition()
		{
			Vector3 position = base.transform.position;
			return new Vector3(position.x + (float)Width / 2f, 0f, position.z - (float)Depth / 2f);
		}

		public void Init(Definition definition, IDefinitionService definitionService)
		{
			base.DefinitionID = definition.ID;
			UpdateFootprint(definitionService);
			base.Init();
		}

		private void UpdateFootprint(IDefinitionService definitionService)
		{
			BuildingDefinition buildingDefinition = definitionService.Get<BuildingDefinition>(base.DefinitionID);
			string buildingFootprint = definitionService.GetBuildingFootprint(buildingDefinition.FootprintID);
			Width = BuildingUtil.GetFootprintWidth(buildingFootprint);
			Depth = BuildingUtil.GetFootprintDepth(buildingFootprint);
			HasSidewalk = buildingFootprint.Contains(".");
		}

		public override void OnDefinitionsHotSwap(IDefinitionService definitionService)
		{
			base.OnDefinitionsHotSwap(definitionService);
			UpdateFootprint(definitionService);
		}
	}
}
